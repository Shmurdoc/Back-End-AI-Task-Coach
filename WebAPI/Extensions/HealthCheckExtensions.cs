using Infrastructure.Persistence.Data;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

namespace WebAPI.Extensions;

public static class HealthCheckExtensions
{
    /// <summary>
    /// Registers liveness and readiness checks. Liveness ("live") reports the
    /// process itself and has no dependencies; readiness ("ready") reports
    /// dependencies required to serve requests, currently the database.
    /// </summary>
    public static IServiceCollection AddApiHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy("API process is running."), tags: new[] { "live" })
            .AddCheck<DatabaseHealthCheck>("database", tags: new[] { "ready" });
        return services;
    }

    public static void MapApiHealthChecks(this WebApplication app)
    {
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("live"),
            ResponseWriter = WriteJsonResponse
        });

        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = WriteJsonResponse
        });
    }

    private static Task WriteJsonResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";
        var payload = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                description = entry.Value.Description
            }),
            totalDurationMs = report.TotalDuration.TotalMilliseconds
        };
        return context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}

/// <summary>
/// Readiness check for the primary datastore. CanConnectAsync is a lightweight
/// round-trip for relational providers; non-relational providers (the InMemory
/// database used by tests) are treated as ready.
/// </summary>
public sealed class DatabaseHealthCheck : IHealthCheck
{
    private readonly ApplicationDbContext _dbContext;

    public DatabaseHealthCheck(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_dbContext.Database.IsRelational())
            {
                return HealthCheckResult.Healthy("Non-relational provider; connectivity check skipped.");
            }

            return await _dbContext.Database.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy("Database is reachable.")
                : HealthCheckResult.Unhealthy("Database is not reachable.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database connectivity check failed.", ex);
        }
    }
}
