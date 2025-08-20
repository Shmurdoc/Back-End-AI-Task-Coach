using System;
using Domain.Enums;

namespace Application.DTOs.HabitDtos;

public record HabitDto(
    Guid Id,
    string Name,
    string Description,
    HabitFrequency Frequency,
    TimeSpan? PreferredTime,
    int TargetCount,
    string Unit,
    string Color,
    string Icon,
    string Motivation,
    int CurrentStreak,
    int BestStreak,
    double CompletionRate,
    bool IsActive,
    HabitCategory Category,
    string[] Triggers,
    string[] Rewards,
    string? AIInsights,
    int DifficultyLevel,
    string? EnvironmentFactors,
    DateTime? LastCompletedAt,
    Guid UserId,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
   