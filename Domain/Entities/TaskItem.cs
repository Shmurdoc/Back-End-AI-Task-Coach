using Domain.Common;
using Domain.Enums;
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
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;
    [Required]
    public TaskItemStatus Status { get; set; } = TaskItemStatus.Todo;
    [Required]
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    [Range(0.1, 1000)]
    public double EstimatedHours { get; set; }
    [Range(0, 1000)]
    public double ActualHours { get; set; }
    [Range(0, 100)]
    public int CompletionPercentage { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public string[] Tags { get; set; } = Array.Empty<string>();
    public string? AISuggestions { get; set; } // AI-generated recommendations
    public Guid[] Dependencies { get; set; } = Array.Empty<Guid>();
    [Range(1, 5)]
    public int EnergyLevel { get; set; } = 3;
    [Range(5, 480)]
    public int FocusTimeMinutes { get; set; } = 30;
    public Guid UserId { get; set; }
    public Guid? GoalId { get; set; }
    public virtual User User { get; set; } = null!;
    public virtual Goal? Goal { get; set; }
    public virtual ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
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

    public TaskItemStatus FromStatus { get; set; }

    public TaskItemStatus ToStatus { get; set; }

    [StringLength(500)]
    public string? Reason { get; set; }

    public DateTime ChangedAt { get; set; }

    // Foreign key
    public Guid TaskId { get; set; }
    public virtual TaskItem Task { get; set; } = null!;
}


