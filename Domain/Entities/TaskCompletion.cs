using Domain.Common;
using Domain.Enums.extension.helper;

namespace Domain.Entities;

public class TaskCompletion : BaseEntity
{
    public DateTime CompletedAt { get; set; }
    public TimeSpan ActualDuration { get; set; }
    public CompletionStatus Status { get; set; }
    public string Notes { get; set; } = string.Empty;
    public int QualityRating { get; set; } // 1-5 scale
    public bool WasOnTime { get; set; }
    public bool WasRescheduled { get; set; }

    // Foreign key
    public Guid TaskId { get; set; }
    public TaskItem Task { get; set; } = null!;
}
