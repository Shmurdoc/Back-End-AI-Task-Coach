using Domain.Entities;

namespace Application.IRepositories;

public interface IHabitRepository
{
    Task<Habit?> GetByIdAsync(Guid id);
    Task<IEnumerable<Habit>> GetUserHabitsAsync(Guid userId);
    Task<IEnumerable<Habit>> GetActiveUserHabitsAsync(Guid userId);
    Task<Habit> AddAsync(Habit habit);
    Task<Habit> UpdateAsync(Habit habit);
    Task<bool> DeleteAsync(Guid id);
    Task<HabitEntry> AddHabitEntryAsync(HabitEntry entry);
    Task<IEnumerable<HabitEntry>> GetHabitEntriesAsync(Guid habitId, DateTime startDate, DateTime endDate);
    Task UpdateStreakAsync(Guid habitId);
    Task<Dictionary<DayOfWeek, double>> GetCompletionRateByDayOfWeekAsync(Guid habitId);
    Task<Dictionary<int, double>> GetCompletionRateByHourAsync(Guid habitId);
    Task CreateAnalyticsAsync(Guid habitId);

    Task<int>SaveChangesAsync(CancellationToken cancellationToken); // Optional, if you want to handle SaveChanges in the repository
}
