using Application.IService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace Application.Services;

/// <summary>
/// My AI service that integrates with OpenAI to provide smart task suggestions, user pattern analysis, and coaching insights.
/// </summary>
public class AIService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AIService> _logger;
    private readonly string _apiKey;

    public AIService(HttpClient httpClient, IConfiguration configuration, ILogger<AIService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiKey = configuration["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI API key not configured");
        
        _httpClient.BaseAddress = new Uri("https://api.openai.com/v1/");
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
    }

    public async Task<string> GetTaskSuggestionAsync(string taskDescription)
    {
        try
        {
            _logger.LogInformation("Generating AI task suggestion for: {TaskDescription}", taskDescription);

            var requestBody = new
            {
                model = "gpt-4o-mini",
                messages = new[]
                {
                    new { role = "system", content = "You are a productivity coach AI. Analyze the given task and provide helpful suggestions for breaking it down, prioritizing it, or optimizing how to approach it. Keep responses concise and actionable." },
                    new { role = "user", content = $"Task: {taskDescription}" }
                },
                max_tokens = 500,
                temperature = 0.7
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("chat/completions", content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(responseJson);
            
            var suggestion = result.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

            _logger.LogInformation("AI task suggestion generated successfully");
            return suggestion ?? "Unable to generate suggestion";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating task suggestion for: {TaskDescription}", taskDescription);
            return "Unable to generate AI suggestion at this time. Please try again later.";
        }
    }

    public async Task<string> AnalyzeUserPatternsAsync(Guid userId)
    {
        try
        {
            _logger.LogInformation("Analyzing user patterns for user: {UserId}", userId);

            var requestBody = new
            {
                model = "gpt-4o-mini",
                messages = new[]
                {
                    new { role = "system", content = "You are a behavioral analysis AI. Based on user activity patterns, provide insights about productivity trends, potential procrastination triggers, and recommendations for improvement. Keep responses helpful and non-judgmental." },
                    new { role = "user", content = $"Analyze patterns for user ID: {userId}. Provide general productivity insights and recommendations." }
                },
                max_tokens = 500,
                temperature = 0.7
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("chat/completions", content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(responseJson);
            
            var analysis = result.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

            _logger.LogInformation("User pattern analysis completed for user: {UserId}", userId);
            return analysis ?? "Unable to analyze patterns";
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
            _logger.LogInformation("Generating weekly plan for user: {UserId}, week starting: {WeekStart}", userId, weekStart);

            var requestBody = new
            {
                model = "gpt-4o-mini",
                messages = new[]
                {
                    new { role = "system", content = "You are a weekly planning AI assistant. Create a structured weekly plan that balances productivity, personal development, and well-being. Provide specific daily recommendations and time management tips." },
                    new { role = "user", content = $"Generate a weekly plan for user {userId} starting on {weekStart:yyyy-MM-dd}. Include daily structure, productivity tips, and goal-focused activities." }
                },
                max_tokens = 800,
                temperature = 0.7
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("chat/completions", content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<JsonElement>(responseJson);
            
            var weeklyPlan = result.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

            _logger.LogInformation("Weekly plan generated successfully for user: {UserId}", userId);
            return weeklyPlan ?? "Unable to generate weekly plan";
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
            _logger.LogInformation("Processing reflection for user: {UserId}", userId);

            var requestBody = new
            {
                model = "gpt-4o-mini",
                messages = new[]
                {
                    new { role = "system", content = "You are a mindful reflection coach. Help users process their thoughts, identify insights, and find clarity. Respond with empathy, ask thoughtful questions, and provide gentle guidance for personal growth." },
                    new { role = "user", content = $"User reflection: {input}" }
                },
                max_tokens = 600,
                temperature = 0.8
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("chat/completions", content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<JsonElement>(responseJson);
            
            var reflection = result.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

            _logger.LogInformation("Reflection processed successfully for user: {UserId}", userId);
            return reflection ?? "Unable to process reflection";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing reflection for user: {UserId}", userId);
            return "Unable to process reflection at this time. Please try again later.";
        }
    }
}
