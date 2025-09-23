namespace Application.DTOs.Gamification;

public record Achievement(
    Guid Id,
    string Name,
    string Description,
    string IconUrl,
    int PointsReward,
    AchievementType Type,
    bool IsCompleted,
    DateTime? CompletedAt = null,
    double Progress = 0.0,
    string? ProgressDescription = null
);

public enum AchievementType
{
    TaskCompletion,
    StreakMaintenance,
    HabitFormation,
    GoalAchievement,
    TimeManagement,
    Consistency,
    Productivity,
    Social,
    Special
}
