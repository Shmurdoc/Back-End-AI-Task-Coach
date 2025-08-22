using Application.CQRS.Commands.Goals;
using Application.DTOs.GoalDtos;
using Application.IRepositories;
using MediatR;

namespace Application.CQRS.Handlers.Goals;

public class UpdateGoalCommandHandler : IRequestHandler<UpdateGoalCommand, GoalDto>
{
    private readonly IGoalRepository _goalRepository;
    public UpdateGoalCommandHandler(IGoalRepository goalRepository)
    {
        _goalRepository = goalRepository;
    }

    public async Task<GoalDto> Handle(UpdateGoalCommand request, CancellationToken cancellationToken)
    {
        var goal = await _goalRepository.GetByIdAsync(request.GoalId);
        if (goal == null) throw new InvalidOperationException("Goal not found.");
        var dto = request.Dto;
        if (dto.Title is not null) goal.Title = dto.Title;
        if (dto.Description is not null) goal.Description = dto.Description;
        if (dto.Category is not null) goal.Category = dto.Category.Value;
        if (dto.Status is not null) goal.Status = dto.Status.Value;
        if (dto.Priority is not null) goal.Priority = dto.Priority.Value;
        if (dto.TargetDate is not null) goal.TargetDate = dto.TargetDate;
        if (dto.CompletedAt is not null) goal.CompletedAt = dto.CompletedAt;
        if (dto.Progress is not null) goal.Progress = (int)dto.Progress.Value;
        var updated = await _goalRepository.UpdateAsync(goal);
        return new GoalDto(
            updated.Id,
            updated.Title,
            updated.Description,
            updated.Category,
            updated.Status,
            updated.Priority,
            updated.TargetDate,
            updated.CreatedAt,
            updated.CompletedAt,
            updated.Progress
        );
    }
}
