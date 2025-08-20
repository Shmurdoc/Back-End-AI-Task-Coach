using Domain.Enums;

namespace Application.DTOs.TaskDtos;

public record TaskDto(
    Guid Id,
    string Title,
    string Description,
    TaskItemStatus Status,
    TaskPriority Priority,
    double EstimatedHours,
    double ActualHours,
    int CompletionPercentage,
    DateTime? CompletedAt,
    DateTime? StartedAt,
    string[] Tags,
    string? AISuggestions,
    Guid[] Dependencies,
    int EnergyLevel,
    int FocusTimeMinutes,
    Guid UserId,
    Guid? GoalId,
    DateTime CreatedAt,
    DateTime UpdatedAt
);


