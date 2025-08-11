using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class UserPattern : BaseEntity
{
    public PatternType Type { get; set; }
    public string PatternData { get; set; } = string.Empty; // JSON data about the pattern
    public double Confidence { get; set; } // 0.0 to 1.0
    public DateTime LastAnalyzed { get; set; }
    public int SampleSize { get; set; } // Number of data points used
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    // Foreign key
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
}
