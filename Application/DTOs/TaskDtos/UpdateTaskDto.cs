using System;
using Domain.Enums;

namespace Application.DTOs.TaskDtos;

public record UpdateTaskDto(
    string? Title = null,
    string? Description = null,
    TaskItemStatus? Status = null,
    TaskPriority? Priority = null,
    double? EstimatedHours = null,
    double? ActualHours = null,
    int? CompletionPercentage = null,
    DateTime? CompletedAt = null,
    DateTime? StartedAt = null,
    string[]? Tags = null,
    string? AISuggestions = null,
    Guid[]? Dependencies = null,
    int? EnergyLevel = null,
    int? FocusTimeMinutes = null,
    Guid? GoalId = null
);
        