using Application.CQRS.Features.Commands.Habits;
using Application.CQRS.Features.Commands.Tasks;
using Application.IService;
using MediatR;

namespace Application.CQRS.Features.Handlers.Tasks;

public class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand, bool>
{
    private readonly ITaskService _taskService;

    public DeleteTaskCommandHandler(ITaskService taskService)
    {
        _taskService = taskService;
    }

    public async Task<bool> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
    {
        return await _taskService.DeleteTaskAsync(request.Id, cancellationToken);
    }
}
