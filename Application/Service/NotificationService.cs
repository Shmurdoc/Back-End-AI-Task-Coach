namespace Application.Services;

public interface INotificationService
{
    Task SendEmailAsync(string to, string subject, string body);
    Task SendReminderAsync(Guid userId, string message);
}

public class NotificationService : INotificationService
{
    public async Task SendEmailAsync(string to, string subject, string body)
    {
        // TODO: Integrate with SMTP/SendGrid
        await Task.CompletedTask;
    }

    public async Task SendReminderAsync(Guid userId, string message)
    {
        // TODO: Implement reminder logic (e.g., push notification, email)
        await Task.CompletedTask;
    }
}
