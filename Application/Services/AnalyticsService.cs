using Application.DTOs;
using Application.IRepositories;
using Application.IService;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;



public class AnalyticsService : IAnalyticsService
{
    private readonly IGoalRepository _goalRepository;
    private readonly IHabitRepository _habitRepository;
    private readonly ITaskRepository _taskRepository;

    public AnalyticsService(IGoalRepository goalRepo, IHabitRepository habitRepo, ITaskRepository taskRepo)
    {
        _goalRepository = goalRepo;
        _habitRepository = habitRepo;
        _taskRepository = taskRepo;
    }

    public async Task<ProductivitySummaryDto> CalculateUserStats(Guid userId)
    {
        var goals = await _goalRepository.GetUserGoalsAsync(userId);
        var habits = await _habitRepository.GetUserHabitsAsync(userId);
        var tasks = await _taskRepository.GetUserTasksAsync(userId);

        int goalsCompleted = goals.Count(g => g.Status == GoalStatus.Completed);
        int habitsTracked = habits.Count();
        int tasksCompleted = tasks.Count(t => t.Status == TaskItemStatus.Completed);

        return new ProductivitySummaryDto(goalsCompleted, habitsTracked, tasksCompleted);
    }

    public async Task<ProductivitySummaryDto> GetProductivitySummaryAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await CalculateUserStats(userId);
    }

    public async Task<UserReportDto> GenerateUserReportAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var goals = (await _goalRepository.GetUserGoalsAsync(userId)).ToList();
        var tasks = (await _taskRepository.GetUserTasksAsync(userId)).ToList();
        var habits = (await _habitRepository.GetUserHabitsAsync(userId)).ToList();

        var now = DateTime.UtcNow;
        int completedTasks = tasks.Count(t => t.Status == TaskItemStatus.Completed);

        return new UserReportDto
        {
            UserId = userId,
            GeneratedAt = now,

            TotalGoals = goals.Count,
            CompletedGoals = goals.Count(g => g.Status == GoalStatus.Completed),
            OverdueGoals = goals.Count(g => g.TargetDate < now && g.Status != GoalStatus.Completed),
            AverageGoalProgress = goals.Count > 0 ? Math.Round(goals.Average(g => g.Progress), 1) : 0,

            TotalTasks = tasks.Count,
            CompletedTasks = completedTasks,
            TaskCompletionRate = tasks.Count > 0 ? Math.Round(100.0 * completedTasks / tasks.Count, 1) : 0,
            TasksCompletedLast7Days = tasks.Count(t => t.CompletedAt >= now.AddDays(-7)),
            GoalLinkedTasks = tasks.Count(t => t.GoalId != null),
            TasksByPriority = tasks
                .GroupBy(t => t.Priority.ToString())
                .ToDictionary(g => g.Key, g => g.Count()),

            TotalHabits = habits.Count,
            ActiveHabits = habits.Count(h => h.IsActive),
            HabitsOnCurrentStreak = habits.Count(h => h.CurrentStreak > 0)
        };
    }

    public async Task<HabitSummaryDto> GetHabitSummaryAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var habits = (await _habitRepository.GetUserHabitsAsync(userId)).ToList();

        var today = DateTime.UtcNow.Date;
        var start7 = today.AddDays(-6);
        var start30 = today.AddDays(-29);
        var endExclusive = today.AddDays(1);

        var habitStats = new List<HabitStatDto>(habits.Count);
        foreach (var habit in habits)
        {
            var entryList = (await _habitRepository.GetHabitEntriesAsync(habit.Id, start30, endExclusive)).ToList();

            habitStats.Add(new HabitStatDto
            {
                HabitId = habit.Id,
                Name = habit.Name,
                Frequency = habit.Frequency.ToString(),
                IsActive = habit.IsActive,
                CurrentStreak = habit.CurrentStreak,
                BestStreak = habit.BestStreak,
                CompletionRate = habit.CompletionRate,
                LastCompletedAt = habit.LastCompletedAt,
                EntriesLast7Days = entryList.Count(e => e.Date >= start7),
                EntriesLast30Days = entryList.Count
            });
        }

        return new HabitSummaryDto
        {
            UserId = userId,
            GeneratedAt = DateTime.UtcNow,
            TotalHabits = habits.Count,
            ActiveHabits = habits.Count(h => h.IsActive),
            HabitsOnCurrentStreak = habits.Count(h => h.CurrentStreak > 0),
            AverageCompletionRate = habits.Count > 0 ? Math.Round(habits.Average(h => h.CompletionRate), 1) : 0,
            Habits = habitStats
        };
    }
}
