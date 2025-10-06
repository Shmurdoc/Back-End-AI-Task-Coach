using Application.CQRS.Features.Queries.Tasks;
using Application.DTOs.TaskDtos;
using Application.IService;
using MediatR;

namespace Application.CQRS.Features.Handlers.Tasks;

public class GetTaskByIdQueryHandler : IRequestHandler<GetTasksByIdQuery, TaskDto?>
{
    private readonly ITaskService _taskService;

    public GetTaskByIdQueryHandler(ITaskService taskService)
    {
        _taskService = taskService;
    }

    public async Task<TaskDto?> Handle(GetTasksByIdQuery request, CancellationToken cancellationToken)
    {
        return await _taskService.GetTaskByIdAsync(request.TaskId, cancellationToken);
    }
}
