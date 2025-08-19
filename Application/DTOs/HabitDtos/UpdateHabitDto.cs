namespace Application.DTOs.HabitDtos;

public record UpdateHabitDto(
    string? Name,
    string? Description,
    bool? IsActive,
    int? TargetStreak,
    int? Priority,
    DateTime? StartDate,
    DateTime? EndDate
);
