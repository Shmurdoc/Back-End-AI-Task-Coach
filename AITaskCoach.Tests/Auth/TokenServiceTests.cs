using Application.IRepositories;
using Application.Services;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace AITaskCoach.Tests.Auth;

// The refresh endpoint must only exchange refresh-scoped tokens. Older
// versions accepted an access token here, even an expired one, which made
// access tokens effectively permanent. Refresh tokens are now stored
// server-side with rotation and reuse detection.
public class TokenServiceTests
{
    private static readonly Guid UserId = Guid.NewGuid();

    private static TokenService NewService(
        IUserRepository? repository = null,
        IRefreshTokenRepository? refreshTokens = null,
        int refreshDays = 7)
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

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        return new TokenService(
            config,
            new Mock<ILogger<TokenService>>().Object,
            repository,
            refreshTokens ?? new FakeRefreshTokenRepository(),
            unitOfWork.Object);
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
        var refresh = await service.GenerateRefreshTokenAsync(UserId, "user@test.local", "Tester");

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
        var expiredRefresh = await service.GenerateRefreshTokenAsync(UserId, "user@test.local", "Tester");

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
        var refresh = await service.GenerateRefreshTokenAsync(UserId, "user@test.local", "Tester");

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

    [Fact]
    public async Task Refresh_WithWellFormedTokenMissingFromStore_IsRejected()
    {
        // A token that is correctly signed but was never issued by this server
        // (or an instance with an empty store) must not be exchangeable.
        var issuer = NewService();
        var storedElsewhere = await issuer.GenerateRefreshTokenAsync(UserId, "user@test.local", "Tester");

        var otherInstance = NewService();

        var act = () => otherInstance.RefreshTokenAsync(storedElsewhere);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Refresh_WithRevokedToken_IsRejected()
    {
        var service = NewService();
        var refresh = await service.GenerateRefreshTokenAsync(UserId, "user@test.local", "Tester");

        (await service.RevokeRefreshTokenAsync(refresh, UserId)).Should().BeTrue();

        var act = () => service.RefreshTokenAsync(refresh);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Refresh_WithReusedRotatedToken_RevokesTheWholeFamily()
    {
        var service = NewService();
        var original = await service.GenerateRefreshTokenAsync(UserId, "user@test.local", "Tester");
        var (_, rotated) = await service.RefreshTokenAsync(original);

        // Replaying the rotated token is treated as theft...
        var reuse = () => service.RefreshTokenAsync(original);
        await reuse.Should().ThrowAsync<UnauthorizedAccessException>();

        // ...and must also invalidate the token the thief might hold.
        var stolen = () => service.RefreshTokenAsync(rotated);
        await stolen.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Revoke_WithUnknownToken_ReturnsFalse()
    {
        var service = NewService();

        var revoked = await service.RevokeRefreshTokenAsync("not-a-stored-token", UserId);

        revoked.Should().BeFalse();
    }

    [Fact]
    public async Task Revoke_WithForeignUser_ReturnsFalseAndLeavesTokenUsable()
    {
        var service = NewService();
        var refresh = await service.GenerateRefreshTokenAsync(UserId, "user@test.local", "Tester");

        var revoked = await service.RevokeRefreshTokenAsync(refresh, Guid.NewGuid());

        revoked.Should().BeFalse();

        // The real owner can still exchange it.
        await service.RefreshTokenAsync(refresh);
    }

    [Fact]
    public async Task Revoke_IsIdempotent()
    {
        var service = NewService();
        var refresh = await service.GenerateRefreshTokenAsync(UserId, "user@test.local", "Tester");

        (await service.RevokeRefreshTokenAsync(refresh, UserId)).Should().BeTrue();
        (await service.RevokeRefreshTokenAsync(refresh, UserId)).Should().BeTrue();
    }

    /// <summary>
    /// In-memory store that behaves like the EF Core repository: reads return
    /// the tracked instance, so mutations (revocation) are visible afterwards.
    /// </summary>
    private sealed class FakeRefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly List<RefreshToken> _tokens = new();

        public Task<RefreshToken?> GetByTokenHashAsync(string tokenHash)
            => Task.FromResult(_tokens.FirstOrDefault(t => t.TokenHash == tokenHash));

        public Task<IEnumerable<RefreshToken>> GetActiveByUserIdAsync(Guid userId)
            => Task.FromResult<IEnumerable<RefreshToken>>(_tokens
                .Where(t => t.UserId == userId && t.RevokedAt == null && t.ExpiresAt > DateTime.UtcNow)
                .ToList());

        public Task<RefreshToken> AddAsync(RefreshToken refreshToken)
        {
            if (refreshToken.Id == Guid.Empty)
                refreshToken.Id = Guid.NewGuid();

            _tokens.Add(refreshToken);
            return Task.FromResult(refreshToken);
        }

        public void RemoveRange(IEnumerable<RefreshToken> refreshTokens)
        {
            foreach (var token in refreshTokens.ToList())
                _tokens.Remove(token);
        }

        public Task<List<RefreshToken>> GetStaleAsync(DateTime utcNow, DateTime revokedBefore, int take)
            => Task.FromResult(_tokens
                .Where(t => t.ExpiresAt < utcNow || (t.RevokedAt != null && t.RevokedAt < revokedBefore))
                .Take(take)
                .ToList());
    }
}
