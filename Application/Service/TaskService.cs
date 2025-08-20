using Application.DTOs.TaskDtos;
using Application.IRepositories;
using Application.IService;
using Domain.Entities;

namespace Application.Service;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;

    public TaskService(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<TaskDto> CreateTaskAsync(CreateTaskDto dto, CancellationToken cancellationToken)
    {
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
            UserId = Guid.Empty, // Set this from context if needed
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

    public async Task<TaskDto?> GetTaskByIdAsync(Guid taskId, CancellationToken cancellationToken)
    {
        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task == null) return null;
        return new TaskDto(
            task.Id,
            task.Title,
            task.Description,
            task.Status,
            task.Priority,
            task.EstimatedHours,
            task.ActualHours,
            task.CompletionPercentage,
            task.CompletedAt,
            task.StartedAt,
            task.Tags,
            task.AISuggestions,
            task.Dependencies,
            task.EnergyLevel,
            task.FocusTimeMinutes,
            task.UserId,
            task.GoalId,
            task.CreatedAt,
            task.UpdatedAt
        );
    }

    public async Task<TaskDto> UpdateTaskAsync(Guid taskId, TaskDto dto, CancellationToken cancellationToken)
    {
        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task == null) throw new InvalidOperationException("Task not found.");

        task.Title = dto.Title;
        task.Description = dto.Description;
        task.Status = dto.Status;
        task.Priority = dto.Priority;
        task.EstimatedHours = dto.EstimatedHours;
        task.ActualHours = dto.ActualHours;
        task.CompletionPercentage = dto.CompletionPercentage;
        task.CompletedAt = dto.CompletedAt;
        task.StartedAt = dto.StartedAt;
        task.Tags = dto.Tags;
        task.AISuggestions = dto.AISuggestions;
        task.Dependencies = dto.Dependencies;
        task.EnergyLevel = dto.EnergyLevel;
        task.FocusTimeMinutes = dto.FocusTimeMinutes;
        task.UserId = dto.UserId;
        task.GoalId = dto.GoalId;
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

    public async Task<bool> DeleteTaskAsync(Guid taskId, CancellationToken cancellationToken)
    {
        await _taskRepository.DeleteAsync(taskId);
        return true;
    }

    public async Task<IEnumerable<TaskDto>> GetUserTasksAsync(Guid userId)
    {
        var tasks = await _taskRepository.GetUserTasksAsync(userId);
        return tasks.Select(task => new TaskDto(
            task.Id,
            task.Title,
            task.Description,
            task.Status,
            task.Priority,
            task.EstimatedHours,
            task.ActualHours,
            task.CompletionPercentage,
            task.CompletedAt,
            task.StartedAt,
            task.Tags,
            task.AISuggestions,
            task.Dependencies,
            task.EnergyLevel,
            task.FocusTimeMinutes,
            task.UserId,
            task.GoalId,
            task.CreatedAt,
            task.UpdatedAt
        ));
    }

    public async Task<TaskDto> UpdateTaskAsync(Guid taskId, UpdateTaskDto dto, CancellationToken cancellationToken)
    {
        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task == null) throw new InvalidOperationException("Task not found.");

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
