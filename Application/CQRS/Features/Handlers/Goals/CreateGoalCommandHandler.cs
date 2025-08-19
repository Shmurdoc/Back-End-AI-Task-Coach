using Application.CQRS.Features.Commands.Goals;
using Application.IRepositories;
using Domain.Entities;
using MediatR;

namespace Application.CQRS.Features.Handlers.Goals;

public class CreateGoalCommandHandler : IRequestHandler<CreateGoalCommand, Goal>
{
    private readonly IGoalRepository _goalRepository;
    private readonly IUserContext _userContext;

    public CreateGoalCommandHandler(IGoalRepository goalRepository, IUserContext userContext)
    {
        _goalRepository = goalRepository;
        _userContext = userContext;
    }

    public async Task<Goal> Handle(CreateGoalCommand request, CancellationToken cancellationToken)
    {

        //var userId = _userContext.GetCurrentUserId();
        var b1b6edf2 = new Guid("b1b6edf2-d505-47fb-b696-fc1c8a86ebde"); // Replace with actual user ID retrieval logic
        var userId = b1b6edf2;

        var goal = new Goal
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = request.CreateGoalDto.Title,
            Description = request.CreateGoalDto.Description ?? string.Empty,
            Category = request.CreateGoalDto.Category,
            Priority = request.CreateGoalDto.Priority,
            TargetDate = request.CreateGoalDto.TargetDate,
            Status = Domain.Enums.GoalStatus.NotStarted,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdGoal = await _goalRepository.AddAsync(goal);

        return goal;
    }
}
