using Application.Extensions;
using Application.IService;
using Domain.Entities;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using Polly;
using Polly.Extensions.Http;

namespace Application.Services;

public class MailKitEmailProvider : INotificationProvider
{
    private readonly ILogger<MailKitEmailProvider> _logger;
    private readonly IConfiguration _config;
    private readonly IAsyncPolicy _retryPolicy;
    
    public string Name => "mailkit";
    
    public MailKitEmailProvider(ILogger<MailKitEmailProvider> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
        _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    _logger.LogWarning("Email send attempt {RetryCount} failed. Retrying in {Delay}ms", 
                        retryCount, timespan.TotalMilliseconds);
                });
    }

    public async Task<bool> SendAsync(User user, string subject, string message, CancellationToken ct = default)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress(
                    _config["Email:FromName"] ?? "AI Task Coach",
                    _config["Email:FromAddress"] ?? "noreply@aitaskcoach.com"));
                email.To.Add(new MailboxAddress(user.Name ?? user.Email, user.Email));
                email.Subject = subject;
                email.Body = new TextPart("html") { Text = message };

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(
                    _config["Email:Host"] ?? "smtp.gmail.com",
                    int.Parse(_config["Email:Port"] ?? "587"),
                    SecureSocketOptions.StartTls,
                    ct);

                await smtp.AuthenticateAsync(
                    _config["Email:Username"] ?? throw new InvalidOperationException("Email:Username not configured"),
                    _config["Email:Password"] ?? throw new InvalidOperationException("Email:Password not configured"),
                    ct);

                await smtp.SendAsync(email, ct);
                await smtp.DisconnectAsync(true, ct);

                ObservabilityExtensions.NudgesDelivered.Add(1, new KeyValuePair<string, object?>("provider", Name), new KeyValuePair<string, object?>("type", "email"));
                _logger.LogInformation("Email sent successfully to {Email}", user.Email);
                return true;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email} after retries", user.Email);
            return false;
        }
    }
}
