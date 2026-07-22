namespace Domain.Enums;

public enum CompletionStatus
{
    Pending = 1,
    InProgress = 2,
    Completed = 3,
    PartiallyCompleted = 4,
    Skipped = 5,
    Failed = 6
}

public static class CompletionStatusExtensions
{
    public static bool IsSuccessfulCompletion(this CompletionStatus status)
    {
        return status == CompletionStatus.Completed || status == CompletionStatus.PartiallyCompleted;
    }

    public static bool IsAttempt(this CompletionStatus status)
    {
        return status switch
        {
            CompletionStatus.Pending => false,
            CompletionStatus.Failed => false,
            _ => true
        };
    }

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

    public static bool IsNegativeForHabitFormation(this CompletionStatus status)
    {
        return status switch
        {
            CompletionStatus.Failed => true,
            _ => false
        };
    }

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
