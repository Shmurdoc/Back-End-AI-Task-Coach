using Application.DTOs.GoalDtos;
using Application.CQRS.Features.Queries.Goals;
using Application.IService;
using MediatR;

namespace Application.CQRS.Features.Handlers.Goals;

public class GetGoalByIdQueryHandler : IRequestHandler<GetGoalByIdQuery, GoalDto?>
{
    private readonly IGoalService _goalService;

    public GetGoalByIdQueryHandler(IGoalService goalService)
    {
        _goalService = goalService;
    }

    public async Task<GoalDto?> Handle(GetGoalByIdQuery request, CancellationToken cancellationToken)
    {
        return await _goalService.GetGoalByIdAsync(request.GoalId, cancellationToken);
    }
}