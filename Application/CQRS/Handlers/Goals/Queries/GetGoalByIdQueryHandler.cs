using Application.CQRS.Queries.Goals;
using Application.DTOs.GoalDtos;
using Application.IRepositories;
using MediatR;

namespace Application.CQRS.Handlers.Goals.Queries;

public class GetGoalByIdQueryHandler : IRequestHandler<GetGoalByIdQuery, GoalDto?>
{
    private readonly IGoalRepository _goalRepository;
    public GetGoalByIdQueryHandler(IGoalRepository goalRepository)
    {
        _goalRepository = goalRepository;
    }

    public async Task<GoalDto?> Handle(GetGoalByIdQuery request, CancellationToken cancellationToken)
    {
        var goal = await _goalRepository.GetByIdAsync(request.GoalId);
        if (goal == null) return null;
        return new GoalDto(
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
        );
    }
}
