using Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

/// <summary>
/// Task entity for AI TaskFlow Coach system
/// </summary>
public class TaskItem : BaseEntity
{
    /// <summary>
    /// Task title
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Detailed task description
    /// </summary>
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Current task status
    /// </summary>
    [Required]
    public TaskStatus Status { get; set; } = TaskStatus.Todo;

    /// <summary>
    /// Task priority for AI scheduling
    /// </summary>
    [Required]
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    /// <summary>
    /// Due date for deadline tracking
    /// </summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// Estimated time to complete (in hours)
    /// </summary>
    [Range(0.1, 1000)]
    public double EstimatedHours { get; set; }

    /// <summary>
    /// Actual time spent (in hours) for AI accuracy improvement
    /// </summary>
    [Range(0, 1000)]
    public double ActualHours { get; set; }

    /// <summary>
    /// Task completion percentage (0-100)
    /// </summary>
    [Range(0, 100)]
    public int CompletionPercentage { get; set; }

    /// <summary>
    /// Task completion date
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Task start date for time tracking
    /// </summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>
    /// Tags for AI classification
    /// </summary>
    public string[] Tags { get; set; } = Array.Empty<string>();

    /// <summary>
    /// AI-generated suggestions and insights
    /// </summary>
    public string? AISuggestions { get; set; } // AI-generated recommendations

    /// <summary>
    /// Task dependencies for smart scheduling
    /// </summary>
    public Guid[] Dependencies { get; set; } = Array.Empty<Guid>();

    /// <summary>
    /// Energy level required (1-5) for optimal scheduling
    /// </summary>
    [Range(1, 5)]
    public int EnergyLevel { get; set; } = 3;

    /// <summary>
    /// Focus time required (in minutes) for AI scheduling
    /// </summary>
    [Range(5, 480)]
    public int FocusTimeMinutes { get; set; } = 30;

    // Foreign keys
    /// <summary>
    /// Task owner user ID
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Associated goal ID (optional)
    /// </summary>
    public Guid? GoalId { get; set; }

    // Navigation properties
    /// <summary>
    /// Task owner
    /// </summary>
    public virtual User User { get; set; } = null!;

    /// <summary>
    /// Associated goal (if any)
    /// </summary>
    public virtual Goal? Goal { get; set; }

    /// <summary>
    /// Time tracking entries for detailed analytics
    /// </summary>
    public virtual ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();

    /// <summary>
    /// Task status history for AI pattern analysis
    /// </summary>
    public virtual ICollection<TaskStatusHistory> StatusHistory { get; set; } = new List<TaskStatusHistory>();
}

/// <summary>
/// Time tracking entries for detailed productivity analysis
/// </summary>
public class TimeEntry
{
    public Guid Id { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    [Range(0, 24)]
    public double HoursWorked { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    [Range(1, 5)]
    public int ProductivityRating { get; set; } = 3;

    // Foreign key
    public Guid TaskId { get; set; }
    public virtual TaskItem Task { get; set; } = null!;
}

/// <summary>
/// Task status change history for AI pattern recognition
/// </summary>
public class TaskStatusHistory
{
    public Guid Id { get; set; }

    public TaskStatus FromStatus { get; set; }

    public TaskStatus ToStatus { get; set; }

    [StringLength(500)]
    public string? Reason { get; set; }

    public DateTime ChangedAt { get; set; }

    // Foreign key
    public Guid TaskId { get; set; }
    public virtual TaskItem Task { get; set; } = null!;
}


