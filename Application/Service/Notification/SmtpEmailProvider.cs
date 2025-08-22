using Application.IService;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class SmtpEmailProvider : INotificationProvider
{
    private readonly ILogger<SmtpEmailProvider> _logger;
    private readonly IConfiguration _config;
    public string Name => "smtp";
    public SmtpEmailProvider(ILogger<SmtpEmailProvider> logger, IConfiguration config) { _logger = logger; _config = config; }

    public Task<bool> SendAsync(User user, string subject, string message, CancellationToken ct = default)
    {
        // Placeholder: integrate System.Net.Mail SmtpClient or MailKit here.
        var server = _config["Smtp:Host"] ?? "(not-configured)";
        _logger.LogInformation("[SMTP] To={Email} Server={Server} Subject={Sub}", user.Email, server, subject);
        return Task.FromResult(!string.IsNullOrEmpty(user.Email));
    }
}
