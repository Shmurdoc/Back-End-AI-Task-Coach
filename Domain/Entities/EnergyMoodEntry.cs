using Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

/// <summary>
/// One user energy/mood check-in. Persisted rows replace the old
/// process-local demo dictionary: entries survive restarts and are
/// queryable across requests.
/// </summary>
public class EnergyMoodEntry : BaseEntity
{
    [Range(1, 10)]
    public int EnergyLevel { get; set; }

    [Range(1, 10)]
    public int MoodLevel { get; set; }

    [StringLength(500)]
    public string? Note { get; set; }

    /// <summary>
    /// When the check-in happened (client-supplied clock), distinct from
    /// CreatedAt which records when the row was stored.
    /// </summary>
    public DateTime Timestamp { get; set; }

    public Guid UserId { get; set; }

    public virtual User User { get; set; } = null!;
}
