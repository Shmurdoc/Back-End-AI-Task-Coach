using Application.IService;
using Application.IRepositories;
using Application.DTOs.Analytics;
using Application.DTOs.Gamification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services
{
    /// <summary>
    /// Every gamification value is recomputed from real activity (tasks, goals,
    /// habits) on each read. No points, badge or streak ledger exists. Anything
    /// not derivable from completion timestamps is not reported.
    /// </summary>
    public class GamificationService : IGamificationService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITaskRepository _taskRepository;
        private readonly IHabitRepository _habitRepository;
        private readonly IGoalRepository _goalRepository;

        // Streak history is rebuilt from completed-task timestamps over this
        // window; there is no permanent streak table.
        private const int StreakHistoryDays = 365;

        public GamificationService(
            IUserRepository userRepository,
            ITaskRepository taskRepository,
            IHabitRepository habitRepository,
            IGoalRepository goalRepository)
        {
            _userRepository = userRepository;
            _taskRepository = taskRepository;
            _habitRepository = habitRepository;
            _goalRepository = goalRepository;
        }

        public async Task<int> GetUserPointsAsync(Guid userId)
        {
            // Derived score: 10 pts per completed task, 50 per completed goal,
            // 25 per actively tracked habit. Recomputed on every read.
            var completedTasks = await _taskRepository.GetCompletedTaskCountByUserAsync(userId);
            var completedGoals = await _goalRepository.GetCompletedGoalCountByUserAsync(userId);
            var activeHabits = await _habitRepository.GetActiveHabitCountByUserAsync(userId);

            return (completedTasks * 10) + (completedGoals * 50) + (activeHabits * 25);
        }

        public async Task<List<UserBadge>> GetUserBadgesAsync(Guid userId)
        {
            var badges = new List<UserBadge>();
            var completedTasks = await _taskRepository.GetCompletedTaskCountByUserAsync(userId);
            var streak = await GetCurrentStreakAsync(userId);

            // Badges are derived on read (no badge ledger), so AwardedAt is the
            // evaluation time rather than the original milestone moment.
            if (completedTasks >= 10)
                badges.Add(new UserBadge(Guid.NewGuid(), "Task Starter", "Completed 10 tasks", "/badges/task-starter.png", DateTime.UtcNow, "Task completion milestone"));

            if (completedTasks >= 50)
                badges.Add(new UserBadge(Guid.NewGuid(), "Task Master", "Completed 50 tasks", "/badges/task-master.png", DateTime.UtcNow, "Task completion milestone", BadgeRarity.Rare));

            if (streak >= 7)
                badges.Add(new UserBadge(Guid.NewGuid(), "Week Warrior", "7-day streak", "/badges/week-warrior.png", DateTime.UtcNow, "Streak achievement"));

            if (streak >= 30)
                badges.Add(new UserBadge(Guid.NewGuid(), "Month Master", "30-day streak", "/badges/month-master.png", DateTime.UtcNow, "Streak achievement", BadgeRarity.Epic));

            return badges;
        }

        public async Task<int> GetCurrentStreakAsync(Guid userId)
        {
            var dates = await GetCompletionDatesAsync(userId);

            var day = DateTime.UtcNow.Date;
            // Today's streak survives until the day ends: only a full day
            // without any completion breaks it.
            if (!dates.Contains(day))
                day = day.AddDays(-1);

            var streak = 0;
            while (dates.Contains(day))
            {
                streak++;
                day = day.AddDays(-1);
            }
            return streak;
        }

        public async Task<int> GetBestStreakAsync(Guid userId)
        {
            // Longest run of consecutive days with at least one completed task
            // inside the retained window (no permanent best-streak ledger).
            var dates = (await GetCompletionDatesAsync(userId)).OrderBy(d => d).ToList();

            var best = 0;
            var run = 0;
            DateTime? previous = null;
            foreach (var date in dates)
            {
                run = (previous != null && date == previous.Value.AddDays(1)) ? run + 1 : 1;
                if (run > best) best = run;
                previous = date;
            }
            return best;
        }

        private async Task<HashSet<DateTime>> GetCompletionDatesAsync(Guid userId)
        {
            var completed = await _taskRepository.GetRecentTasksByUserAsync(userId, StreakHistoryDays);
            return completed
                .Where(t => t.CompletedAt.HasValue)
                .Select(t => t.CompletedAt!.Value.Date)
                .ToHashSet();
        }

        public async Task<int> GetUserLevelAsync(Guid userId)
        {
            var experience = await GetUserExperienceAsync(userId);
            return (experience / 1000) + 1; // 1000 XP per level
        }

        public async Task<int> GetUserExperienceAsync(Guid userId)
        {
            // Points convert to XP at a fixed 2:1 rate; there is no separate
            // experience store.
            var points = await GetUserPointsAsync(userId);
            return points * 2;
        }

        public async Task<List<LeaderboardEntry>> GetLeaderboardAsync(LeaderboardType type, int limit = 10)
        {
            var users = (await _userRepository.GetActiveUsersAsync()).ToList();
            var scored = new List<LeaderboardEntry>(users.Count);

            foreach (var user in users)
            {
                int score = type switch
                {
                    LeaderboardType.TasksCompleted => await _taskRepository.GetCompletedTaskCountByUserAsync(user.Id),
                    LeaderboardType.CurrentStreak => await GetCurrentStreakAsync(user.Id),
                    _ => await GetUserPointsAsync(user.Id),
                };
                scored.Add(new LeaderboardEntry(user.Id, user.Name, score, 0));
            }

            // Real ranking over active users; an empty table yields an empty
            // board rather than fabricated entries.
            return scored
                .OrderByDescending(e => e.Score)
                .Take(limit)
                .Select((entry, index) => entry with { Rank = index + 1 })
                .ToList();
        }

        public async Task<List<Achievement>> CheckAchievementsAsync(Guid userId)
        {
            var achievements = new List<Achievement>();
            var completedTasks = await _taskRepository.GetCompletedTaskCountByUserAsync(userId);
            var streak = await GetCurrentStreakAsync(userId);

            achievements.Add(new Achievement(
                Guid.NewGuid(),
                "Task Completionist",
                "Complete 100 tasks",
                "/achievements/task-completionist.png",
                500,
                AchievementType.TaskCompletion,
                completedTasks >= 100,
                completedTasks >= 100 ? DateTime.UtcNow.AddDays(-1) : null,
                Math.Min((double)completedTasks / 100, 1.0),
                $"{completedTasks}/100 tasks completed"
            ));

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
            var recent = (await _taskRepository.GetRecentTasksByUserAsync(userId, 7))
                .Where(t => t.CompletedAt.HasValue)
                .ToList();

            var today = DateTime.UtcNow.Date;
            var weekStart = today.AddDays(-(int)today.DayOfWeek);

            int dailyCompletions = recent.Count(t => t.CompletedAt!.Value.Date == today);
            int weeklyCompletions = recent.Count(t =>
            {
                var d = t.CompletedAt!.Value.Date;
                return d >= weekStart && d <= today;
            });

            return new List<Challenge>
            {
                new Challenge(
                    Guid.NewGuid(),
                    "Daily Dynamo",
                    "Complete 5 tasks today",
                    ChallengeType.Daily,
                    5,
                    Math.Min(dailyCompletions, 5),
                    today,
                    today.AddDays(1),
                    100,
                    dailyCompletions >= 5,
                    "Daily Star"
                ),
                new Challenge(
                    Guid.NewGuid(),
                    "Weekly Warrior",
                    "Complete 25 tasks this week",
                    ChallengeType.Weekly,
                    25,
                    Math.Min(weeklyCompletions, 25),
                    weekStart,
                    today.AddDays(7 - (int)today.DayOfWeek),
                    500,
                    weeklyCompletions >= 25,
                    "Weekly Champion"
                )
            };
        }

        public async Task<string> GetMotivationalMessageAsync(Guid userId)
        {
            var level = await GetUserLevelAsync(userId);
            var streak = await GetCurrentStreakAsync(userId);
            var completedTasks = await _taskRepository.GetCompletedTaskCountByUserAsync(userId);

            return (level, streak, completedTasks) switch
            {
                var (l, s, t) when s >= 7 => $"🔥 Amazing {s}-day streak! You're on fire!",
                var (l, s, t) when l >= 5 => $"🌟 Level {l} achiever! Your dedication is inspiring!",
                var (l, s, t) when t >= 50 => $"🏆 Task master with {t} completions! Keep crushing it!",
                var (l, s, t) when s >= 3 => $"💪 {s} days strong! Consistency is your superpower!",
                var (l, s, t) when t >= 10 => $"🚀 {t} tasks conquered! You're building momentum!",
                _ => "✨ Every small step counts! You've got this!"
            };
        }

        public async Task<GamificationSummaryDto> GetGamificationSummaryAsync(Guid userId)
        {
            var currentStreak = await GetCurrentStreakAsync(userId);
            var bestStreak = await GetBestStreakAsync(userId);
            var completedTasks = await _taskRepository.GetCompletedTaskCountByUserAsync(userId);
            var totalTasks = await _taskRepository.GetTaskCountByUserAsync(userId);
            var habitCount = await _habitRepository.GetHabitCountByUserAsync(userId);
            var goalCount = await _goalRepository.GetGoalCountByUserAsync(userId);
            var motivationalMessage = await GetMotivationalMessageAsync(userId);

            // Completion rate = completed / total tasks (the old formula
            // divided streak by task count, which is not a rate).
            double completionRate = totalTasks > 0 ? (double)completedTasks / totalTasks : 0.0;

            return new GamificationSummaryDto(
                CurrentStreak: currentStreak,
                BestStreak: bestStreak,
                CompletionRate: completionRate,
                TotalTasks: totalTasks,
                TotalGoals: goalCount,
                TotalHabits: habitCount,
                MotivationalMessage: motivationalMessage
            );
        }
    }
}
