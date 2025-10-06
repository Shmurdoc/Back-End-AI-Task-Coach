using Domain.Enums;

namespace Application.DTOs.TaskDtos;

public record CreateTaskDto(
    Guid UserId,
    string Title,
    string? Description,
    TaskItemStatus Status,
    TaskPriority Priority,
    double EstimatedHours,
    DateTime? DueDate,
    string[]? Tags,
    Guid[]? Dependencies,
    int EnergyLevel,
    int FocusTimeMinutes,
    Guid? GoalId
);
