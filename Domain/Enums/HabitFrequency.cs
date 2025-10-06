namespace Domain.Enums;

/// <summary>
/// Defines how frequently a habit should be performed
/// Optimized for AI scheduling and pattern recognition
/// </summary>
public enum HabitFrequency
{
    /// <summary>
    /// Habit should be performed every day
    /// </summary>
    Daily = 1,

    /// <summary>
    /// Habit should be performed every other day
    /// </summary>
    EveryOtherDay = 2,

    /// <summary>
    /// Habit should be performed 3 times per week
    /// </summary>
    ThreeTimesPerWeek = 3,

    /// <summary>
    /// Habit should be performed on weekdays only (Monday-Friday)
    /// </summary>
    Weekdays = 4,

    /// <summary>
    /// Habit should be performed on weekends only (Saturday-Sunday)
    /// </summary>
    Weekends = 5,

    /// <summary>
    /// Habit should be performed once per week
    /// </summary>
    Weekly = 6,

    /// <summary>
    /// Habit should be performed twice per week
    /// </summary>
    TwicePerWeek = 7,

    /// <summary>
    /// Habit should be performed once every two weeks (bi-weekly)
    /// </summary>
    BiWeekly = 8,

    /// <summary>
    /// Habit should be performed once per month
    /// </summary>
    Monthly = 9,

    /// <summary>
    /// Custom frequency defined by user
    /// </summary>
    Custom = 10
}

/// <summary>
/// Extension methods for HabitFrequency enum to support AI analysis
/// </summary>
public static class HabitFrequencyExtensions
{
    /// <summary>
    /// Gets the number of times per week this frequency should occur
    /// Used for AI progress calculations and streak analysis
    /// </summary>
    public static double GetTimesPerWeek(this HabitFrequency frequency)
    {
        return frequency switch
        {
            HabitFrequency.Daily => 7.0,
            HabitFrequency.EveryOtherDay => 3.5,
            HabitFrequency.ThreeTimesPerWeek => 3.0,
            HabitFrequency.Weekdays => 5.0,
            HabitFrequency.Weekends => 2.0,
            HabitFrequency.Weekly => 1.0,
            HabitFrequency.TwicePerWeek => 2.0,
            HabitFrequency.BiWeekly => 0.5,
            HabitFrequency.Monthly => 0.25,
            HabitFrequency.Custom => 1.0, // Default fallback
            _ => 1.0
        };
    }

    /// <summary>
    /// Gets a human-readable description of the frequency
    /// </summary>
    public static string GetDescription(this HabitFrequency frequency)
    {
        return frequency switch
        {
            HabitFrequency.Daily => "Every day",
            HabitFrequency.EveryOtherDay => "Every other day",
            HabitFrequency.ThreeTimesPerWeek => "3 times per week",
            HabitFrequency.Weekdays => "Weekdays only",
            HabitFrequency.Weekends => "Weekends only",
            HabitFrequency.Weekly => "Once per week",
            HabitFrequency.TwicePerWeek => "Twice per week",
            HabitFrequency.BiWeekly => "Every two weeks",
            HabitFrequency.Monthly => "Once per month",
            HabitFrequency.Custom => "Custom schedule",
            _ => "Unknown frequency"
        };
    }

    /// <summary>
    /// Determines if this frequency should be performed on a given day of the week
    /// Used for AI scheduling and reminder systems
    /// </summary>
    public static bool ShouldPerformOnDay(this HabitFrequency frequency, DayOfWeek dayOfWeek)
    {
        return frequency switch
        {
            HabitFrequency.Daily => true,
            HabitFrequency.Weekdays => dayOfWeek >= DayOfWeek.Monday && dayOfWeek <= DayOfWeek.Friday,
            HabitFrequency.Weekends => dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday,
            // For other frequencies, additional logic would be needed based on start date
            _ => true // Default to true for complex frequencies
        };
    }
}


/// <summary>
/// Habit categories for AI classification
/// </summary>
public enum HabitCategory
{
    Health = 1,
    Fitness = 2,
    Productivity = 3,
    Learning = 4,
    Social = 5,
    Mental = 6,
    Spiritual = 7,
    Financial = 8,
    Creative = 9,
    Personal = 10
}
