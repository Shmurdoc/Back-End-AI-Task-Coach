using Application.CQRS.Queries.Goals;
using Application.DTOs.GoalDtos;
using Application.IRepositories;
using MediatR;

namespace Application.CQRS.Handlers.Goals.Queries;

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
        return goals.Select(goal => new GoalDto(
            goal.Id,
            goal.Title,
            goal.Description,
            goal.Category,
            goal.Status,
            goal.Priority,
            goal.TargetDate,
            goal.CreatedAt,
            goal.CompletedAt,
            goal.Progress
        ));
    }
}
