using MediatR;
using Domain.Entities;
using Application.CQRS.Features.Commands.Habits;
using Application.IRepositories;

namespace Application.CQRS.Features.Handlers.Habits;

public class CreateHabitCommandHandler : IRequestHandler<CreateHabitCommand, Habit>
{
    private readonly IHabitRepository _habitRepository;

    public CreateHabitCommandHandler(IHabitRepository habitRepository)
    {
        _habitRepository = habitRepository;
    }

    public async Task<Habit> Handle(CreateHabitCommand request, CancellationToken cancellationToken)
    {
        var habit = new Habit
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description ?? string.Empty,
            Frequency = request.Frequency,
            PreferredTime = request.PreferredTime,
            TargetCount = request.TargetCount,
            Unit = request.Unit,
            Color = request.Color,
            Icon = request.Icon,
            Motivation = request.Motivation ?? string.Empty,
            CurrentStreak = 0,
            BestStreak = 0,
            CompletionRate = 0,
            IsActive = true,
            Category = request.Category,
            Triggers = request.Triggers ?? Array.Empty<string>(),
            Rewards = request.Rewards ?? Array.Empty<string>(),
            AIInsights = null,
            DifficultyLevel = request.DifficultyLevel,
            EnvironmentFactors = request.EnvironmentFactors,
            LastCompletedAt = null,
            UserId = request.UserId, // find userId
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return await _habitRepository.AddAsync(habit);
    }
}
