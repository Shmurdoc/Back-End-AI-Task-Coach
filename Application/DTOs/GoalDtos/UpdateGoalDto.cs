using Domain.Enums;

namespace Application.DTOs.GoalDtos;

public record UpdateGoalDto(
    string? Title,
    string? Description,
    GoalCategory? Category,
    GoalStatus? Status,
    int? Priority,
    DateTime? TargetDate,
    DateTime? CompletedAt,
    double? Progress
);
