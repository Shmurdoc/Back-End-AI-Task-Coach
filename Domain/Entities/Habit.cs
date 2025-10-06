using Domain.Common;
using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

/// <summary>
/// Habit entity optimized for AI behavior analysis and pattern recognition
/// Tracks consistency, triggers, and environmental factors for intelligent coaching
/// </summary>
public class Habit : BaseEntity
{
    /// <summary>
    /// Habit name
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Detailed habit description
    /// </summary>
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Habit frequency for scheduling
    /// </summary>
    [Required]
    public HabitFrequency Frequency { get; set; }

    /// <summary>
    /// Preferred time of day for habit execution
    /// </summary>
    public TimeSpan? PreferredTime { get; set; }

    /// <summary>
    /// Target count per frequency period (e.g., 3 times per day)
    /// </summary>
    [Range(1, 1000)]
    public int TargetCount { get; set; } = 1;

    /// <summary>
    /// Unit of measurement (e.g., "minutes", "pages", "reps")
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Unit { get; set; } = string.Empty;

    /// <summary>
    /// Color code for UI visualization
    /// </summary>
    [Required]
    [RegularExpression(@"^#[0-9A-Fa-f]{6}$")]
    public string Color { get; set; } = "#3498db";

    /// <summary>
    /// Icon identifier for UI
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// Motivational message for AI coaching
    /// </summary>
    [StringLength(200)]
    public string Motivation { get; set; } = string.Empty;

    /// <summary>
    /// Current streak count for gamification
    /// </summary>
    public int CurrentStreak { get; set; }

    /// <summary>
    /// Best streak achieved
    /// </summary>
    public int BestStreak { get; set; }

    /// <summary>
    /// Overall completion rate (0-100%)
    /// </summary>
    [Range(0, 100)]
    public double CompletionRate { get; set; }

    /// <summary>
    /// Whether the habit is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Habit category for AI classification
    /// </summary>
    public HabitCategory Category { get; set; } = HabitCategory.Personal;

    /// <summary>
    /// Triggers that prompt the habit (JSON array)
    /// </summary>
    public string[] Triggers { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Rewards for completing the habit
    /// </summary>
    public string[] Rewards { get; set; } = Array.Empty<string>();

    /// <summary>
    /// AI-generated insights and recommendations
    /// </summary>
    public string? AIInsights { get; set; }

    /// <summary>
    /// Difficulty level (1-5) for AI progression
    /// </summary>
    [Range(1, 5)]
    public int DifficultyLevel { get; set; } = 1;

    /// <summary>
    /// Environment factors that affect success
    /// </summary>
    public string? EnvironmentFactors { get; set; }

    /// <summary>
    /// Last completion timestamp
    /// </summary>
    public DateTime? LastCompletedAt { get; set; }

    // Foreign key
    /// <summary>
    /// Habit owner user ID
    /// </summary>
    public Guid UserId { get; set; }

    // Navigation properties
    /// <summary>
    /// Habit owner
    /// </summary>
    public virtual User User { get; set; } = null!;

    /// <summary>
    /// Habit entries for tracking and analytics
    /// </summary>
    public virtual ICollection<HabitEntry> Entries { get; set; } = new List<HabitEntry>();

    /// <summary>
    /// Habit analytics data for AI insights
    /// </summary>
    public virtual ICollection<HabitAnalytics> Analytics { get; set; } = new List<HabitAnalytics>();

    public void TrackProgress(int value)
    {
        // Example logic: update streak and completion rate
        if (value >= TargetCount)
            CurrentStreak += 1;
        else
            CurrentStreak = 0;

        CompletionRate = Math.Min(100, (double)value / TargetCount * 100);
        UpdatedAt = DateTime.UtcNow;
    }
}




