using Application.CQRS.Features.Commands.Habits;
using Application.CQRS.Features.Commands.Tasks;
using Application.DTOs.TaskDtos;
using Application.IService;
using MediatR;

namespace Application.CQRS.Features.Handlers.Tasks;

public class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand, TaskDto>
{
    private readonly ITaskService _taskService;

    public UpdateTaskCommandHandler(ITaskService taskService)
    {
        _taskService = taskService;
    }

    public async Task<TaskDto> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        return await _taskService.UpdateTaskAsync(request.Id, request.Dto, cancellationToken);
    }
}
