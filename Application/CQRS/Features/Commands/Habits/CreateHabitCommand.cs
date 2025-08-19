using MediatR;
using Domain.Entities;
using Domain.Enums;

namespace Application.CQRS.Features.Commands.Habits;

public record CreateHabitCommand(
    string Name,
    string Description,
    HabitFrequency Frequency,
    TimeSpan? PreferredTime,
    int TargetCount,
    string Unit,
    string Color,
    string Icon,
    string Motivation,
    HabitCategory Category,
    string[] Triggers,
    string[] Rewards,
    int DifficultyLevel,
    string? EnvironmentFactors
) : IRequest<Habit>;
