using MediatR;
using Domain.Entities;
using Application.CQRS.Features.Commands.Tasks;
using Application.IRepositories;

namespace Application.CQRS.Features.Handlers.Tasks;

public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, TaskItem>
{
    private readonly ITaskRepository _taskRepository;

    public CreateTaskCommandHandler(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<TaskItem> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Title = request.Title,
            Description = request.Description ?? string.Empty,
            CompletedAt = request.DueDate,
            StartedAt = request.StartedAt,
            Priority = request.Priority,
            Status = request.Status,
            EstimatedHours = request.EstimatedHours,
            ActualHours = 0,
            CompletionPercentage = 0,
            Tags = request.Tags ?? Array.Empty<string>(),
            AISuggestions = null,
            EnergyLevel = request.EnergyLevel,
            FocusTimeMinutes = request.FocusTimeMinutes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return await _taskRepository.AddAsync(task);
    }
}
