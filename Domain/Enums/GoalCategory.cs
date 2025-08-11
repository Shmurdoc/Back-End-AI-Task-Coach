namespace Domain.Enums;

/// <summary>
/// Goal categories for classification
/// </summary>
public enum GoalCategory
{
    Personal = 1,
    Professional = 2,
    Health = 3,
    Education = 4,
    Finance = 5,
    Relationships = 6,
    Creative = 7,
    Travel = 8,
    Other = 9
}

/// <summary>
/// Goal status enumeration
/// </summary>
public enum GoalStatus
{
    NotStarted = 1,
    InProgress = 2,
    OnHold = 3,
    Completed = 4,
    Cancelled = 5
}
