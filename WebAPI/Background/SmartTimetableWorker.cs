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
/// Background service to generate and update smart timetables for users based on urgency, workload, and preferences.
/// </summary>
public class SmartTimetableWorker : BackgroundService
{
    private readonly ILogger<SmartTimetableWorker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public SmartTimetableWorker(
        ILogger<SmartTimetableWorker> logger,
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
                    // Example: Regenerate timetable every morning
                    if (DateTime.UtcNow.Hour == 4) // 4 AM UTC
                    {
                        _logger.LogInformation("Smart timetable generation for {UserId}", u.Id);
                        await scheduler.RescheduleAsync(u.Id, stoppingToken);
                    }
                }
            }
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}
