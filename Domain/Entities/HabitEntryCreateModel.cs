using Domain.Enums.extension.helper;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    /// <summary>
    /// ViewModel for creating a new habit entry (used in frontend/API communication)
    /// </summary>
    public class HabitEntryCreateModel
    {
        /// <summary>
        /// ID of the habit being updated
        /// </summary>
        [Required]
        public Guid HabitId { get; set; }

        /// <summary>
        /// Date the entry was completed
        /// </summary>
        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        /// <summary>
        /// Completion status (Completed, Skipped, etc.)
        /// </summary>
        [Required]
        public CompletionStatus Status { get; set; }

        /// <summary>
        /// Optional notes about the entry (reflection, context, etc.)
        /// </summary>
        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
