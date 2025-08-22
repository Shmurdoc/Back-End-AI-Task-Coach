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

    public NudgeOrchestrator(IUserRepository users, ITaskRepository tasks, IAIService ai, INotificationService notify, ILogger<NudgeOrchestrator> logger)
    {
        _users = users; _tasks = tasks; _ai = ai; _notify = notify; _logger = logger;
    }

    public async Task<int> ScanAndSendNudgesAsync(CancellationToken ct = default)
    {
        var delivered = 0;
        var now = DateTime.UtcNow;
        var users = await _users.GetActiveUsersAsync();
        foreach (var u in users)
        {
            var tasks = await _tasks.GetActiveUserTasksAsync(u.Id);
            var overdue = tasks.Where(t => t.EndTime != null && t.EndTime < now && t.Status != Domain.Enums.TaskItemStatus.Completed).ToList();
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
