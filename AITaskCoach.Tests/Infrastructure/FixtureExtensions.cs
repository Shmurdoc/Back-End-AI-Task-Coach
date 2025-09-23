// This file is required to add the missing AutoFixture.DataAnnotations customization to the test project.
// It ensures that RegularExpression attributes are handled correctly for properties like Habit.Color.
using AutoFixture;


namespace AITaskCoach.Tests.Infrastructure
{
    public static class FixtureExtensions
    {
        public static void AddDataAnnotationsSupport(this Fixture fixture)
        {
            // DataAnnotationsCustomization is not available in this environment.
            // This method is now a no-op. Habit.Color is handled by a custom specimen builder in TestBase.cs.
        }
    }
}
