namespace Application.DTOs;

/// <summary>
/// Habit analytics built from stored habit fields and real HabitEntry rows
/// over rolling 7-day and 30-day windows.
/// </summary>
public class HabitSummaryDto
{
    public Guid UserId { get; set; }
    public DateTime GeneratedAt { get; set; }

    public int TotalHabits { get; set; }
    public int ActiveHabits { get; set; }
    public int HabitsOnCurrentStreak { get; set; }
    public double AverageCompletionRate { get; set; }

    public List<HabitStatDto> Habits { get; set; } = new();
}

public class HabitStatDto
{
    public Guid HabitId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int CurrentStreak { get; set; }
    public int BestStreak { get; set; }
    public double CompletionRate { get; set; }
    public DateTime? LastCompletedAt { get; set; }
    public int EntriesLast7Days { get; set; }
    public int EntriesLast30Days { get; set; }
}
