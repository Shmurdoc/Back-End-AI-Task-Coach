using Application.IRepositories;
using Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace AITaskCoach.Tests.Auth;

// The refresh endpoint must only exchange refresh-scoped tokens.
// Before Cycle 2 an access token (even an expired one) was accepted here,
// making access tokens effectively immortal.
public class TokenServiceTests
{
    private static readonly Guid UserId = Guid.NewGuid();

    private static TokenService NewService(IUserRepository? repository = null, int refreshDays = 7)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "unit-test-signing-key-at-least-32-characters!",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience",
                ["Jwt:ExpiresInMinutes"] = "60",
                ["Jwt:RefreshExpiresInDays"] = refreshDays.ToString()
            })
            .Build();

        repository ??= ActiveUserRepository();
        return new TokenService(config, new Mock<ILogger<TokenService>>().Object, repository);
    }

    private static IUserRepository ActiveUserRepository()
    {
        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new User { Id = UserId, Email = "user@test.local", Name = "Tester", IsActive = true });
        return repo.Object;
    }

    private static IUserRepository InactiveUserRepository()
    {
        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new User { Id = UserId, Email = "user@test.local", Name = "Tester", IsActive = false });
        return repo.Object;
    }

    [Fact]
    public async Task Refresh_WithValidRefreshToken_ReturnsRotatedPair()
    {
        var service = NewService();
        var refresh = service.GenerateRefreshToken(UserId, "user@test.local", "Tester");

        var (access, rotatedRefresh) = await service.RefreshTokenAsync(refresh);

        access.Should().NotBeNullOrWhiteSpace();
        rotatedRefresh.Should().NotBeNullOrWhiteSpace();
        rotatedRefresh.Should().NotBe(refresh);
    }

    [Fact]
    public async Task Refresh_WithAccessToken_IsRejected()
    {
        var service = NewService();
        var access = service.GenerateToken(UserId, "user@test.local", "Tester");

        var act = () => service.RefreshTokenAsync(access);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Refresh_WithExpiredRefreshToken_IsRejected()
    {
        var service = NewService(refreshDays: -1);
        var expiredRefresh = service.GenerateRefreshToken(UserId, "user@test.local", "Tester");

        var act = () => service.RefreshTokenAsync(expiredRefresh);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Refresh_WithGarbageToken_IsRejected()
    {
        var service = NewService();

        var act = () => service.RefreshTokenAsync("not-a-jwt-at-all");

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Refresh_WithInactiveUser_IsRejected()
    {
        var service = NewService(InactiveUserRepository());
        var refresh = service.GenerateRefreshToken(UserId, "user@test.local", "Tester");

        var act = () => service.RefreshTokenAsync(refresh);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Refresh_WithEmptyToken_IsRejected()
    {
        var service = NewService();

        var act = () => service.RefreshTokenAsync(string.Empty);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
