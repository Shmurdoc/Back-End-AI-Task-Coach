using Domain.Enums;

namespace Application.DTOs.HabitDtos;

public record UpdateHabitDto(
    string? Name = null,
    string? Description = null,
    HabitFrequency? Frequency = null,
    TimeSpan? PreferredTime = null,
    int? TargetCount = null,
    string? Unit = null,
    string? Color = null,
    string? Icon = null,
    string? Motivation = null,
    HabitCategory? Category = null,
    string[]? Triggers = null,
    string[]? Rewards = null,
    int? DifficultyLevel = null,
    string? EnvironmentFactors = null,
    bool? IsActive = null
);
