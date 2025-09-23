using Application.DTOs.Analytics;
using Application.DTOs.Gamification;

namespace Application.IService;

public interface IGamificationService
{
    // Points and Rewards System
    Task<int> AwardPointsAsync(Guid userId, int points, string reason);
    Task<int> GetUserPointsAsync(Guid userId);
    Task<List<UserBadge>> GetUserBadgesAsync(Guid userId);
    Task<UserBadge?> AwardBadgeAsync(Guid userId, string badgeType, string reason);
    
    // Streaks and Progress
    Task<int> UpdateStreakAsync(Guid userId, bool taskCompleted);
    Task<int> GetCurrentStreakAsync(Guid userId);
    Task<int> GetBestStreakAsync(Guid userId);
    
    // Levels and Experience
    Task<int> GetUserLevelAsync(Guid userId);
    Task<int> AddExperienceAsync(Guid userId, int experience, string activity);
    Task<int> GetUserExperienceAsync(Guid userId);
    
    // Leaderboards and Competition
    Task<List<LeaderboardEntry>> GetLeaderboardAsync(LeaderboardType type, int limit = 10);
    Task<int> GetUserRankAsync(Guid userId, LeaderboardType type);
    
    // Achievements and Challenges
    Task<List<Achievement>> CheckAchievementsAsync(Guid userId);
    Task<List<Challenge>> GetActiveChallengesAsync(Guid userId);
    Task<bool> CompleteObjectiveAsync(Guid userId, string objectiveType, object data);
    
    // Motivational System
    Task<string> GetMotivationalMessageAsync(Guid userId);
    Task<string> GetCelebrationMessageAsync(Guid userId, string achievement);
    
    // Analytics and Progress
    Task<GamificationSummaryDto> GetGamificationSummaryAsync(Guid userId);
    Task<bool> DetectRelapseAsync(Guid userId, CancellationToken cancellationToken = default);
}
