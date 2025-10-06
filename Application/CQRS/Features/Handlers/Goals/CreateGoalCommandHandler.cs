using Application.CQRS.Features.Commands.Goals;
using Application.DTOs.GoalDtos;
using Application.IRepositories;
using Application.IService;
using Domain.Entities;
using MediatR;

namespace Application.CQRS.Features.Handlers.Goals;

public class CreateGoalCommandHandler : IRequestHandler<CreateGoalCommand, GoalDto>
{
    private readonly IGoalService _goalService;

    public CreateGoalCommandHandler(IGoalService goalService)
    {
       _goalService = goalService;
    }

    public async Task<GoalDto> Handle(CreateGoalCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate the request data (e.g., check if the goal name is not empty).
        if (string.IsNullOrWhiteSpace(request.CreateGoalDto.Title))
        {
            throw new ArgumentException("Goal name cannot be empty.", nameof(request.CreateGoalDto.Title));
        }
        var goal = await _goalService.CreateGoalAsync(request.CreateGoalDto,cancellationToken);

        return goal;
    }
}
