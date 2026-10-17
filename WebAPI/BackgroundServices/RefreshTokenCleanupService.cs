using Application.IRepositories;
using Prometheus;

namespace WebAPI.BackgroundServices;

/// <summary>
/// Removes expired refresh tokens (and revoked ones older than a short grace
/// period, so reuse detection still works right after a rotation) every six
/// hours. The first run waits a few minutes so a booting deployment does not
/// race migrations; failures are logged and retried on the next cycle.
/// </summary>
public class RefreshTokenCleanupService : BackgroundService
{
    private static readonly TimeSpan StartupDelay = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan Interval = TimeSpan.FromHours(6);
    private const int RevokedRetentionHours = 24;
    private const int BatchSize = 500;
    private const string JobName = "refresh_token_cleanup";

    private static readonly Counter JobsExecuted = Metrics.CreateCounter(
        "background_jobs_executed_total",
        "Background jobs that completed successfully.",
        new CounterConfiguration { LabelNames = new[] { "job" } });

    private static readonly Counter JobsFailed = Metrics.CreateCounter(
        "background_jobs_failed_total",
        "Background jobs that threw an exception.",
        new CounterConfiguration { LabelNames = new[] { "job" } });

    private static readonly Histogram JobDuration = Metrics.CreateHistogram(
        "background_job_duration_seconds",
        "Background job execution time in seconds.",
        new HistogramConfiguration { LabelNames = new[] { "job" } });

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RefreshTokenCleanupService> _logger;

    public RefreshTokenCleanupService(IServiceScopeFactory scopeFactory, ILogger<RefreshTokenCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(StartupDelay, stoppingToken);

        using var timer = new PeriodicTimer(Interval);
        do
        {
            await RunCleanupAsync(stoppingToken);
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task RunCleanupAsync(CancellationToken cancellationToken)
    {
        using var durationTimer = JobDuration.WithLabels(JobName).NewTimer();

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IRefreshTokenRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var now = DateTime.UtcNow;
            var staleTokens = await repository.GetStaleAsync(now, now.AddHours(-RevokedRetentionHours), BatchSize);

            if (staleTokens.Count > 0)
            {
                repository.RemoveRange(staleTokens);
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }

            JobsExecuted.WithLabels(JobName).Inc();
            _logger.LogInformation("Refresh token cleanup removed {Count} stale token(s).", staleTokens.Count);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            JobsFailed.WithLabels(JobName).Inc();
            _logger.LogError(ex, "Refresh token cleanup failed; will retry on the next cycle.");
        }
    }
}
