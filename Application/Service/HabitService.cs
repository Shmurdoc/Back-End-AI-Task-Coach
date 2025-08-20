using Application.DTOs.HabitDtos;
using Application.IRepositories;
using Application.IService;
using Domain.Entities;

namespace Application.Service;

public class HabitService : IHabitService
{
    private readonly IHabitRepository _habitRepository;
    private readonly IUserContext _userContext;

    public HabitService(IHabitRepository habitRepository, IUserContext userContext)
    {
        _habitRepository = habitRepository;
        _userContext = userContext;
    }

    public async Task<HabitDto> CreateHabitAsync(CreateHabitDto request, CancellationToken cancellationToken)
    {
        var userId = _userContext.GetCurrentUserId();
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
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var createdHabit = await _habitRepository.AddAsync(habit);
        await _habitRepository.SaveChangesAsync(cancellationToken);

        return new HabitDto(
            createdHabit.Id,
            createdHabit.Name,
            createdHabit.Description,
            createdHabit.Frequency,
            createdHabit.PreferredTime,
            createdHabit.TargetCount,
            createdHabit.Unit,
            createdHabit.Color,
            createdHabit.Icon,
            createdHabit.Motivation,
            createdHabit.CurrentStreak,
            createdHabit.BestStreak,
            createdHabit.CompletionRate,
            createdHabit.IsActive,
            createdHabit.Category,
            createdHabit.Triggers,
            createdHabit.Rewards,
            createdHabit.AIInsights,
            createdHabit.DifficultyLevel,
            createdHabit.EnvironmentFactors,
            createdHabit.LastCompletedAt,
            createdHabit.UserId,
            createdHabit.CreatedAt,
            createdHabit.UpdatedAt
        );
    }

    public async Task<IEnumerable<HabitDto>> GetUserHabitsAsync(Guid userId)
    {
        var habits = await _habitRepository.GetUserHabitsAsync(userId);
        return habits.Select(habit => new HabitDto(
            habit.Id,
            habit.Name,
            habit.Description,
            habit.Frequency,
            habit.PreferredTime,
            habit.TargetCount,
            habit.Unit,
            habit.Color,
            habit.Icon,
            habit.Motivation,
            habit.CurrentStreak,
            habit.BestStreak,
            habit.CompletionRate,
            habit.IsActive,
            habit.Category,
            habit.Triggers,
            habit.Rewards,
            habit.AIInsights,
            habit.DifficultyLevel,
            habit.EnvironmentFactors,
            habit.LastCompletedAt,
            habit.UserId,
            habit.CreatedAt,
            habit.UpdatedAt
        ));
    }

    public async Task TrackHabitProgressAsync(Guid habitId, int value)
    {
        var habit = await _habitRepository.GetByIdAsync(habitId);
        if (habit == null) throw new InvalidOperationException("Habit not found.");
        habit.TrackProgress(value);
        await _habitRepository.UpdateAsync(habit);
    }

    public async Task<HabitDto?> GetHabitByIdAsync(Guid habitId, CancellationToken cancellationToken)
    {
        var habit = await _habitRepository.GetByIdAsync(habitId);
        if (habit == null) return null;
        return new HabitDto(
            habit.Id,
            habit.Name,
            habit.Description,
            habit.Frequency,
            habit.PreferredTime,
            habit.TargetCount,
            habit.Unit,
            habit.Color,
            habit.Icon,
            habit.Motivation,
            habit.CurrentStreak,
            habit.BestStreak,
            habit.CompletionRate,
            habit.IsActive,
            habit.Category,
            habit.Triggers,
            habit.Rewards,
            habit.AIInsights,
            habit.DifficultyLevel,
            habit.EnvironmentFactors,
            habit.LastCompletedAt,
            habit.UserId,
            habit.CreatedAt,
            habit.UpdatedAt
        );
    }

    public async Task<HabitDto> UpdateHabitAsync(Guid habitId, UpdateHabitDto dto, CancellationToken cancellationToken)
    {
        var habit = await _habitRepository.GetByIdAsync(habitId);
        if (habit == null) throw new InvalidOperationException("Habit not found.");

        if (dto.Name is not null) habit.Name = dto.Name;
        if (dto.Description is not null) habit.Description = dto.Description;
        if (dto.IsActive is not null) habit.IsActive = dto.IsActive.Value;
        if (dto.TargetStreak is not null) habit.BestStreak = dto.TargetStreak.Value;
        if (dto.Priority is not null) habit.DifficultyLevel = dto.Priority.Value;
        if (dto.StartDate is not null) habit.CreatedAt = dto.StartDate.Value;
        if (dto.EndDate is not null) habit.UpdatedAt = dto.EndDate.Value;

        var updated = await _habitRepository.UpdateAsync(habit);
        return new HabitDto(
            updated.Id,
            updated.Name,
            updated.Description,
            updated.Frequency,
            updated.PreferredTime,
            updated.TargetCount,
            updated.Unit,
            updated.Color,
            updated.Icon,
            updated.Motivation,
            updated.CurrentStreak,
            updated.BestStreak,
            updated.CompletionRate,
            updated.IsActive,
            updated.Category,
            updated.Triggers,
            updated.Rewards,
            updated.AIInsights,
            updated.DifficultyLevel,
            updated.EnvironmentFactors,
            updated.LastCompletedAt,
            updated.UserId,
            updated.CreatedAt,
            updated.UpdatedAt
        );
    }

    public async Task<bool> DeleteHabitAsync(Guid habitId, CancellationToken cancellationToken)
    {
        await _habitRepository.DeleteAsync(habitId);
        return true;
    }
}
