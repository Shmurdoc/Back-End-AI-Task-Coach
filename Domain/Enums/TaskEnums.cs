namespace Domain.Enums;



/// <summary>
/// How important a task is, from "Low" to "Urgent". I use this to help the AI figure out what should get attention first.
/// </summary>
public enum TaskPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4,
    Urgent = 5
}


/// <summary>
/// The kinds of patterns I want the AI to look for—like productivity times, habit streaks, or procrastination triggers. This helps drive smarter suggestions.
/// </summary>
public enum PatternType
{
    ProductivityTime = 1,
    TaskCompletion = 2,
    HabitStreaks = 3,
    GoalProgress = 4,
    EnergyLevels = 5,
    ProcrastinationTriggers = 6
}
