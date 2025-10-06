using Application.IService;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Notification;

public class SmsProvider : INotificationProvider
{
    private readonly ILogger<SmsProvider> _logger;
    private readonly IConfiguration _config;
    public string Name => "sms";
    public SmsProvider(ILogger<SmsProvider> logger, IConfiguration config) { _logger = logger; _config = config; }

    public Task<bool> SendAsync(User user, string subject, string message, CancellationToken ct = default)
    {
        // Placeholder: integrate Twilio SDK here using config keys
        var accountSid = _config["Twilio:AccountSid"] ?? "(not-configured)";
        _logger.LogInformation("[SMS] To={Phone} Account={Acc} Msg={Msg}", "N/A" /* user.PhoneNumber ?? "N/A" */, accountSid, message);
        return Task.FromResult(true /* !string.IsNullOrEmpty(user.PhoneNumber) */);
    }
}
