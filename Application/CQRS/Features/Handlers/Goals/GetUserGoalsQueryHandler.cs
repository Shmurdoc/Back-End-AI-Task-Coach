using Application.CQRS.Features.Queries.Goals;
using Application.DTOs.GoalDtos;
using Application.IRepositories;
using Application.IService;
using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Application.CQRS.Features.Handlers.Goals;

public class GetUserGoalsQueryHandler : IRequestHandler<GetUserGoalsQuery, IEnumerable<GoalDto>>
{
    private readonly IGoalService _goalService;

    public GetUserGoalsQueryHandler(IGoalRepository goalRepository, IGoalService goalService)
    {
        _goalService = goalService;
    }

    public async Task<IEnumerable<GoalDto>> Handle(GetUserGoalsQuery request, CancellationToken cancellationToken)
    {
        var goals = await _goalService.GetUserGoal(request.UserId);

        return goals;
    }
}
