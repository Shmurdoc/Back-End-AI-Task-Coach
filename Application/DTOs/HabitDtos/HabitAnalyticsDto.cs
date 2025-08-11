namespace Application.DTOs.HabitDtos;

/// <summary>
/// Analytics data for habit tracking
/// </summary>
public class HabitAnalyticsDto
{
    /// <summary>
    /// Habit ID
    /// </summary>
    public Guid HabitId { get; set; }

    /// <summary>
    /// Habit name
    /// </summary>
    public string HabitName { get; set; } = string.Empty;

    /// <summary>
    /// Current streak in days
    /// </summary>
    public int CurrentStreak { get; set; }

    /// <summary>
    /// Longest streak achieved
    /// </summary>
    public int LongestStreak { get; set; }

    /// <summary>
    /// Completion rate as percentage
    /// </summary>
    public double CompletionRate { get; set; }

    /// <summary>
    /// Total completions
    /// </summary>
    public int TotalCompletions { get; set; }

    /// <summary>
    /// Date range for the analytics
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// End date for the analytics
    /// </summary>
    public DateTime EndDate { get; set; }
}
