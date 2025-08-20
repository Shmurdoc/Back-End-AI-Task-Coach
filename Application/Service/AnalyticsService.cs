
using Application.DTOs;
using Application.IRepositories;
using Application.IService;
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

    // ...other analytics logic as needed...
}
