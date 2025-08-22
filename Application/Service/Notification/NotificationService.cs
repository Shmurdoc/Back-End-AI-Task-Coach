using Application.IRepositories;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using Application.IService;

namespace Application.Services;

public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;
    private readonly NotificationFactory _factory;
    private readonly IUserRepository _userRepo; // lightweight access to user prefs

    public NotificationService(ILogger<NotificationService> logger, NotificationFactory factory, IUserRepository userRepo)
    {
        _logger = logger;
        _factory = factory;
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
            if (InQuietHours(prefs)) { _logger.LogInformation("Quiet hours: suppressed for {User}", user.Email); return false; }

            var delivered = false;
            if (prefs?.UseEmail ?? true)
            {
                var smtp = _factory.Get("smtp"); if (smtp != null) delivered = await smtp.SendAsync(user, subject, message, ct) || delivered;
            }
            if (prefs?.UseSms ?? false)
            {
                var sms = _factory.Get("sms"); if (sms != null) delivered = await sms.SendAsync(user, subject, message, ct) || delivered;
            }
            return delivered;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Notification failed for {User}", user.Email);
            return false;
        }
    }
}
