using Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace WebAPI.Extensions;

/// <summary>
/// Applies EF Core migrations at startup with a small retry loop, so a database
/// container that is still booting cannot crash the API host.
/// </summary>
public static class DatabaseMigrationExtensions
{
    public static async Task MigrateDatabaseAsync(this WebApplication app, int maxAttempts = 10, int delaySeconds = 3)
    {
        using var scope = app.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (!context.Database.IsRelational())
        {
            logger.LogInformation("Database provider is not relational (likely an integration test); skipping migrations.");
            return;
        }

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await context.Database.MigrateAsync();
                logger.LogInformation("Database migrations applied successfully.");
                return;
            }
            catch (Exception ex) when (attempt < maxAttempts)
            {
                logger.LogWarning(ex,
                    "Database migration attempt {Attempt}/{MaxAttempts} failed; retrying in {Delay} seconds.",
                    attempt, maxAttempts, delaySeconds);
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
            }
        }
    }
}
