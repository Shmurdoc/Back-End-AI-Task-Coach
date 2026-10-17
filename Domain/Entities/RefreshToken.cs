using Domain.Common;

namespace Domain.Entities;

/// <summary>
/// Server-side record of an issued refresh token.
/// Refresh tokens are JWTs; only their SHA-256 hash is stored so a database
/// leak does not hand out usable tokens. Rotation (every refresh revokes the
/// presented token) and reuse detection build on <see cref="RevokedAt"/>.
/// </summary>
public class RefreshToken : BaseEntity
{
    /// <summary>
    /// Owner of the token.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// SHA-256 hash (lowercase hex) of the refresh token string.
    /// </summary>
    public string TokenHash { get; set; } = string.Empty;

    /// <summary>
    /// Moment after which the token can no longer be exchanged.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Set when the token is rotated, explicitly revoked, or the whole token
    /// family is revoked after reuse detection.
    /// </summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>
    /// A token is usable only while it is neither revoked nor expired.
    /// </summary>
    public bool IsActive(DateTime utcNow) => RevokedAt == null && ExpiresAt > utcNow;
}
