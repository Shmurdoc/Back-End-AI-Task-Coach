using Domain.Common;

namespace Domain.Entities;


/// <summary>
/// Habit analytics for AI pattern recognition
/// </summary>
public class HabitAnalytics : BaseEntity
{
    public DateTime AnalysisDate { get; set; }

    public int WeeklyCompletions { get; set; }

    public int MonthlyCompletions { get; set; }

    public double AverageCompletionTime { get; set; }

    public string? BestPerformanceFactors { get; set; }

    public string? ChallengeFactors { get; set; }

    public double TrendDirection { get; set; } // -1 to 1

    public string? AIRecommendations { get; set; }

    // Foreign key
    public Guid HabitId { get; set; }
    public virtual Habit Habit { get; set; } = null!;
}
