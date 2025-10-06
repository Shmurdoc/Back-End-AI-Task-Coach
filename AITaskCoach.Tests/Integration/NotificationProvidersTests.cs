using Application.Services;
using Application.IService;
using Infrastructure.Services.Notification;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace AITaskCoach.Tests.Integration;

public class NotificationProvidersTests
{
    private readonly Mock<ILogger<MailKitEmailProvider>> _emailLogger;
    private readonly Mock<ILogger<TwilioSmsProvider>> _smsLogger;
    private readonly Mock<IConfiguration> _config;

    public NotificationProvidersTests()
    {
        _emailLogger = new Mock<ILogger<MailKitEmailProvider>>();
        _smsLogger = new Mock<ILogger<TwilioSmsProvider>>();
        _config = new Mock<IConfiguration>();
        
        // Setup mock configuration
        _config.Setup(c => c["Email:Host"]).Returns("smtp.test.com");
        _config.Setup(c => c["Email:Port"]).Returns("587");
        _config.Setup(c => c["Email:Username"]).Returns("test@test.com");
        _config.Setup(c => c["Email:Password"]).Returns("testpassword");
        _config.Setup(c => c["Email:FromAddress"]).Returns("noreply@test.com");
        _config.Setup(c => c["Email:FromName"]).Returns("Test");
        _config.Setup(c => c["Twilio:AccountSid"]).Returns("test_sid");
        _config.Setup(c => c["Twilio:AuthToken"]).Returns("test_token");
        _config.Setup(c => c["Twilio:FromNumber"]).Returns("+1234567890");
    }

    [Fact]
    public void MailKitEmailProvider_ShouldHaveCorrectName()
    {
        // Arrange
        var provider = new MailKitEmailProvider(_emailLogger.Object, _config.Object);

        // Act & Assert
        provider.Name.Should().Be("mailkit");
    }

    [Fact]
    public void TwilioSmsProvider_ShouldHaveCorrectName()
    {
        // Arrange
        var provider = new TwilioSmsProvider(_smsLogger.Object, _config.Object);

        // Act & Assert
        provider.Name.Should().Be("twilio");
    }

    [Fact]
    public async Task TwilioSmsProvider_ShouldReturnFalse_WhenUserHasNoPhoneNumber()
    {
        // Arrange
        var provider = new TwilioSmsProvider(_smsLogger.Object, _config.Object);
        var user = new User { Id = Guid.NewGuid(), Email = "test@test.com" /* PhoneNumber = null */ };

        // Act
        var result = await provider.SendAsync(user, "Test", "Test message");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task MailKitEmailProvider_ShouldLogAttempt()
    {
        // Arrange
        var provider = new MailKitEmailProvider(_emailLogger.Object, _config.Object);
        var user = new User { Id = Guid.NewGuid(), Email = "test@test.com", Name = "Test User" };

        // Act
        try
        {
            await provider.SendAsync(user, "Test Subject", "Test message");
        }
        catch
        {
            // Expected to fail in test environment without real SMTP
        }

        // Assert - Verify logging occurred
        _emailLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Email send attempt")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }
}
