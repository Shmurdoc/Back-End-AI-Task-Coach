namespace Application.DTOs.Analytics;

public record GamificationSummaryDto(
    int CurrentStreak,
    int BestStreak,
    double CompletionRate,
    int TotalTasks,
    int TotalGoals,
    int TotalHabits,
    string? MotivationalMessage = null
);
