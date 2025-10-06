using Application.CQRS.Queries.Tasks;
using Application.DTOs.TaskDtos;
using Application.IRepositories;
using MediatR;

namespace Application.CQRS.Handlers.Tasks.Queries;

public class GetUserTasksQueryHandler : IRequestHandler<GetUserTasksQuery, IEnumerable<TaskDto>>
{
    private readonly ITaskRepository _taskRepository;
    public GetUserTasksQueryHandler(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<IEnumerable<TaskDto>> Handle(GetUserTasksQuery request, CancellationToken cancellationToken)
    {
        var tasks = await _taskRepository.GetUserTasksAsync(request.UserId);
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
}
