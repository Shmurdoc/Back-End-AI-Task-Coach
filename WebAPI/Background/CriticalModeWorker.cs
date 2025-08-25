using Application.IRepositories;
using Application.IService;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Domain.Enums;

namespace WebAPI.Background;

public class CriticalModeWorker : BackgroundService
{
    private readonly ILogger<CriticalModeWorker> _logger;
    private readonly IUserRepository _users;
    private readonly ITaskRepository _tasks;
    private readonly IGamificationService _gamification;
    private readonly IAdaptiveSchedulingEngine _scheduler;

    public CriticalModeWorker(ILogger<CriticalModeWorker> logger, IUserRepository users, ITaskRepository tasks, IGamificationService gamification, IAdaptiveSchedulingEngine scheduler)
    { _logger = logger; _users = users; _tasks = tasks; _gamification = gamification; _scheduler = scheduler; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var users = await _users.GetActiveUsersAsync();
            foreach (var u in users)
            {
                var relapse = await _gamification.DetectRelapseAsync(u.Id, stoppingToken);
                var active = await _tasks.GetActiveUserTasksAsync(u.Id);
                var criticalOverdue = active.Count(t => (t.EndTime ?? t.StartTime) < DateTime.UtcNow && t.Priority >= TaskPriority.High);
                if (relapse || criticalOverdue >= 3)
                {
                    _logger.LogWarning("Critical Mode activated for {UserId}", u.Id);
                    await _scheduler.RescheduleAsync(u.Id, stoppingToken);
                }
            }
            await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
        }
    }
}
