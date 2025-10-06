using Application.IService;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Application.Extensions;

namespace Infrastructure.Services.Notification;

public class TwilioSmsProvider : INotificationProvider
{
    private readonly ILogger<TwilioSmsProvider> _logger;
    private readonly IConfiguration _config;
    private readonly IAsyncPolicy _retryPolicy;
    
    public string Name => "twilio";
    
    public TwilioSmsProvider(ILogger<TwilioSmsProvider> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
        
        var accountSid = _config["Twilio:AccountSid"];
        var authToken = _config["Twilio:AuthToken"];
        
        if (!string.IsNullOrEmpty(accountSid) && !string.IsNullOrEmpty(authToken))
        {
            TwilioClient.Init(accountSid, authToken);
        }
        
        _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    _logger.LogWarning("SMS send attempt {RetryCount} failed. Retrying in {Delay}ms", 
                        retryCount, timespan.TotalMilliseconds);
                });
    }

    public Task<bool> SendAsync(User user, string subject, string message, CancellationToken ct = default)
    {
        // TODO: Add PhoneNumber support when database is updated
        _logger.LogWarning("User {UserId} - SMS temporarily disabled (PhoneNumber field not available)", user.Id);
        return Task.FromResult(false);
        
        /*
        if (string.IsNullOrEmpty(user.PhoneNumber))
        {
            _logger.LogWarning("User {UserId} has no phone number configured", user.Id);
            return false;
        }

        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var messageResource = await MessageResource.CreateAsync(
                    body: $"{subject}\n\n{message}",
                    from: new Twilio.Types.PhoneNumber(_config["Twilio:FromNumber"] ?? throw new InvalidOperationException("Twilio:FromNumber not configured")),
                    to: new Twilio.Types.PhoneNumber(user.PhoneNumber)
                );

                ObservabilityExtensions.NudgesDelivered.Add(1, new("provider", Name), new("type", "sms"));
                _logger.LogInformation("SMS sent successfully to {PhoneNumber}, SID: {MessageSid}", user.PhoneNumber, messageResource.Sid);
                return true;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS to {PhoneNumber} after retries", user.PhoneNumber);
            return false;
        }
        */
    }
}
