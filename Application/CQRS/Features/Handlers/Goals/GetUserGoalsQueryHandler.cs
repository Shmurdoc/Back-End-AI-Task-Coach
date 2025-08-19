using Application.CQRS.Features.Queries.Goals;
using Application.DTOs.GoalDtos;
using Application.IRepositories;
using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Application.CQRS.Features.Handlers.Goals;

public class GetUserGoalsQueryHandler : IRequestHandler<GetUserGoalsQuery, IEnumerable<GoalDto>>
{
    private readonly IGoalRepository _goalRepository;

    public GetUserGoalsQueryHandler(IGoalRepository goalRepository)
    {
        _goalRepository = goalRepository;
    }

    public async Task<IEnumerable<GoalDto>> Handle(GetUserGoalsQuery request, CancellationToken cancellationToken)
    {
        var goals = await _goalRepository.GetUserGoalsAsync(request.UserId);
        return goals.Select(MapToGoalDto).ToList();
    }

    private static GoalDto MapToGoalDto(Goal goal)
    {
        double progress = 0;
        if (goal.Tasks != null && goal.Tasks.Count > 0)
        {
            var completed = goal.Tasks.Count(t => t.Status == TaskItemStatus.Completed);
            progress = Math.Round((double)completed / goal.Tasks.Count * 100, 2);
        }

        return new GoalDto(
            goal.Id,
            goal.Title,
            goal.Description ?? string.Empty,
            goal.Category,
            goal.Status,
            goal.Priority,
            goal.TargetDate,
            goal.CreatedAt,
            goal.CompletedAt,
            progress
        );
    }
}
