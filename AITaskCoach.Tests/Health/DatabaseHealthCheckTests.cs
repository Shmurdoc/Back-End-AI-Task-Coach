using Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using WebAPI.Extensions;

namespace AITaskCoach.Tests.Health;

public class DatabaseHealthCheckTests
{
    [Fact]
    public async Task InMemoryProvider_ReportsHealthy()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var context = new ApplicationDbContext(options);
        var check = new DatabaseHealthCheck(context);
        var healthContext = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration("database", _ => check, HealthStatus.Unhealthy, null)
        };

        var result = await check.CheckHealthAsync(healthContext);

        result.Status.Should().Be(HealthStatus.Healthy);
    }
}
