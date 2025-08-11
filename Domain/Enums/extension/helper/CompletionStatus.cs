namespace Domain.Enums.extension.helper;

/// <summary>
/// Status of completion for habits and tasks
/// </summary>
public enum CompletionStatus
{
    /// <summary>
    /// Not started or pending
    /// </summary>
    Pending = 1,

    /// <summary>
    /// In progress
    /// </summary>
    InProgress = 2,

    /// <summary>
    /// Successfully completed
    /// </summary>
    Completed = 3,

    /// <summary>
    /// Partially completed
    /// </summary>
    PartiallyCompleted = 4,

    /// <summary>
    /// Skipped for the day
    /// </summary>
    Skipped = 5,

    /// <summary>
    /// Failed to complete
    /// </summary>
    Failed = 6
}

/// <summary>
/// Extension methods for CompletionStatus to support AI analysis
/// </summary>
public static class CompletionStatusExtensions
{
    /// <summary>
    /// Determines if this status counts as a successful completion for streak calculation
    /// </summary>
    public static bool IsSuccessfulCompletion(this CompletionStatus status)
    {
        return status == CompletionStatus.Completed || status == CompletionStatus.PartiallyCompleted;
    }

    /// <summary>
    /// Determines if this status counts as an attempt (for AI learning purposes)
    /// </summary>
    public static bool IsAttempt(this CompletionStatus status)
    {
        return status switch
        {
            CompletionStatus.Pending => false,
            CompletionStatus.Failed => false,
            _ => true
        };
    }

    /// <summary>
    /// Gets the completion percentage (0-100) for this status
    /// Used for AI progress calculations
    /// </summary>
    public static int GetCompletionPercentage(this CompletionStatus status)
    {
        return status switch
        {
            CompletionStatus.Pending => 0,
            CompletionStatus.InProgress => 25,
            CompletionStatus.Completed => 100,
            CompletionStatus.PartiallyCompleted => 50,
            CompletionStatus.Skipped => 0,
            CompletionStatus.Failed => 25,
            _ => 0
        };
    }

    /// <summary>
    /// Gets a human-readable description of the status
    /// </summary>
    public static string GetDescription(this CompletionStatus status)
    {
        return status switch
        {
            CompletionStatus.Pending => "Not started",
            CompletionStatus.InProgress => "In progress",
            CompletionStatus.Completed => "Completed",
            CompletionStatus.PartiallyCompleted => "Partially completed",
            CompletionStatus.Skipped => "Skipped",
            CompletionStatus.Failed => "Failed",
            _ => "Unknown status"
        };
    }

    /// <summary>
    /// Gets the color associated with this status for UI representation
    /// </summary>
    public static string GetStatusColor(this CompletionStatus status)
    {
        return status switch
        {
            CompletionStatus.Pending => "#6c757d", // Gray
            CompletionStatus.InProgress => "#ffc107", // Yellow
            CompletionStatus.Completed => "#28a745", // Green
            CompletionStatus.PartiallyCompleted => "#17a2b8", // Blue
            CompletionStatus.Skipped => "#fd7e14", // Orange
            CompletionStatus.Failed => "#dc3545", // Red
            _ => "#6c757d"
        };
    }

    /// <summary>
    /// Determines if this status has a negative impact on habit formation
    /// Used for AI coaching and intervention triggers
    /// </summary>
    public static bool IsNegativeForHabitFormation(this CompletionStatus status)
    {
        return status switch
        {
            CompletionStatus.Failed => true,
            _ => false
        };
    }

    /// <summary>
    /// Gets the priority level for AI intervention (0-5, higher means more urgent)
    /// </summary>
    public static int GetInterventionPriority(this CompletionStatus status)
    {
        return status switch
        {
            CompletionStatus.Completed => 0,
            CompletionStatus.PartiallyCompleted => 1,
            CompletionStatus.InProgress => 2,
            CompletionStatus.Skipped => 3,
            CompletionStatus.Pending => 4,
            CompletionStatus.Failed => 4,
            _ => 3
        };
    }
}
