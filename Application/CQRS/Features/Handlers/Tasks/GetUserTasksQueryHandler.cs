using MediatR;
using Domain.Entities;
using Application.CQRS.Features.Queries.Tasks;
using Application.IRepositories;

namespace Application.CQRS.Features.Handlers.Tasks;

public class GetUserTasksQueryHandler : IRequestHandler<GetUserTasksQuery, IEnumerable<TaskItem>>
{
    private readonly ITaskRepository _taskRepository;

    public GetUserTasksQueryHandler(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<IEnumerable<TaskItem>> Handle(GetUserTasksQuery request, CancellationToken cancellationToken)
    {
        return await _taskRepository.GetUserTasksAsync(request.UserId);
    }
}
