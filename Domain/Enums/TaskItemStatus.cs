namespace Domain.Enums;

/// <summary>
/// Defines the status of a task item
/// </summary>
public enum TaskItemStatus
{
    /// <summary>
    /// Task is pending and not yet started
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Task is currently in progress
    /// </summary>
    InProgress = 2,

    /// <summary>
    /// Task has been completed
    /// </summary>
    Completed = 3,

    /// <summary>
    /// Task has been cancelled
    /// </summary>
    Cancelled = 4,

    /// <summary>
    /// Task is on hold or paused
    /// </summary>
    OnHold = 5
}
