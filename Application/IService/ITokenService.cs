namespace Application.IService;

public interface ITokenService
{
    string GenerateToken(Guid userId, string email, string username);

    /// <summary>
    /// Creates a refresh token and stores its hash server-side (committed
    /// through the unit of work) so it can be rotated and revoked later.
    /// </summary>
    Task<string> GenerateRefreshTokenAsync(Guid userId, string email, string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Exchanges a refresh token for a new access + refresh token pair.
    /// The presented token is revoked (rotation); reusing an already
    /// rotated token revokes every active token of the user.
    /// </summary>
    Task<(string AccessToken, string RefreshToken)> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes a single refresh token if it belongs to the given user.
    /// Idempotent; returns false for unknown or foreign tokens.
    /// </summary>
    Task<bool> RevokeRefreshTokenAsync(string refreshToken, Guid userId, CancellationToken cancellationToken = default);
}
