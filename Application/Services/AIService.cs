using Application.IRepositories;
using Application.IService;
using Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace Application.Services;

/// <summary>
/// OpenAI-backed coaching service. Every call degrades gracefully: without a
/// configured API key (or when OpenAI fails) it serves a deterministic local
/// response derived from the user's real data instead of throwing, so the API
/// surface stays useful offline. When a key is present, the same real data is
/// compiled into the prompt so the model coaches on actual activity rather
/// than a bare user id.
/// </summary>
public class AIService : IAIService
{
    private const string DefaultApiBaseAddress = "https://api.openai.com/v1/";

    private const string TaskSuggestionSystemPrompt =
        "You are a productivity coach AI. Analyze the given task and provide helpful suggestions for breaking it down, prioritizing it, or optimizing how to approach it. Keep responses concise and actionable.";
    private const string PatternAnalysisSystemPrompt =
        "You are a behavioral analysis AI. Based on user activity patterns, provide insights about productivity trends, potential procrastination triggers, and recommendations for improvement. Keep responses helpful and non-judgmental.";
    private const string WeeklyPlanSystemPrompt =
        "You are a weekly planning AI assistant. Create a structured weekly plan that balances productivity, personal development, and well-being. Provide specific daily recommendations and time management tips.";
    private const string ReflectionSystemPrompt =
        "You are a mindful reflection coach. Help users process their thoughts, identify insights, and find clarity. Respond with empathy, ask thoughtful questions, and provide gentle guidance for personal growth.";

    private readonly HttpClient _httpClient;
    private readonly ILogger<AIService> _logger;
    private readonly ITaskRepository _taskRepository;
    private readonly IGoalRepository _goalRepository;
    private readonly IHabitRepository _habitRepository;
    private readonly IEnergyMoodRepository _energyMoodRepository;
    private readonly bool _isConfigured;
    private readonly string _model;
    private readonly string _chatEndpoint;

    public AIService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<AIService> logger,
        ITaskRepository taskRepository,
        IGoalRepository goalRepository,
        IHabitRepository habitRepository,
        IEnergyMoodRepository energyMoodRepository)
    {
        _httpClient = httpClient;
        _logger = logger;
        _taskRepository = taskRepository;
        _goalRepository = goalRepository;
        _habitRepository = habitRepository;
        _energyMoodRepository = energyMoodRepository;

        var apiKey = configuration["OpenAI:ApiKey"];
        // A missing or empty key is a valid dev/test state: the service still
        // resolves and serves data-derived local insights. Only a present key
        // enables HTTP calls.
        _isConfigured = !string.IsNullOrWhiteSpace(apiKey);

        // Configuration is the source of truth for endpoint and model so
        // deployments can point at a gateway or switch models without a
        // rebuild; the constants below are only fallbacks.
        var endpoint = configuration["OpenAI:Endpoint"];
        _chatEndpoint = string.IsNullOrWhiteSpace(endpoint)
            ? $"{DefaultApiBaseAddress}chat/completions"
            : endpoint;

        var model = configuration["OpenAI:Model"];
        _model = string.IsNullOrWhiteSpace(model) ? "gpt-4o-mini" : model;

        _httpClient.BaseAddress = new Uri(DefaultApiBaseAddress);
        if (_isConfigured)
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
        else
            _logger.LogInformation("OpenAI API key not configured; AI endpoints will return local data-derived insights.");
    }

    public async Task<string> GetTaskSuggestionAsync(string taskDescription, CancellationToken cancellationToken = default)
    {
        try
        {
            var suggestion = await CallChatAsync(
                TaskSuggestionSystemPrompt,
                $"Task: {taskDescription}",
                maxTokens: 500,
                temperature: 0.7,
                cancellationToken);
            return suggestion ?? "Unable to generate AI suggestion at this time. Please try again later.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating task suggestion for: {TaskDescription}", taskDescription);
            return "Unable to generate AI suggestion at this time. Please try again later.";
        }
    }

    public async Task<string> AnalyzeUserPatternsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var context = await BuildUserContextAsync(userId, cancellationToken);

            var analysis = await CallChatAsync(
                PatternAnalysisSystemPrompt,
                $"Productivity data for the last 7 days:\n{context}\nProvide insights about productivity trends, procrastination triggers, and recommendations.",
                maxTokens: 500,
                temperature: 0.7,
                cancellationToken);

            return analysis ?? LocalInsightFallback(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing patterns for user: {UserId}", userId);
            return "Unable to analyze user patterns at this time. Please try again later.";
        }
    }

    public async Task<string> GenerateWeeklyPlanAsync(Guid userId, DateTime weekStart, CancellationToken cancellationToken = default)
    {
        try
        {
            var weekEnd = weekStart.AddDays(7);
            var context = await BuildUserContextAsync(userId, cancellationToken, weekStart, weekEnd);

            var plan = await CallChatAsync(
                WeeklyPlanSystemPrompt,
                $"Week starting {weekStart:yyyy-MM-dd}. Current productivity data:\n{context}\nCreate the weekly plan.",
                maxTokens: 800,
                temperature: 0.7,
                cancellationToken);

            return plan ?? LocalWeeklyPlanFallback(weekStart, context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating weekly plan for user: {UserId}", userId);
            return "Unable to generate weekly plan at this time. Please try again later.";
        }
    }

    public async Task<string> ReflectAsync(Guid userId, string input, CancellationToken cancellationToken = default)
    {
        try
        {
            var reflection = await CallChatAsync(
                ReflectionSystemPrompt,
                $"User reflection: {input}",
                maxTokens: 600,
                temperature: 0.8,
                cancellationToken);
            return reflection ?? "Unable to process reflection at this time. Please try again later.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing reflection for user: {UserId}", userId);
            return "Unable to process reflection at this time. Please try again later.";
        }
    }

    /// <summary>
    /// Compiles the user's real activity into a compact text block: open and
    /// recently-completed tasks, active goals (optionally only those with
    /// target dates inside the requested week), active habits, and 7-day
    /// energy/mood averages. Shared by the LLM prompts and the offline
    /// fallbacks so both coach on the same facts.
    /// </summary>
    private async Task<string> BuildUserContextAsync(
        Guid userId,
        CancellationToken cancellationToken,
        DateTime? weekStart = null,
        DateTime? weekEnd = null)
    {
        var activeTasks = (await _taskRepository.GetActiveUserTasksAsync(userId)).ToList();
        var recentTasks = (await _taskRepository.GetRecentTasksByUserAsync(userId, 7)).ToList();
        var completedLast7Days = recentTasks.Count(t => t.CompletedAt >= DateTime.UtcNow.AddDays(-7));

        var goals = (await _goalRepository.GetActiveUserGoalsAsync(userId)).ToList();
        if (weekStart.HasValue && weekEnd.HasValue)
            goals = goals.Where(g => g.TargetDate >= weekStart.Value && g.TargetDate < weekEnd.Value).ToList();

        var habits = (await _habitRepository.GetActiveUserHabitsAsync(userId)).ToList();

        var energyEntries = (await _energyMoodRepository.GetByUserAsync(userId, DateTime.UtcNow.AddDays(-7), null)).ToList();

        var sb = new StringBuilder();
        sb.Append($"Open tasks: {activeTasks.Count}");
        var highPriority = activeTasks.Count(t => t.Priority == TaskPriority.High);
        if (highPriority > 0)
            sb.Append($" (high priority: {highPriority})");
        sb.AppendLine($". Completed in the last 7 days: {completedLast7Days}.");

        sb.Append("Active goals");
        if (weekStart.HasValue && weekEnd.HasValue)
            sb.Append(" with target dates this week");
        sb.Append(": ");
        sb.AppendLine(goals.Count == 0
            ? "none."
            : string.Join(", ", goals.Take(5).Select(g => $"\"{g.Title}\"")) + (goals.Count > 5 ? "…" : "."));

        sb.Append("Active habits: ");
        sb.AppendLine(habits.Count == 0
            ? "none."
            : string.Join(", ", habits.Take(5).Select(h => $"\"{h.Name}\"")) + (habits.Count > 5 ? "…" : "."));

        if (energyEntries.Count > 0)
        {
            var avgEnergy = energyEntries.Average(e => e.EnergyLevel);
            var avgMood = energyEntries.Average(e => e.MoodLevel);
            sb.AppendLine($"Energy/mood over the last 7 days ({energyEntries.Count} entries): energy {avgEnergy:F1}/10, mood {avgMood:F1}/10.");
        }
        else
        {
            sb.AppendLine("No energy/mood entries in the last 7 days.");
        }

        return sb.ToString().TrimEnd();
    }

    /// <summary>
    /// Offline substitute for pattern analysis: the same real numbers, restated
    /// as plain observations. The text says it came from the local fallback.
    /// </summary>
    private static string LocalInsightFallback(string context)
        => $"Local insights (offline mode. Configure an OpenAI API key for personalized coaching):\n{context}\nPrioritize the high-priority open tasks first, and keep logging energy/mood so trends become visible.";

    /// <summary>
    /// Offline substitute for the weekly plan: points the user at their real
    /// this-week goals and open workload instead of inventing a schedule.
    /// </summary>
    private static string LocalWeeklyPlanFallback(DateTime weekStart, string context)
        => $"Local weekly outline for the week of {weekStart:yyyy-MM-dd} (offline mode. Configure an OpenAI API key for a detailed plan):\n{context}\nBlock time for the highest-priority open tasks early in the week, and reserve a review slot before {weekStart.AddDays(6):yyyy-MM-dd}.";

    /// <summary>
    /// The single OpenAI chat-completions call used by every public method.
    /// Returns null when no API key is configured so callers can serve a
    /// fallback without making a request that would fail.
    /// </summary>
    private async Task<string?> CallChatAsync(string systemPrompt, string userPrompt, int maxTokens, double temperature, CancellationToken cancellationToken)
    {
        if (!_isConfigured)
            return null;

        var requestBody = new
        {
            model = _model,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            },
            max_tokens = maxTokens,
            temperature
        };

        var json = JsonSerializer.Serialize(requestBody);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(_chatEndpoint, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<JsonElement>(responseJson);
        return result.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
    }
}
