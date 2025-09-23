using Application.IRepositories;
using Domain.Entities;
using Domain.Enums.extension.helper;
using Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// Habit repository implementation
/// </summary>
public class HabitRepository : IHabitRepository
{
    private readonly ApplicationDbContext _context;

    public HabitRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Habit?> GetByIdAsync(Guid id)
    {
        return await _context.Habits
            .Include(h => h.Entries)
            .Include(h => h.Analytics)
            .FirstOrDefaultAsync(h => h.Id == id);
    }

    public async Task<IEnumerable<Habit>> GetUserHabitsAsync(Guid userId)
    {
        return await _context.Habits
            .Where(h => h.UserId == userId)
            .Include(h => h.Entries)
            .OrderBy(h => h.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Habit>> GetActiveUserHabitsAsync(Guid userId)
    {
        return await _context.Habits
            .Where(h => h.UserId == userId && h.IsActive)
            .Include(h => h.Entries)
            .OrderBy(h => h.Name)
            .ToListAsync();
    }

    public async Task<Habit> AddAsync(Habit habit)
    {
        habit.CreatedAt = DateTime.UtcNow;
        habit.UpdatedAt = DateTime.UtcNow;
        _context.Habits.Add(habit);
        await _context.SaveChangesAsync();
        return habit;
    }

    public async Task<Habit> UpdateAsync(Habit habit)
    {
        habit.UpdatedAt = DateTime.UtcNow;
        _context.Habits.Update(habit);
        await _context.SaveChangesAsync();
        return habit;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var habit = await _context.Habits.FindAsync(id);
        if (habit != null)
        {
            habit.IsActive = false;
            habit.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
        return false;
    }

    public async Task<HabitEntry> AddHabitEntryAsync(HabitEntry entry)
    {
        entry.CreatedAt = DateTime.UtcNow;
        _context.HabitEntries.Add(entry);
        await _context.SaveChangesAsync();
        return entry;
    }

    public async Task<IEnumerable<HabitEntry>> GetHabitEntriesAsync(Guid habitId, DateTime startDate, DateTime endDate)
    {
        return await _context.HabitEntries
            .Where(e => e.HabitId == habitId && e.Date >= startDate.Date && e.Date <= endDate.Date)
            .OrderBy(e => e.Date)
            .ToListAsync();
    }

    public async Task UpdateStreakAsync(Guid habitId)
    {
        var habit = await _context.Habits.FindAsync(habitId);
        if (habit == null) return;

        var recentEntries = await _context.HabitEntries
            .Where(e => e.HabitId == habitId && e.Status == CompletionStatus.Completed)
            .OrderByDescending(e => e.Date)
            .Take(30)
            .ToListAsync();

        if (!recentEntries.Any())
        {
            habit.CurrentStreak = 0;
            await _context.SaveChangesAsync();
            return;
        }

        // Calculate current streak
        var currentStreak = 0;
        var yesterday = DateTime.Today.AddDays(-1);

        foreach (var entry in recentEntries)
        {
            if (entry.Date.Date == yesterday.Date)
            {
                currentStreak++;
                yesterday = yesterday.AddDays(-1);
            }
            else
            {
                break;
            }
        }

        habit.CurrentStreak = currentStreak;
        habit.BestStreak = Math.Max(habit.BestStreak, currentStreak);
        habit.LastCompletedAt = recentEntries.First().Date;

        await _context.SaveChangesAsync();
    }

    public async Task<Dictionary<DayOfWeek, double>> GetCompletionRateByDayOfWeekAsync(Guid habitId)
    {
        var entries = await _context.HabitEntries
            .Where(e => e.HabitId == habitId)
            .ToListAsync();

        var result = new Dictionary<DayOfWeek, double>();

        foreach (DayOfWeek dayOfWeek in Enum.GetValues<DayOfWeek>())
        {
            var dayEntries = entries.Where(e => e.Date.DayOfWeek == dayOfWeek).ToList();
            var completedCount = dayEntries.Count(e => e.Status == CompletionStatus.Completed);
            var totalCount = dayEntries.Count;

            result[dayOfWeek] = totalCount > 0 ? (double)completedCount / totalCount * 100 : 0;
        }

        return result;
    }

    public async Task<Dictionary<int, double>> GetCompletionRateByHourAsync(Guid habitId)
    {
        var entries = await _context.HabitEntries
            .Where(e => e.HabitId == habitId && e.TimeSpent.HasValue)
            .ToListAsync();

        var result = new Dictionary<int, double>();

        for (int hour = 0; hour < 24; hour++)
        {
            var hourEntries = entries.Where(e => e.CreatedAt.Hour == hour).ToList();
            var completedCount = hourEntries.Count(e => e.Status == CompletionStatus.Completed);
            var totalCount = hourEntries.Count;

            result[hour] = totalCount > 0 ? (double)completedCount / totalCount * 100 : 0;
        }

        return result;
    }

    public async Task CreateAnalyticsAsync(Guid habitId)
    {
        var habit = await _context.Habits.FindAsync(habitId);
        if (habit == null) return;

        var entries = await _context.HabitEntries
            .Where(e => e.HabitId == habitId)
            .Where(e => e.Date >= DateTime.Today.AddDays(-30))
            .ToListAsync();

        var completedEntries = entries.Where(e => e.Status == CompletionStatus.Completed).ToList();
        var weeklyCompletions = completedEntries.Count(e => e.Date >= DateTime.Today.AddDays(-7));
        var monthlyCompletions = completedEntries.Count;
        var averageCompletionTime = completedEntries.Where(e => e.TimeSpent.HasValue)
            .Select(e => e.TimeSpent!.Value.TotalMinutes)
            .DefaultIfEmpty(0)
            .Average();

        var analytics = new HabitAnalytics
        {
            Id = Guid.NewGuid(),
            HabitId = habitId,
            AnalysisDate = DateTime.UtcNow,
            WeeklyCompletions = weeklyCompletions,
            MonthlyCompletions = monthlyCompletions,
            AverageCompletionTime = averageCompletionTime,
            TrendDirection = CalculateTrendDirection(entries),
            BestPerformanceFactors = "Consistent daily practice",
            ChallengeFactors = "Weekend gaps",
            AIRecommendations = "Focus on weekend consistency"
        };

        _context.HabitAnalytics.Add(analytics);
        await _context.SaveChangesAsync();
    }

    private static double CalculateTrendDirection(List<HabitEntry> entries)
    {
        if (entries.Count < 7) return 0;

        var recentWeek = entries.Where(e => e.Date >= DateTime.Today.AddDays(-7))
            .Count(e => e.Status == CompletionStatus.Completed);
        var previousWeek = entries.Where(e => e.Date >= DateTime.Today.AddDays(-14) && e.Date < DateTime.Today.AddDays(-7))
            .Count(e => e.Status == CompletionStatus.Completed);

        if (previousWeek == 0) return recentWeek > 0 ? 1.0 : 0.0;

        var change = (double)(recentWeek - previousWeek) / previousWeek;
        return Math.Max(-1.0, Math.Min(1.0, change));
    }

    public async Task<int>SaveChangesAsync(CancellationToken cancellationToken)
    {
      return await _context.SaveChangesAsync(cancellationToken);
    }

    // Gamification methods
    public async Task<int> GetHabitCountByUserAsync(Guid userId)
    {
        return await _context.Habits
            .Where(h => h.UserId == userId)
            .CountAsync();
    }

    public async Task<int> GetActiveHabitCountByUserAsync(Guid userId)
    {
        return await _context.Habits
            .Where(h => h.UserId == userId && h.IsActive)
            .CountAsync();
    }
}
