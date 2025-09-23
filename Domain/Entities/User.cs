using Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

/// <summary>
/// User entity for AI TaskFlow Coach system
/// </summary>
public class User : BaseEntity
{

    /// <summary>
    /// User email address
    /// </summary>
    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;


    /// <summary>
    /// User display name
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// User phone number (for SMS notifications)
    /// </summary>
    // [Phone]
    // [StringLength(32)]
    // public string? PhoneNumber { get; set; }

    /// <summary>
    /// Hashed password
    /// </summary>
    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Whether the user account is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    // Navigation properties
    /// <summary>
    /// User's habits
    /// </summary>
    public virtual ICollection<Habit> Habits { get; set; } = new List<Habit>();

    /// <summary>
    /// User's goals
    /// </summary>
    public virtual ICollection<Goal> Goals { get; set; } = new List<Goal>();

    /// <summary>
    /// User's tasks
    /// </summary>
    public virtual ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
