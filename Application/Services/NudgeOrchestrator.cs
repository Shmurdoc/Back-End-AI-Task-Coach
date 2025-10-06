
using Application.IRepositories;
using Application.IService;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class NudgeOrchestrator : INudgeOrchestrator
{
    private readonly IUserRepository _users;
    private readonly ITaskRepository _tasks;
    private readonly IAIService _ai;
    private readonly INotificationService _notify;
    private readonly ILogger<NudgeOrchestrator> _logger;

    public NudgeOrchestrator(
        IUserRepository users,
        ITaskRepository tasks,
        IAIService ai,
        INotificationService notify,
        ILogger<NudgeOrchestrator> logger)
    {
        _users = users;
        _tasks = tasks;
        _ai = ai;
        _notify = notify;
        _logger = logger;
    }

    public async Task OrchestrateNudgeAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Orchestrate nudges for a single user: find incomplete and overdue tasks
        var now = DateTime.UtcNow;
        var user = await _users.GetByIdAsync(userId);
        if (user == null) return;
        var tasks = await _tasks.GetActiveUserTasksAsync(userId);
        var overdue = tasks
            .Where(t => t.CompletedAt == null && t.Status != Domain.Enums.TaskItemStatus.Completed && t.StartedAt != null && t.StartedAt < now)
            .ToList();
        foreach (var t in overdue.Take(50))
        {
            var suggestion = await _ai.GetTaskSuggestionAsync($"{t.Title}: {t.Description}");
            await _notify.SendAsync(user, $"Nudge: {t.Title}", suggestion, cancellationToken);
        }
        _logger.LogInformation("Orchestrated nudges for user: {UserId}", userId);
    }

    public async Task<int> ScanAndSendNudgesAsync(CancellationToken ct = default)
    {
        var delivered = 0;
        var now = DateTime.UtcNow;
        var users = await _users.GetActiveUsersAsync();
        foreach (var u in users)
        {
            var tasks = await _tasks.GetActiveUserTasksAsync(u.Id);
            var overdue = tasks
                .Where(t => t.CompletedAt == null && t.Status != Domain.Enums.TaskItemStatus.Completed && t.StartedAt != null && t.StartedAt < now)
                .ToList();
            foreach (var t in overdue.Take(50))
            {
                var suggestion = await _ai.GetTaskSuggestionAsync($"{t.Title}: {t.Description}");
                var ok = await _notify.SendAsync(u, $"Nudge: {t.Title}", suggestion, ct);
                if (ok) delivered++;
            }
        }
        _logger.LogInformation("Nudges delivered: {Count}", delivered);
        return delivered;
    }
}
