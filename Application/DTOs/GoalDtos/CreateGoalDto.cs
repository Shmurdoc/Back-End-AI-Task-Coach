using Domain.Enums;

namespace Application.DTOs.GoalDtos;

// Only fields the service actually consumes on create. Status always starts
// NotStarted (changes go through UpdateGoalDto); CreatedAt is stamped by
// ApplicationDbContext; CompletedAt/Progress start empty server-side.
// UserId must be present for the EnsureSelfUser ownership check and must
// match the caller's token.
public record CreateGoalDto(
    Guid UserId,
    string Title,
    string? Description,
    GoalCategory Category,
    int Priority,
    DateTime? TargetDate
);
