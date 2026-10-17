using Application.DTOs.HabitDtos;
using Application.IRepositories;
using Application.IService;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class HabitService : IHabitService
{
    private readonly IHabitRepository _habitRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;

    public HabitService(IHabitRepository habitRepository, IUnitOfWork unitOfWork, IUserContext userContext)
    {
        _habitRepository = habitRepository;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
    }

    public async Task<HabitDto?> CreateHabitAsync(CreateHabitDto request, CancellationToken cancellationToken)
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
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(createdHabit);
    }

    public async Task<IEnumerable<HabitDto>> GetUserHabitsAsync(Guid userId)
    {
        if (userId != _userContext.GetCurrentUserId()) return Enumerable.Empty<HabitDto>();
        var habits = await _habitRepository.GetUserHabitsAsync(userId);
        return habits.Select(MapToDto);
    }

    public async Task<(HabitDto Habit, bool Completed)> TrackHabitProgressAsync(Guid habitId, int value)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(nameof(value), "Progress value must be non-negative.");

        var habit = await _habitRepository.GetByIdAsync(habitId);
        if (habit == null) throw new InvalidOperationException("Habit not found.");
        if (habit.UserId != _userContext.GetCurrentUserId())
            throw new InvalidOperationException("Habit not found.");

        var completed = value >= habit.TargetCount;
        habit.TrackProgress(value);

        // Persist the attempt as a real entry so streak history and the
        // habit analytics endpoints are backed by stored rows instead of
        // reading from data that was never written.
        await _habitRepository.AddHabitEntryAsync(new HabitEntry
        {
            Id = Guid.NewGuid(),
            HabitId = habit.Id,
            Date = DateTime.UtcNow.Date,
            Value = value,
            Status = completed
                ? CompletionStatus.Completed
                : (value > 0 ? CompletionStatus.PartiallyCompleted : CompletionStatus.Skipped),
            Notes = string.Empty
        });

        await _habitRepository.UpdateAsync(habit);
        await _unitOfWork.SaveChangesAsync();

        return (MapToDto(habit), completed);
    }

    public async Task<HabitDto?> GetHabitByIdAsync(Guid habitId, CancellationToken cancellationToken)
    {
        var habit = await _habitRepository.GetByIdAsync(habitId);
        if (habit == null) return null;
        if (habit.UserId != _userContext.GetCurrentUserId()) return null;
        return MapToDto(habit);
    }

    public async Task<HabitDto> UpdateHabitAsync(Guid habitId, UpdateHabitDto dto, CancellationToken cancellationToken)
    {
        var habit = await _habitRepository.GetByIdAsync(habitId);
        if (habit == null) throw new InvalidOperationException("Habit not found.");
        if (habit.UserId != _userContext.GetCurrentUserId())
            throw new InvalidOperationException("Habit not found.");
        if (dto.Name is not null) habit.Name = dto.Name;
        if (dto.Description is not null) habit.Description = dto.Description;
        if (dto.Frequency is not null) habit.Frequency = dto.Frequency.Value;
        if (dto.PreferredTime is not null) habit.PreferredTime = dto.PreferredTime;
        if (dto.TargetCount is not null) habit.TargetCount = dto.TargetCount.Value;
        if (dto.Unit is not null) habit.Unit = dto.Unit;
        if (dto.Color is not null) habit.Color = dto.Color;
        if (dto.Icon is not null) habit.Icon = dto.Icon;
        if (dto.Motivation is not null) habit.Motivation = dto.Motivation;
        if (dto.IsActive is not null) habit.IsActive = dto.IsActive.Value;
        if (dto.Category is not null) habit.Category = dto.Category.Value;
        if (dto.Triggers is not null) habit.Triggers = dto.Triggers;
        if (dto.Rewards is not null) habit.Rewards = dto.Rewards;
        if (dto.DifficultyLevel is not null) habit.DifficultyLevel = dto.DifficultyLevel.Value;
        if (dto.EnvironmentFactors is not null) habit.EnvironmentFactors = dto.EnvironmentFactors;
        habit.UpdatedAt = DateTime.UtcNow;

        var updated = await _habitRepository.UpdateAsync(habit);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(updated);
    }

    public async Task<bool> DeleteHabitAsync(Guid habitId, CancellationToken cancellationToken)
    {
        var existing = await _habitRepository.GetByIdAsync(habitId);
        if (existing == null || existing.UserId != _userContext.GetCurrentUserId())
            return false;

        var deleted = await _habitRepository.DeleteAsync(habitId);
        if (deleted)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        return deleted;
    }

    private static HabitDto MapToDto(Habit habit) => new(
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
        habit.UpdatedAt);
}
