using System;
using Domain.Enums;

namespace Application.DTOs.HabitDtos;

public record CreateHabitDto(
    string Name,
    string? Description,
    HabitFrequency Frequency,
    TimeSpan? PreferredTime,
    int TargetCount,
    string Unit,
    string Color,
    string Icon,
    string? Motivation,
    HabitCategory Category,
    string[]? Triggers,
    string[]? Rewards,
    int DifficultyLevel,
    string? EnvironmentFactors
);
       