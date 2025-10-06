namespace Application.DTOs.Gamification;

public record Challenge(
    Guid Id,
    string Name,
    string Description,
    ChallengeType Type,
    int TargetValue,
    int CurrentProgress,
    DateTime StartDate,
    DateTime EndDate,
    int PointsReward,
    bool IsCompleted,
    string? BadgeReward = null
);

public enum ChallengeType
{
    Daily,
    Weekly,
    Monthly,
    TaskCount,
    StreakMaintenance,
    HabitConsistency,
    TimeSpent,
    ProductivityScore,
    GoalProgress
}
