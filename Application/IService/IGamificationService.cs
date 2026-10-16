using Application.DTOs.Analytics;
using Application.DTOs.Gamification;

namespace Application.IService;

/// <summary>
/// Read-only gamification surface. Points, streaks, badges, achievements,
/// challenges and leaderboards are derived from real activity on every read.
/// There are deliberately no write endpoints (formerly "award points" /
/// "complete objective") because completing real tasks, goals and habits is
/// the only source of progress.
/// </summary>
public interface IGamificationService
{
    // Points and levels (derived)
    Task<int> GetUserPointsAsync(Guid userId);
    Task<int> GetUserLevelAsync(Guid userId);
    Task<int> GetUserExperienceAsync(Guid userId);
    Task<List<UserBadge>> GetUserBadgesAsync(Guid userId);

    // Streaks (derived from completed-task timestamps)
    Task<int> GetCurrentStreakAsync(Guid userId);
    Task<int> GetBestStreakAsync(Guid userId);

    // Leaderboards (real ranking over active users)
    Task<List<LeaderboardEntry>> GetLeaderboardAsync(LeaderboardType type, int limit = 10);

    // Achievements and challenges (derived progress)
    Task<List<Achievement>> CheckAchievementsAsync(Guid userId);
    Task<List<Challenge>> GetActiveChallengesAsync(Guid userId);

    // Motivation and summary
    Task<string> GetMotivationalMessageAsync(Guid userId);
    Task<GamificationSummaryDto> GetGamificationSummaryAsync(Guid userId);
}
