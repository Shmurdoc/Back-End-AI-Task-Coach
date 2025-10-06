using Domain.Enums;

namespace Application.DTOs.GoalDtos;

/// <summary>
/// Represents a goal data transfer object for API responses.
/// </summary>
public record GoalDto(
    Guid Id,
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
