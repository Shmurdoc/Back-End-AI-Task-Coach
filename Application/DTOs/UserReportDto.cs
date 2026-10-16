namespace Application.DTOs;

/// <summary>
/// Point-in-time productivity report derived entirely from stored goals,
/// tasks, and habits — no estimated or synthesized figures.
/// </summary>
public class UserReportDto
{
    public Guid UserId { get; set; }
    public DateTime GeneratedAt { get; set; }

    public int TotalGoals { get; set; }
    public int CompletedGoals { get; set; }
    public int OverdueGoals { get; set; }
    public double AverageGoalProgress { get; set; }

    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public double TaskCompletionRate { get; set; }
    public int TasksCompletedLast7Days { get; set; }
    public int GoalLinkedTasks { get; set; }
    public Dictionary<string, int> TasksByPriority { get; set; } = new();

    public int TotalHabits { get; set; }
    public int ActiveHabits { get; set; }
    public int HabitsOnCurrentStreak { get; set; }
}
