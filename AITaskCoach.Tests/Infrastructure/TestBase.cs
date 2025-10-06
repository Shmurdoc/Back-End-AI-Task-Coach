
using AutoFixture;
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

        // Remove built-in DataAnnotations relays (RegularExpressionAttributeRelay, DataAnnotationsSupportNode) if present
        var toRemove = Fixture.Customizations
            .Where(c => c.GetType().FullName == "AutoFixture.DataAnnotations.RegularExpressionAttributeRelay" ||
                        c.GetType().FullName == "AutoFixture.DataAnnotations.DataAnnotationsSupportNode")
            .ToList();
        foreach (var c in toRemove)
        {
            Fixture.Customizations.Remove(c);
        }

        // Intercept RegularExpressionRequest for Habit.Color and always return a valid color
        Fixture.Customizations.Insert(0, new HabitColorRegexSpecimenBuilder());

        // Configure DateOnly to generate valid dates
        Fixture.Customize<DateOnly>(composer => 
            composer.FromFactory(() => DateOnly.FromDateTime(DateTime.Today.AddDays(Random.Shared.Next(-365, 365)))));

        // Configure TimeOnly to generate valid times
        Fixture.Customize<TimeOnly>(composer => 
            composer.FromFactory(() => TimeOnly.FromTimeSpan(TimeSpan.FromMinutes(Random.Shared.Next(0, 1440)))));

        // Customize entities to avoid complex relationships in basic tests
        Fixture.Customize<Domain.Entities.Habit>(composer =>
            composer
                .With(h => h.Color, "#3498db")
                .With(h => h.IsActive, true)
                .Without(h => h.Analytics)
                .Without(h => h.Entries)
                .Without(h => h.User));

        Fixture.Customize<Domain.Entities.HabitEntry>(composer =>
            composer
                .Without(e => e.Habit));

        Fixture.Customize<Domain.Entities.Goal>(composer =>
            composer
                .Without(g => g.User)
                .Without(g => g.Tasks)
                .Without(g => g.ProgressHistory));

        Fixture.Customize<Domain.Entities.TaskItem>(composer =>
            composer
                .Without(t => t.Goal)
                .Without(t => t.User)
                .Without(t => t.StatusHistory)
                .Without(t => t.TimeEntries));

        Fixture.Customize<Domain.Entities.User>(composer =>
            composer
                .Without(u => u.Goals)
                .Without(u => u.Habits)
                .Without(u => u.Tasks));
    }

    // Custom specimen builder to intercept RegularExpressionRequest for Habit.Color
    private class HabitColorRegexSpecimenBuilder : AutoFixture.Kernel.ISpecimenBuilder
    {
        public object Create(object request, AutoFixture.Kernel.ISpecimenContext context)
        {
            // Intercept RegularExpressionRequest for Habit.Color
            if (request != null && request.GetType().Name == "RegularExpressionRequest")
            {
                // Try to get the member info (property) from the request
                var patternProp = request.GetType().GetProperty("Pattern");
                var memberProp = request.GetType().GetProperty("Member");
                var pattern = patternProp?.GetValue(request) as string;
                var member = memberProp?.GetValue(request);
                var propInfo = member as System.Reflection.PropertyInfo;
                if (propInfo != null &&
                    propInfo.DeclaringType == typeof(Domain.Entities.Habit) &&
                    propInfo.Name == "Color")
                {
                    // Always return a valid color
                    return "#3498db";
                }
            }
            return new AutoFixture.Kernel.NoSpecimen();
        }
    }

    protected async Task SeedDatabaseAsync<T>(params T[] entities) where T : class
    {
        // Clear any existing tracked entities to avoid conflicts
        DbContext.ChangeTracker.Clear();
        
        DbContext.Set<T>().AddRange(entities);
        await DbContext.SaveChangesAsync();
        
        // Detach entities after saving to avoid tracking issues
        foreach (var entity in entities)
        {
            DbContext.Entry(entity).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
        }
    }

    protected async Task ClearDatabaseAsync()
    {
        // Clear all data and reset tracking
        DbContext.ChangeTracker.Clear();
        
        try
        {
            // Simple approach - clear specific entities we know about
            DbContext.RemoveRange(DbContext.Users);
            DbContext.RemoveRange(DbContext.Tasks);
            DbContext.RemoveRange(DbContext.Goals);
            DbContext.RemoveRange(DbContext.Habits);
            DbContext.RemoveRange(DbContext.HabitEntries);
            
            await DbContext.SaveChangesAsync();
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    public void Dispose()
    {
        try
        {
            ClearDatabaseAsync().Wait();
        }
        catch
        {
            // Ignore cleanup errors during disposal
        }
        
        DbContext?.Dispose();
    }
}
