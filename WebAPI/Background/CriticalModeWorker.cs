using Application.IRepositories;
using Application.IService;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Domain.Enums;

namespace WebAPI.Background;

public class CriticalModeWorker : BackgroundService
{
    private readonly ILogger<CriticalModeWorker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public CriticalModeWorker(ILogger<CriticalModeWorker> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var usersRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
                var tasksRepo = scope.ServiceProvider.GetRequiredService<ITaskRepository>();
                var gamification = scope.ServiceProvider.GetRequiredService<IGamificationService>();
                var scheduler = scope.ServiceProvider.GetRequiredService<IAdaptiveSchedulingEngine>();
                var users = await usersRepo.GetActiveUsersAsync();
                foreach (var u in users)
                {
                    var relapse = await gamification.DetectRelapseAsync(u.Id, stoppingToken);
                    var active = await tasksRepo.GetActiveUserTasksAsync(u.Id);
                    var criticalOverdue = active.Count(t => (t.EndTime ?? t.StartTime) < DateTime.UtcNow && t.Priority >= TaskPriority.High);
                    if (relapse || criticalOverdue >= 3)
                    {
                        _logger.LogWarning("Critical Mode activated for {UserId}", u.Id);
                        await scheduler.RescheduleAsync(u.Id, stoppingToken);
                    }
                }
            }
            await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
        }
    }
}
