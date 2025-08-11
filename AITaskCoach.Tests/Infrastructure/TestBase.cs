
using Microsoft.EntityFrameworkCore;
using Infrastructure.Persistence.Data;

namespace AITaskCoach.Tests.Infrastructure;

public abstract class TestBase : IDisposable
{
    protected readonly Fixture Fixture;
    protected readonly ApplicationDbContext DbContext;

    protected TestBase()
    {
        Fixture = new Fixture();
        ConfigureFixture();
        
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        DbContext = new ApplicationDbContext(options);
    }

    protected virtual void ConfigureFixture()
    {
        // Configure AutoFixture to handle circular references
        Fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => Fixture.Behaviors.Remove(b));
        Fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    protected async Task SeedDatabaseAsync<T>(params T[] entities) where T : class
    {
        DbContext.Set<T>().AddRange(entities);
        await DbContext.SaveChangesAsync();
    }

    public void Dispose()
    {
        DbContext?.Dispose();
    }
}
