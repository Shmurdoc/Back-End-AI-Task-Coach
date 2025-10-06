namespace Application.DTOs.HabitDtos;

/// <summary>
/// Analytics data for habit tracking
/// </summary>
public record HabitAnalyticsDto(
    Guid HabitId,
    string HabitName,
    int CurrentStreak,
    int LongestStreak,
    double CompletionRate,
    int TotalCompletions,
    DateTime StartDate,
    DateTime EndDate
);
    
