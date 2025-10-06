using Application.IRepositories;
using Application.IService;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WebAPI.Background;

/// <summary>
/// Background service to detect user inactivity and trigger procrastination recovery protocols.
/// </summary>
public class ProcrastinationRecoveryWorker : BackgroundService
{
    private readonly ILogger<ProcrastinationRecoveryWorker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public ProcrastinationRecoveryWorker(
        ILogger<ProcrastinationRecoveryWorker> logger,
        IServiceScopeFactory scopeFactory)
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
                var scheduler = scope.ServiceProvider.GetRequiredService<IAdaptiveSchedulingEngine>();
                var users = await usersRepo.GetActiveUsersAsync();
                foreach (var u in users)
                {
                    // Example: If user has not completed any tasks in 3 days, trigger recovery
                    var tasks = await tasksRepo.GetActiveUserTasksAsync(u.Id);
                    var lastCompleted = tasks
                        .Where(t => t.CompletedAt != null)
                        .OrderByDescending(t => t.CompletedAt)
                        .FirstOrDefault();
                    if (lastCompleted == null || (lastCompleted.CompletedAt == null) || (DateTime.UtcNow - lastCompleted.CompletedAt.Value).TotalDays > 3)
                    {
                        _logger.LogInformation("Procrastination recovery triggered for {UserId}", u.Id);
                        await scheduler.RescheduleAsync(u.Id, stoppingToken);
                    }
                }
            }
            await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
        }
    }
}
