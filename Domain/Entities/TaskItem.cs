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
}


