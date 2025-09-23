using Application.IService;
using Application.IRepositories;
using Application.DTOs.Analytics;
using Application.DTOs.Gamification;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services
{
    public class GamificationService : IGamificationService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITaskRepository _taskRepository;
        private readonly IHabitRepository _habitRepository;
        private readonly IGoalRepository _goalRepository;
        private readonly ILogger<GamificationService> _logger;

        public GamificationService(
            IUserRepository userRepository,
            ITaskRepository taskRepository,
            IHabitRepository habitRepository,
            IGoalRepository goalRepository,
            ILogger<GamificationService> logger)
        {
            _userRepository = userRepository;
            _taskRepository = taskRepository;
            _habitRepository = habitRepository;
            _goalRepository = goalRepository;
            _logger = logger;
        }

        public async Task<int> AwardPointsAsync(Guid userId, int points, string reason)
        {
            _logger.LogInformation("Awarding {Points} points to user {UserId} for: {Reason}", points, userId, reason);
            // TODO: Implement points storage in database
            // For now, return calculated points based on user activity
            var taskCount = await _taskRepository.GetTaskCountByUserAsync(userId);
            return taskCount * 10 + points;
        }

        public async Task<int> GetUserPointsAsync(Guid userId)
        {
            // Calculate points based on user activities
            var taskCount = await _taskRepository.GetTaskCountByUserAsync(userId);
            var habitCount = await _habitRepository.GetHabitCountByUserAsync(userId);
            var goalCount = await _goalRepository.GetGoalCountByUserAsync(userId);
            
            return (taskCount * 10) + (habitCount * 25) + (goalCount * 50);
        }

        public async Task<List<UserBadge>> GetUserBadgesAsync(Guid userId)
        {
            var badges = new List<UserBadge>();
            var taskCount = await _taskRepository.GetTaskCountByUserAsync(userId);
            var streak = await GetCurrentStreakAsync(userId);

            // Task completion badges
            if (taskCount >= 10)
                badges.Add(new UserBadge(Guid.NewGuid(), "Task Starter", "Completed 10 tasks", "/badges/task-starter.png", DateTime.UtcNow.AddDays(-5), "Task completion milestone"));
            
            if (taskCount >= 50)
                badges.Add(new UserBadge(Guid.NewGuid(), "Task Master", "Completed 50 tasks", "/badges/task-master.png", DateTime.UtcNow.AddDays(-10), "Task completion milestone", BadgeRarity.Rare));
            
            // Streak badges
            if (streak >= 7)
                badges.Add(new UserBadge(Guid.NewGuid(), "Week Warrior", "7-day streak", "/badges/week-warrior.png", DateTime.UtcNow.AddDays(-2), "Streak achievement"));
            
            if (streak >= 30)
                badges.Add(new UserBadge(Guid.NewGuid(), "Month Master", "30-day streak", "/badges/month-master.png", DateTime.UtcNow.AddDays(-5), "Streak achievement", BadgeRarity.Epic));

            return badges;
        }

        public async Task<UserBadge?> AwardBadgeAsync(Guid userId, string badgeType, string reason)
        {
            _logger.LogInformation("Awarding badge {BadgeType} to user {UserId} for: {Reason}", badgeType, userId, reason);
            
            return badgeType.ToLower() switch
            {
                "first-task" => new UserBadge(Guid.NewGuid(), "First Steps", "Completed your first task", "/badges/first-task.png", DateTime.UtcNow, reason),
                "productivity-pro" => new UserBadge(Guid.NewGuid(), "Productivity Pro", "Exceptional productivity", "/badges/productivity-pro.png", DateTime.UtcNow, reason, BadgeRarity.Rare),
                _ => null
            };
        }

        public async Task<int> UpdateStreakAsync(Guid userId, bool taskCompleted)
        {
            if (taskCompleted)
            {
                var currentStreak = await GetCurrentStreakAsync(userId);
                return currentStreak + 1;
            }
            return 0; // Streak broken
        }

        public async Task<int> GetCurrentStreakAsync(Guid userId)
        {
            // TODO: Implement actual streak calculation from database
            // For now, return a mock value based on recent task activity
            var recentTasks = await _taskRepository.GetRecentTasksByUserAsync(userId, 7);
            return Math.Min(recentTasks.Count(), 7); // Simple streak calculation
        }

        public async Task<int> GetBestStreakAsync(Guid userId)
        {
            // TODO: Implement actual best streak from user statistics
            var currentStreak = await GetCurrentStreakAsync(userId);
            return Math.Max(currentStreak, 15); // Mock best streak
        }

        public async Task<int> GetUserLevelAsync(Guid userId)
        {
            var experience = await GetUserExperienceAsync(userId);
            return (experience / 1000) + 1; // 1000 XP per level
        }

        public async Task<int> AddExperienceAsync(Guid userId, int experience, string activity)
        {
            _logger.LogInformation("Adding {Experience} XP to user {UserId} for: {Activity}", experience, userId, activity);
            var currentXP = await GetUserExperienceAsync(userId);
            return currentXP + experience;
        }

        public async Task<int> GetUserExperienceAsync(Guid userId)
        {
            var points = await GetUserPointsAsync(userId);
            return points * 2; // Convert points to experience
        }

        public async Task<List<LeaderboardEntry>> GetLeaderboardAsync(LeaderboardType type, int limit = 10)
        {
            // TODO: Implement actual leaderboard from database
            // For now, return mock leaderboard data
            return new List<LeaderboardEntry>
            {
                new LeaderboardEntry(Guid.NewGuid(), "ProductivityMaster", 2500, 1),
                new LeaderboardEntry(Guid.NewGuid(), "TaskNinja", 2200, 2),
                new LeaderboardEntry(Guid.NewGuid(), "HabitHero", 1900, 3),
                new LeaderboardEntry(Guid.NewGuid(), "GoalGetter", 1750, 4),
                new LeaderboardEntry(Guid.NewGuid(), "FocusedOne", 1600, 5)
            }.Take(limit).ToList();
        }

        public async Task<int> GetUserRankAsync(Guid userId, LeaderboardType type)
        {
            var leaderboard = await GetLeaderboardAsync(type, 100);
            var userEntry = leaderboard.FirstOrDefault(e => e.UserId == userId);
            return userEntry?.Rank ?? 50; // Default rank if not found
        }

        public async Task<List<Achievement>> CheckAchievementsAsync(Guid userId)
        {
            var achievements = new List<Achievement>();
            var taskCount = await _taskRepository.GetTaskCountByUserAsync(userId);
            var streak = await GetCurrentStreakAsync(userId);

            // Task achievements
            achievements.Add(new Achievement(
                Guid.NewGuid(),
                "Task Completionist",
                "Complete 100 tasks",
                "/achievements/task-completionist.png",
                500,
                AchievementType.TaskCompletion,
                taskCount >= 100,
                taskCount >= 100 ? DateTime.UtcNow.AddDays(-1) : null,
                Math.Min((double)taskCount / 100, 1.0),
                $"{taskCount}/100 tasks completed"
            ));

            // Streak achievements
            achievements.Add(new Achievement(
                Guid.NewGuid(),
                "Consistency Champion",
                "Maintain a 30-day streak",
                "/achievements/consistency-champion.png",
                1000,
                AchievementType.StreakMaintenance,
                streak >= 30,
                streak >= 30 ? DateTime.UtcNow.AddDays(-2) : null,
                Math.Min((double)streak / 30, 1.0),
                $"{streak}/30 day streak"
            ));

            return achievements;
        }

        public async Task<List<Challenge>> GetActiveChallengesAsync(Guid userId)
        {
            var taskCount = await _taskRepository.GetTaskCountByUserAsync(userId);
            var today = DateTime.UtcNow.Date;

            return new List<Challenge>
            {
                new Challenge(
                    Guid.NewGuid(),
                    "Daily Dynamo",
                    "Complete 5 tasks today",
                    ChallengeType.Daily,
                    5,
                    Math.Min(taskCount % 5, 5), // Mock daily progress
                    today,
                    today.AddDays(1),
                    100,
                    taskCount % 5 >= 5,
                    "Daily Star"
                ),
                new Challenge(
                    Guid.NewGuid(),
                    "Weekly Warrior",
                    "Complete 25 tasks this week",
                    ChallengeType.Weekly,
                    25,
                    Math.Min(taskCount % 25, 25), // Mock weekly progress
                    today.AddDays(-(int)today.DayOfWeek),
                    today.AddDays(7 - (int)today.DayOfWeek),
                    500,
                    taskCount % 25 >= 25,
                    "Weekly Champion"
                )
            };
        }

        public async Task<bool> CompleteObjectiveAsync(Guid userId, string objectiveType, object data)
        {
            _logger.LogInformation("User {UserId} completed objective: {ObjectiveType}", userId, objectiveType);
            
            // Award points based on objective type
            int points = objectiveType.ToLower() switch
            {
                "task_completed" => 10,
                "habit_completed" => 25,
                "goal_achieved" => 100,
                "streak_maintained" => 50,
                _ => 5
            };

            await AwardPointsAsync(userId, points, $"Completed {objectiveType}");
            return true;
        }

        public async Task<string> GetMotivationalMessageAsync(Guid userId)
        {
            var level = await GetUserLevelAsync(userId);
            var streak = await GetCurrentStreakAsync(userId);
            var taskCount = await _taskRepository.GetTaskCountByUserAsync(userId);

            return (level, streak, taskCount) switch
            {
                var (l, s, t) when s >= 7 => $"🔥 Amazing {s}-day streak! You're on fire!",
                var (l, s, t) when l >= 5 => $"🌟 Level {l} achiever! Your dedication is inspiring!",
                var (l, s, t) when t >= 50 => $"🏆 Task master with {t} completions! Keep crushing it!",
                var (l, s, t) when s >= 3 => $"💪 {s} days strong! Consistency is your superpower!",
                var (l, s, t) when t >= 10 => $"🚀 {t} tasks conquered! You're building momentum!",
                _ => "✨ Every small step counts! You've got this!"
            };
        }

        public async Task<string> GetCelebrationMessageAsync(Guid userId, string achievement)
        {
            var userName = (await _userRepository.GetByIdAsync(userId))?.Name ?? "Champion";
            
            return achievement.ToLower() switch
            {
                "task_completed" => $"🎉 Well done, {userName}! Another task conquered!",
                "streak_milestone" => $"🔥 Incredible streak, {userName}! You're unstoppable!",
                "level_up" => $"⭐ Level up, {userName}! Your growth is amazing!",
                "badge_earned" => $"🏅 New badge unlocked, {userName}! You've earned it!",
                "goal_achieved" => $"🎯 Goal smashed, {userName}! Celebration time!",
                _ => $"🌟 Fantastic work, {userName}! Keep shining!"
            };
        }

        public async Task<GamificationSummaryDto> GetGamificationSummaryAsync(Guid userId)
        {
            var currentStreak = await GetCurrentStreakAsync(userId);
            var bestStreak = await GetBestStreakAsync(userId);
            var taskCount = await _taskRepository.GetTaskCountByUserAsync(userId);
            var habitCount = await _habitRepository.GetHabitCountByUserAsync(userId);
            var goalCount = await _goalRepository.GetGoalCountByUserAsync(userId);
            var motivationalMessage = await GetMotivationalMessageAsync(userId);

            // Calculate completion rate
            double completionRate = taskCount > 0 ? Math.Min((double)currentStreak / taskCount, 1.0) : 0.0;

            return new GamificationSummaryDto(
                CurrentStreak: currentStreak,
                BestStreak: bestStreak,
                CompletionRate: completionRate,
                TotalTasks: taskCount,
                TotalGoals: goalCount,
                TotalHabits: habitCount,
                MotivationalMessage: motivationalMessage
            );
        }

        public async Task<bool> DetectRelapseAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var currentStreak = await GetCurrentStreakAsync(userId);
            var recentTasks = await _taskRepository.GetRecentTasksByUserAsync(userId, 3);
            
            // Detect relapse if no tasks completed in last 3 days and had a streak
            bool hasRelapsed = currentStreak == 0 && !recentTasks.Any() && await GetBestStreakAsync(userId) > 0;
            
            if (hasRelapsed)
            {
                _logger.LogWarning("Relapse detected for user {UserId}", userId);
            }
            
            return hasRelapsed;
        }
    }
}
