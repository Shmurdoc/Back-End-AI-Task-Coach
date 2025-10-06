using Application.CQRS.Commands.Tasks;
using Application.DTOs.TaskDtos;
using Application.IRepositories;
using Domain.Entities;
using MediatR;

namespace Application.CQRS.Handlers.Tasks;

public class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand, TaskDto>
{
    private readonly ITaskRepository _taskRepository;
    public UpdateTaskCommandHandler(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<TaskDto> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _taskRepository.GetByIdAsync(request.TaskId);
        if (task == null) throw new InvalidOperationException("Task not found.");
        var dto = request.Dto;
        if (dto.Title is not null) task.Title = dto.Title;
        if (dto.Description is not null) task.Description = dto.Description;
        if (dto.Status is not null) task.Status = dto.Status.Value;
        if (dto.Priority is not null) task.Priority = dto.Priority.Value;
        if (dto.EstimatedHours is not null) task.EstimatedHours = dto.EstimatedHours.Value;
        if (dto.ActualHours is not null) task.ActualHours = dto.ActualHours.Value;
        if (dto.CompletionPercentage is not null) task.CompletionPercentage = dto.CompletionPercentage.Value;
        if (dto.CompletedAt is not null) task.CompletedAt = dto.CompletedAt;
        if (dto.StartedAt is not null) task.StartedAt = dto.StartedAt;
        if (dto.Tags is not null) task.Tags = dto.Tags;
        if (dto.AISuggestions is not null) task.AISuggestions = dto.AISuggestions;
        if (dto.Dependencies is not null) task.Dependencies = dto.Dependencies;
        if (dto.EnergyLevel is not null) task.EnergyLevel = dto.EnergyLevel.Value;
        if (dto.FocusTimeMinutes is not null) task.FocusTimeMinutes = dto.FocusTimeMinutes.Value;
        if (dto.GoalId is not null) task.GoalId = dto.GoalId;
        task.UpdatedAt = DateTime.UtcNow;
        var updated = await _taskRepository.UpdateAsync(task);
        return new TaskDto(
            updated.Id,
            updated.Title,
            updated.Description,
            updated.Status,
            updated.Priority,
            updated.EstimatedHours,
            updated.ActualHours,
            updated.CompletionPercentage,
            updated.CompletedAt,
            updated.StartedAt,
            updated.Tags,
            updated.AISuggestions,
            updated.Dependencies,
            updated.EnergyLevel,
            updated.FocusTimeMinutes,
            updated.UserId,
            updated.GoalId,
            updated.CreatedAt,
            updated.UpdatedAt
        );
    }
}
