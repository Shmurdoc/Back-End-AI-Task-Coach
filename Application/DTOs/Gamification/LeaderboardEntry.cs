namespace Application.DTOs.Gamification;

public record LeaderboardEntry(
    Guid UserId,
    string Username,
    int Score,
    int Rank,
    string? Avatar = null
);

public enum LeaderboardType
{
    Points,
    TasksCompleted,
    CurrentStreak,
    BestStreak,
    HabitsCompleted,
    GoalsAchieved,
    Weekly,
    Monthly,
    AllTime
}
