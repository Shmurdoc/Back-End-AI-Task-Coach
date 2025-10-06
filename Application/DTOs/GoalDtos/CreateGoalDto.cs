using Domain.Enums;

namespace Application.DTOs.GoalDtos;

public record CreateGoalDto(
    Guid UserId,
    string Title,
    string? Description,
    GoalCategory Category,
    GoalStatus Status,
    int Priority,
    DateTime? TargetDate,
    DateTime CreatedAt,
    DateTime? CompletedAt,
    double Progress
);

