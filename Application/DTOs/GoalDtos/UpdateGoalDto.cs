using Domain.Enums;

namespace Application.DTOs.GoalDtos;

public record UpdateGoalDto(
    string? Title = null,
    string? Description = null,
    GoalCategory? Category = null,
    GoalStatus? Status = null,
    int? Priority = null,
    DateTime? TargetDate = null,
    DateTime? CompletedAt = null,
    double? Progress = null
);
