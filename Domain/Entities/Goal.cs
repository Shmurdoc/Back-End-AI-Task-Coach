using Domain.Common;
using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

/// <summary>
/// Goal entity for AI TaskFlow Coach system
/// </summary>
public class Goal : BaseEntity
{
    /// <summary>
    /// Goal title
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Detailed goal description
    /// </summary>
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Goal category
    /// </summary>
    public GoalCategory Category { get; set; }

    /// <summary>
    /// Current status of the goal
    /// </summary>
    public GoalStatus Status { get; set; } = GoalStatus.NotStarted;

    /// <summary>
    /// Goal priority level
    /// </summary>
    [Range(1, 5)]
    public int Priority { get; set; } = 3;

    /// <summary>
    /// Target completion date
    /// </summary>
    public DateTime? TargetDate { get; set; }

    /// <summary>
    /// Completion timestamp
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Current progress value (0-100)
    /// </summary>
    [Range(0, 100)]
    public int Progress { get; set; } = 0;

    // Foreign key
    /// <summary>
    /// Goal owner user ID
    /// </summary>
    public Guid UserId { get; set; }

    // Navigation properties
    /// <summary>
    /// Goal owner
    /// </summary>
    public virtual User User { get; set; } = null!;

    /// <summary>
    /// Tasks associated with this goal
    /// </summary>
    public virtual ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();

    /// <summary>
    /// Progress history for AI trend analysis
    /// </summary>
    public virtual ICollection<GoalProgress> ProgressHistory { get; set; } = new List<GoalProgress>();

    public void MarkAsCompleted()
    {
        if (Tasks != null && Tasks.Any(t => t.Status != Domain.Enums.TaskStatus.Completed))
            throw new InvalidOperationException("Cannot complete goal until all tasks are done.");
        Status = GoalStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Goal progress tracking for AI analytics
/// </summary>
public class GoalProgress
{
    public Guid Id { get; set; }

    /// <summary>
    /// Progress value (0-100)
    /// </summary>
    [Range(0, 100)]
    public int ProgressValue { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    public DateTime RecordedAt { get; set; }

    // Foreign key
    public Guid GoalId { get; set; }
    public virtual Goal Goal { get; set; } = null!;
}