using Application.CQRS.Features.Commands.Tasks;
using Application.DTOs.TaskDtos;
using Application.IService;
using MediatR;

namespace Application.CQRS.Features.Handlers.Tasks;

public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, TaskDto>
{
    private readonly ITaskService _taskService;

    public CreateTaskCommandHandler(ITaskService taskService)
    {
        _taskService = taskService;
    }

    public async Task<TaskDto> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        return await _taskService.CreateTaskAsync(request.Dto, cancellationToken);
    }
    
}
