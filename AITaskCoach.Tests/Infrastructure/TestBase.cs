
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

        // Optionally, still set Color for all Habits as a fallback
        Fixture.Customize<Domain.Entities.Habit>(composer =>
            composer.Do(h => h.Color = "#3498db")
        );

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
        DbContext.Set<T>().AddRange(entities);
        await DbContext.SaveChangesAsync();
    }

    public void Dispose()
    {
        DbContext?.Dispose();
    }
}
