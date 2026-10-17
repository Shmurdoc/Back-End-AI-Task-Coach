using Application.IRepositories;
using Domain.Entities;
using Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of refresh token storage. Lookups are tracking
/// queries on purpose: the token service revokes records it just read.
/// </summary>
public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<RefreshTokenRepository> _logger;

    public RefreshTokenRepository(ApplicationDbContext context, ILogger<RefreshTokenRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(tokenHash))
                return null;

            return await _context.Set<RefreshToken>()
                .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving refresh token by hash");
            throw;
        }
    }

    public async Task<IEnumerable<RefreshToken>> GetActiveByUserIdAsync(Guid userId)
    {
        try
        {
            var now = DateTime.UtcNow;
            return await _context.Set<RefreshToken>()
                .Where(t => t.UserId == userId && t.RevokedAt == null && t.ExpiresAt > now)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active refresh tokens for user: {UserId}", userId);
            throw;
        }
    }

    public Task<RefreshToken> AddAsync(RefreshToken refreshToken)
    {
        try
        {
            if (refreshToken == null)
                throw new ArgumentNullException(nameof(refreshToken));

            // Stage only: the caller commits through IUnitOfWork.
            _context.Set<RefreshToken>().Add(refreshToken);
            return Task.FromResult(refreshToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error staging refresh token for user: {UserId}", refreshToken?.UserId);
            throw;
        }
    }

    public void RemoveRange(IEnumerable<RefreshToken> refreshTokens)
    {
        try
        {
            _context.Set<RefreshToken>().RemoveRange(refreshTokens);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error staging refresh tokens for removal");
            throw;
        }
    }

    public async Task<List<RefreshToken>> GetStaleAsync(DateTime utcNow, DateTime revokedBefore, int take)
    {
        try
        {
            return await _context.Set<RefreshToken>()
                .Where(t => t.ExpiresAt < utcNow || (t.RevokedAt != null && t.RevokedAt < revokedBefore))
                .OrderBy(t => t.ExpiresAt)
                .Take(take)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving stale refresh tokens");
            throw;
        }
    }
}
