namespace Domain.Enums;

/// <summary>
/// Defines the status of a task item
/// </summary>
public enum TaskItemStatus
{
    Todo,
    InProgress,
    Review,
    Completed,
    Cancelled,
    OnHold,
    Blocked
}
