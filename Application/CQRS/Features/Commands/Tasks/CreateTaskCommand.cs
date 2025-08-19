using MediatR;
using Domain.Entities;
using Domain.Enums;

namespace Application.CQRS.Features.Commands.Tasks;

public record CreateTaskCommand(
    Guid UserId,
    string Title,
    string Description,
    DateTime DueDate,
    DateTime StartedAt,
    TaskPriority Priority,
    string Category,
    TaskItemStatus Status,
    double EstimatedHours,
    string[] Tags,
    TimeSpan EstimatedDuration,
    int EnergyLevel,
    int FocusTimeMinutes,
    bool IsRecurring = false,
    string RecurrencePattern = ""
) : IRequest<TaskItem>;
