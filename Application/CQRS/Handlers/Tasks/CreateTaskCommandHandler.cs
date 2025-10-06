using Application.CQRS.Commands.Tasks;
using Application.DTOs.TaskDtos;
using Application.IRepositories;
using Domain.Entities;
using MediatR;

namespace Application.CQRS.Handlers.Tasks;

public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, TaskDto>
{
    private readonly ITaskRepository _taskRepository;
    public CreateTaskCommandHandler(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<TaskDto> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Description = dto.Description ?? string.Empty,
            Status = dto.Status,
            Priority = dto.Priority,
            EstimatedHours = dto.EstimatedHours,
            ActualHours = 0,
            CompletionPercentage = 0,
            CompletedAt = null,
            StartedAt = null,
            Tags = dto.Tags ?? Array.Empty<string>(),
            AISuggestions = null,
            Dependencies = dto.Dependencies ?? Array.Empty<Guid>(),
            EnergyLevel = dto.EnergyLevel,
            FocusTimeMinutes = dto.FocusTimeMinutes,
            UserId = dto.UserId,
            GoalId = dto.GoalId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var created = await _taskRepository.AddAsync(task);
        return new TaskDto(
            created.Id,
            created.Title,
            created.Description,
            created.Status,
            created.Priority,
            created.EstimatedHours,
            created.ActualHours,
            created.CompletionPercentage,
            created.CompletedAt,
            created.StartedAt,
            created.Tags,
            created.AISuggestions,
            created.Dependencies,
            created.EnergyLevel,
            created.FocusTimeMinutes,
            created.UserId,
            created.GoalId,
            created.CreatedAt,
            created.UpdatedAt
        );
    }
}
