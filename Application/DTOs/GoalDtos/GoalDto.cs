using Domain.Enums;

namespace Application.DTOs.GoalDtos;

/// <summary>
/// Represents a goal data transfer object for API responses.
/// </summary>
/// <param name="Id">The unique identifier of the goal</param>
/// <param name="Title">The goal title</param>
/// <param name="Description">The goal description</param>
/// <param name="Category">The goal category</param>
/// <param name="Status">The current status of the goal</param>
/// <param name="Priority">The priority level (1-5)</param>
/// <param name="TargetDate">The target completion date</param>
/// <param name="CreatedAt">When the goal was created</param>
/// <param name="CompletedAt">When the goal was completed</param>
/// <param name="Progress">The completion progress percentage</param>
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
