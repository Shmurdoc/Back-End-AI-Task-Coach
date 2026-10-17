using Domain.Entities;

namespace Application.IRepositories;

/// <summary>
/// Persistence boundary for server-side refresh token records.
/// </summary>
public interface IRefreshTokenRepository
{
    /// <summary>
    /// Finds the stored record for a token hash, or null when the token was
    /// never issued by this server (or has already been cleaned up).
    /// </summary>
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash);

    /// <summary>
    /// All still-usable tokens of a user. Used by reuse detection to revoke a
    /// compromised token family in one shot.
    /// </summary>
    Task<IEnumerable<RefreshToken>> GetActiveByUserIdAsync(Guid userId);

    /// <summary>
    /// Stages a new token record. Callers commit through <see cref="IUnitOfWork"/>
    /// so a rotation (revoke old + insert new) lands in a single transaction.
    /// </summary>
    Task<RefreshToken> AddAsync(RefreshToken refreshToken);

    /// <summary>
    /// Stages removal of token records (cleanup job).
    /// </summary>
    void RemoveRange(IEnumerable<RefreshToken> refreshTokens);

    /// <summary>
    /// A batch of tokens that are expired, or were revoked before the given
    /// cutoff (small retention so reuse detection still works right after a
    /// rotation).
    /// </summary>
    Task<List<RefreshToken>> GetStaleAsync(DateTime utcNow, DateTime revokedBefore, int take);
}
