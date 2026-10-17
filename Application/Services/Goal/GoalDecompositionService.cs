using Application.DTOs.GoalDtos;
using Application.IRepositories;
using Application.IService.Goal;
using Domain.Enums;

namespace Application.Services.Goal;

/// <summary>
/// Builds a category-aware decomposition for a stored goal. The goal is
/// loaded from the database and the caller must own it; anything else is
/// treated as not found. The steps come from simple rules based on the
/// category and target date, and the response says the result is heuristic.
/// </summary>
public class GoalDecompositionService : IGoalDecompositionService
{
    private readonly IGoalRepository _goalRepository;
    private readonly IUserContext _userContext;

    public GoalDecompositionService(IGoalRepository goalRepository, IUserContext userContext)
    {
        _goalRepository = goalRepository;
        _userContext = userContext;
    }

    public async Task<GoalDecompositionDto?> DecomposeGoalAsync(Guid goalId, CancellationToken ct = default)
    {
        var goal = await _goalRepository.GetByIdAsync(goalId);
        if (goal == null || goal.UserId != _userContext.GetCurrentUserId())
            return null;

        var subtasks = new List<string>
        {
            $"Define what \"done\" looks like for \"{goal.Title}\"",
            CategorySpecificStep(goal.Category, goal.Title),
            goal.TargetDate is { } target
                ? $"Schedule a mid-point review on {target.AddDays(-((target - DateTime.UtcNow).TotalDays / 2)):yyyy-MM-dd}"
                : "Set a target date, then schedule a mid-point review",
            "Review progress weekly and adjust the plan"
        };

        var explanation = "Heuristic decomposition based on the goal's category"
            + (goal.TargetDate.HasValue ? " and target date" : string.Empty)
            + ".";

        return new GoalDecompositionDto(goal.Id, goal.Title, subtasks, explanation);
    }

    private static string CategorySpecificStep(GoalCategory category, string title) => category switch
    {
        GoalCategory.Professional => $"List the deliverables and stakeholders for \"{title}\"",
        GoalCategory.Health => $"Set a measurable weekly target for \"{title}\" and track it daily",
        GoalCategory.Education => $"Gather resources and block focused study time for \"{title}\"",
        GoalCategory.Finance => $"Outline the budget checkpoints for \"{title}\" with review dates",
        GoalCategory.Relationships => $"Plan the first concrete action for \"{title}\" and a follow-up check-in",
        GoalCategory.Creative => $"Sketch a rough first version of \"{title}\" to iterate on",
        GoalCategory.Travel => $"Research logistics (dates, budget, bookings) for \"{title}\"",
        GoalCategory.Personal => $"Identify the smallest first step for \"{title}\" you can do this week",
        _ => $"Break \"{title}\" into three concrete milestones"
    };
}
