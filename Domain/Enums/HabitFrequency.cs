namespace Domain.Enums;

public enum HabitFrequency
{
    Daily = 1,
    EveryOtherDay = 2,
    ThreeTimesPerWeek = 3,
    Weekdays = 4,
    Weekends = 5,
    Weekly = 6,
    TwicePerWeek = 7,
    BiWeekly = 8,
    Monthly = 9,
    Custom = 10
}

public static class HabitFrequencyExtensions
{
    public static double GetTimesPerWeek(this HabitFrequency frequency) => frequency switch
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
        _ => 1.0
    };

    public static string GetDescription(this HabitFrequency frequency) => frequency switch
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

    public static bool ShouldPerformOnDay(this HabitFrequency frequency, DayOfWeek dayOfWeek) => frequency switch
    {
        HabitFrequency.Daily => true,
        HabitFrequency.Weekdays => dayOfWeek >= DayOfWeek.Monday && dayOfWeek <= DayOfWeek.Friday,
        HabitFrequency.Weekends => dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday,
        _ => true
    };
}

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
