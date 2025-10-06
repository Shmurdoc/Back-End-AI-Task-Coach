using Application.CQRS.Commands.Tasks;
using Application.IRepositories;
using MediatR;

namespace Application.CQRS.Handlers.Tasks;

public class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand, bool>
{
    private readonly ITaskRepository _taskRepository;
    public DeleteTaskCommandHandler(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<bool> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
    {
        await _taskRepository.DeleteAsync(request.TaskId);
        return true;
    }
}
