using MediatR;
using Domain.Entities;
using Application.CQRS.Features.Queries.Tasks;
using Application.IRepositories;
using Application.DTOs.TaskDtos;
using Application.IService;

namespace Application.CQRS.Features.Handlers.Tasks;

public class GetUserTasksQueryHandler : IRequestHandler<GetUserTasksQuery, IEnumerable<TaskDto>>
{
    private readonly ITaskService _taskService;

    public GetUserTasksQueryHandler(ITaskService taskService)
    {
        _taskService = taskService;
    }

    public async Task<IEnumerable<TaskDto>> Handle(GetUserTasksQuery request, CancellationToken cancellationToken)
    {
        return await _taskService.GetUserTasksAsync(request.UserId);    
    }
}
