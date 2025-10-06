using Domain.Common;
namespace Domain.Entities;
public class UserPreferences : BaseEntity
{
    public Guid UserId { get; set; }
    public bool UseEmail { get; set; } = true;
    public bool UseSms { get; set; } = false;
    public int QuietFromHour { get; set; } = 22; // 22:00
    public int QuietToHour { get; set; } = 7;   // 07:00
}
