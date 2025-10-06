using Application.IRepositories;
using Application.IService;
using Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Application.Services.Notification;

public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;
    private readonly IUserRepository _userRepo;

    public NotificationService(ILogger<NotificationService> logger, IUserRepository userRepo)
    {
        _logger = logger;
        _userRepo = userRepo;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        // TODO: Integrate with actual email provider
        _logger.LogInformation($"Sending email to {to}: {subject}");
        await Task.CompletedTask;
    }

    public async Task SendReminderAsync(Guid userId, string message)
    {
        // TODO: Integrate with actual reminder logic (push, SMS, etc.)
        _logger.LogInformation($"Sending reminder to user {userId}: {message}");
        await Task.CompletedTask;
    }

    private static bool InQuietHours(UserPreferences? p)
    {
        var now = DateTime.UtcNow.Hour;
        if (p == null) return now >= 22 || now < 7;
        if (p.QuietFromHour <= p.QuietToHour) return now >= p.QuietFromHour && now < p.QuietToHour;
        return now >= p.QuietFromHour || now < p.QuietToHour;
    }

    public async Task<bool> SendAsync(User user, string subject, string message, CancellationToken ct = default)
    {
        try
        {
            var prefs = await _userRepo.GetPreferencesAsync(user.Id);
            if (InQuietHours(prefs)) 
            { 
                _logger.LogInformation("Quiet hours: suppressed notification for {User}", user.Email); 
                return false; 
            }

            // Basic notification logging - actual implementation will be in Infrastructure layer
            _logger.LogInformation("Sending notification to {User}: {Subject}", user.Email, subject);
            
            // TODO: Delegate to concrete notification providers in Infrastructure layer
            // This should be handled by dependency injection of specific providers
            await Task.CompletedTask;
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Notification failed for {User}", user.Email);
            return false;
        }
    }
}
