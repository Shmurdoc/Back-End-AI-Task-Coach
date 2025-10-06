namespace Domain.Enums;

/// <summary>
/// Task status enumeration
/// </summary>

/// <summary>
/// Task priority levels for AI scheduling
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
/// Pattern types for AI analysis
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
