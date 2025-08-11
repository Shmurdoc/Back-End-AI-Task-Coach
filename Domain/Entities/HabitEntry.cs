using Domain.Common;
using Domain.Enums.extension.helper;

namespace Domain.Entities;

public class HabitEntry : BaseEntity
{
    public DateTime Date { get; set; }
    public int Value { get; set; } // How many times completed (for countable habits)
    public CompletionStatus Status { get; set; }
    public string Notes { get; set; } = string.Empty;
    public TimeSpan? TimeSpent { get; set; }

    // Foreign key
    public Guid HabitId { get; set; }
    public Habit Habit { get; set; } = null!;
}
