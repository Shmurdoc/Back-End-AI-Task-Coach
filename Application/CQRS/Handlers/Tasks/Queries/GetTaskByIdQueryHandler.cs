using Application.CQRS.Queries.Tasks;
using Application.DTOs.TaskDtos;
using Application.IRepositories;
using MediatR;

namespace Application.CQRS.Handlers.Tasks.Queries;

public class GetTaskByIdQueryHandler : IRequestHandler<GetTaskByIdQuery, TaskDto?>
{
    private readonly ITaskRepository _taskRepository;
    public GetTaskByIdQueryHandler(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<TaskDto?> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
    {
        var task = await _taskRepository.GetByIdAsync(request.TaskId);
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
}
