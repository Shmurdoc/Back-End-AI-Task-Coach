using Application.DTOs.TaskDtos;
using Application.IRepositories;
using Application.IService;
using Domain.Entities;
using FluentValidation;

namespace Application.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateTaskDto> _createValidator;
    private readonly IValidator<UpdateTaskDto> _updateValidator;
    private readonly IUserContext _userContext;

    public TaskService(
        ITaskRepository taskRepository,
        IUnitOfWork unitOfWork,
        IValidator<CreateTaskDto> createValidator,
        IValidator<UpdateTaskDto> updateValidator,
        IUserContext userContext)
    {
        _taskRepository = taskRepository;
        _unitOfWork = unitOfWork;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _userContext = userContext;
    }

    public async Task<TaskDto?> GetTaskByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task == null || task.UserId != _userContext.GetCurrentUserId()) return null;
        return MapToDto(task);
    }

    public async Task<IEnumerable<TaskDto>> GetUserTasksAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        if (userId != _userContext.GetCurrentUserId()) return Enumerable.Empty<TaskDto>();
        var tasks = await _taskRepository.GetUserTasksAsync(userId);
        return tasks.Select(MapToDto);
    }

    public async Task<TaskDto> CreateTaskAsync(CreateTaskDto dto, CancellationToken cancellationToken = default)
    {
        await _createValidator.ValidateAndThrowAsync(dto, cancellationToken);

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
            UserId = _userContext.GetCurrentUserId(),
            GoalId = dto.GoalId
        };

        var created = await _taskRepository.AddAsync(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(created);
    }

    public async Task<TaskDto> UpdateTaskAsync(Guid id, UpdateTaskDto dto, CancellationToken cancellationToken = default)
    {
        await _updateValidator.ValidateAndThrowAsync(dto, cancellationToken);

        var task = await _taskRepository.GetByIdAsync(id)
            ?? throw new InvalidOperationException("Task not found.");
        if (task.UserId != _userContext.GetCurrentUserId())
            throw new InvalidOperationException("Task not found.");

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

        var updated = await _taskRepository.UpdateAsync(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(updated);
    }

    public async Task<bool> DeleteTaskAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task == null || task.UserId != _userContext.GetCurrentUserId())
            return false;

        var deleted = await _taskRepository.DeleteAsync(id);
        if (deleted)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        return deleted;
    }

    private static TaskDto MapToDto(TaskItem task) => new(
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
        task.UpdatedAt);
}
