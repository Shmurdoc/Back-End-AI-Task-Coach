using Application.CQRS.Commands.Goals;
using Application.DTOs.GoalDtos;
using Application.IRepositories;
using Domain.Entities;
using MediatR;

namespace Application.CQRS.Handlers.Goals;

public class CreateGoalCommandHandler : IRequestHandler<CreateGoalCommand, GoalDto>
{
    private readonly IGoalRepository _goalRepository;
    public CreateGoalCommandHandler(IGoalRepository goalRepository)
    {
        _goalRepository = goalRepository;
    }

    public async Task<GoalDto> Handle(CreateGoalCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        var goal = new Goal
        {
            Id = Guid.NewGuid(),
            UserId = dto.UserId,
            Title = dto.Title,
            Description = dto.Description ?? string.Empty,
            Category = dto.Category,
            Priority = dto.Priority,
            TargetDate = dto.TargetDate,
            Status = Domain.Enums.GoalStatus.NotStarted,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _goalRepository.AddAsync(goal);
        await _goalRepository.SaveChangesAsync(cancellationToken);
        return new GoalDto(
            goal.Id, goal.Title, goal.Description, goal.Category, goal.Status,
            goal.Priority, goal.TargetDate, goal.CreatedAt, goal.CompletedAt, 0
        );
    }
}
