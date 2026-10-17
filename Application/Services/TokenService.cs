using Application.IService;
using Application.IRepositories;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Application.Services;

/// <summary>
/// JWT token service for authentication and authorization.
/// Refresh tokens are stored server-side as SHA-256 hashes so they can be
/// rotated on every use and revoked on demand. Reusing an already rotated
/// token is treated as theft and revokes every active token of that user.
/// </summary>
public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<TokenService> _logger;
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TokenService(
        IConfiguration configuration,
        ILogger<TokenService> logger,
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork)
    {
        _configuration = configuration;
        _logger = logger;
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
    }

    public string GenerateToken(Guid userId, string email, string username)
    {
        try
        {
            var jwtSettings = GetJwtSettings();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Name, username),
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings.Issuer,
                audience: jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(jwtSettings.ExpiresInMinutes),
                signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            _logger.LogInformation("JWT token generated successfully for user: {UserId}", userId);
            return tokenString;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating JWT token for user: {UserId}", userId);
            throw new InvalidOperationException("Failed to generate authentication token", ex);
        }
    }

    public async Task<string> GenerateRefreshTokenAsync(Guid userId, string email, string username, CancellationToken cancellationToken = default)
    {
        try
        {
            var (tokenString, expiresAt) = BuildRefreshTokenJwt(userId, email, username);

            await _refreshTokenRepository.AddAsync(new RefreshToken
            {
                UserId = userId,
                TokenHash = HashRefreshToken(tokenString),
                ExpiresAt = expiresAt
            });
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Refresh token generated for user: {UserId}", userId);
            return tokenString;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating refresh token for user: {UserId}", userId);
            throw new InvalidOperationException("Failed to generate refresh token", ex);
        }
    }

    public async Task<(string AccessToken, string RefreshToken)> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Attempting to refresh token");

            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                throw new UnauthorizedAccessException("Invalid refresh token");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtSettings = GetJwtSettings();
            var key = Encoding.UTF8.GetBytes(jwtSettings.Key);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,
                ValidateLifetime = true, // an expired refresh token must be rejected
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(refreshToken, validationParameters, out SecurityToken validatedToken);

            if (validatedToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            // Only refresh-scoped tokens may be exchanged; an access token is not
            // a refresh token even though it carries the same identity claims.
            if (!string.Equals(principal.FindFirst("token_use")?.Value, "refresh", StringComparison.Ordinal))
            {
                throw new SecurityTokenException("Not a refresh token");
            }

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
            {
                throw new SecurityTokenException("Invalid user ID in token");
            }

            // The token must exist in the store: a validly signed JWT that this
            // server never issued (or already cleaned up) is not acceptable.
            var storedToken = await _refreshTokenRepository.GetByTokenHashAsync(HashRefreshToken(refreshToken));
            if (storedToken == null)
            {
                _logger.LogWarning("Refresh token rejected: not found in store");
                throw new UnauthorizedAccessException("Invalid refresh token");
            }

            var now = DateTime.UtcNow;

            if (storedToken.RevokedAt != null)
            {
                // Reuse of a rotated or revoked token: the only party that
                // replays an old token is a thief, so revoke the whole family.
                var activeTokens = await _refreshTokenRepository.GetActiveByUserIdAsync(storedToken.UserId);
                foreach (var activeToken in activeTokens)
                {
                    activeToken.RevokedAt = now;
                }
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogWarning("Refresh token reuse detected for user {UserId}; all active refresh tokens revoked", storedToken.UserId);
                throw new UnauthorizedAccessException("Invalid refresh token");
            }

            if (storedToken.ExpiresAt <= now)
            {
                _logger.LogWarning("Refresh token rejected: expired for user {UserId}", storedToken.UserId);
                throw new UnauthorizedAccessException("Invalid refresh token");
            }

            // Verify user still exists and is active
            var user = await _userRepository.GetByIdAsync(storedToken.UserId);
            if (user == null || !user.IsActive)
            {
                throw new UnauthorizedAccessException("User not found or inactive");
            }

            // Rotate: revoke the presented token and issue a fresh pair, so a
            // stolen copy stops working the moment the real client refreshes.
            storedToken.RevokedAt = now;
            var (newRefreshToken, newRefreshExpiresAt) = BuildRefreshTokenJwt(user.Id, user.Email, user.Name);
            await _refreshTokenRepository.AddAsync(new RefreshToken
            {
                UserId = user.Id,
                TokenHash = HashRefreshToken(newRefreshToken),
                ExpiresAt = newRefreshExpiresAt
            });
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Token refreshed successfully for user: {UserId}", user.Id);
            return (GenerateToken(user.Id, user.Email, user.Name), newRefreshToken);
        }
        catch (Exception ex) when (ex is SecurityTokenException or ArgumentException)
        {
            _logger.LogWarning(ex, "Refresh token rejected");
            throw new UnauthorizedAccessException("Invalid refresh token", ex);
        }
    }

    public async Task<bool> RevokeRefreshTokenAsync(string refreshToken, Guid userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return false;

        var storedToken = await _refreshTokenRepository.GetByTokenHashAsync(HashRefreshToken(refreshToken));
        if (storedToken == null || storedToken.UserId != userId)
        {
            // Unknown token or a token that belongs to somebody else; report
            // failure without revealing which of the two it was.
            return false;
        }

        if (storedToken.RevokedAt != null)
            return true; // already revoked; revocation is idempotent

        storedToken.RevokedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Refresh token revoked for user: {UserId}", userId);
        return true;
    }

    private (string Token, DateTime ExpiresAtUtc) BuildRefreshTokenJwt(Guid userId, string email, string username)
    {
        var jwtSettings = GetJwtSettings();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Name, username),
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            // Marks this token as refresh-only so it is rejected by the
            // auth middleware and by RefreshTokenAsync when used as an access token.
            new Claim("token_use", "refresh")
        };

        var expiresAt = DateTime.UtcNow.AddDays(jwtSettings.RefreshExpiresInDays);
        var token = new JwtSecurityToken(
            issuer: jwtSettings.Issuer,
            audience: jwtSettings.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    private static string HashRefreshToken(string refreshToken)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken))).ToLowerInvariant();

    public static string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be null or empty", nameof(password));

        using var rng = RandomNumberGenerator.Create();
        var salt = new byte[16];
        rng.GetBytes(salt);

        var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256);
        var hash = pbkdf2.GetBytes(32);

        var hashBytes = new byte[48];
        Array.Copy(salt, 0, hashBytes, 0, 16);
        Array.Copy(hash, 0, hashBytes, 16, 32);

        return Convert.ToBase64String(hashBytes);
    }

    public static bool VerifyPassword(string password, string hashedPassword)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hashedPassword))
            return false;

        try
        {
            var hashBytes = Convert.FromBase64String(hashedPassword);
            if (hashBytes.Length != 48)
                return false;

            var salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);

            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256);
            var hash = pbkdf2.GetBytes(32);

            for (int i = 0; i < 32; i++)
            {
                if (hashBytes[i + 16] != hash[i])
                    return false;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    private JwtSettings GetJwtSettings()
    {
        var jwtSection = _configuration.GetSection("Jwt");

        return new JwtSettings
        {
            Key = jwtSection["Key"] ?? throw new InvalidOperationException("JWT Key not configured"),
            Issuer = jwtSection["Issuer"] ?? throw new InvalidOperationException("JWT Issuer not configured"),
            Audience = jwtSection["Audience"] ?? throw new InvalidOperationException("JWT Audience not configured"),
            ExpiresInMinutes = int.Parse(jwtSection["ExpiresInMinutes"] ?? "120"),
            RefreshExpiresInDays = int.Parse(jwtSection["RefreshExpiresInDays"] ?? "7")
        };
    }

    private class JwtSettings
    {
        public string Key { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int ExpiresInMinutes { get; set; }
        public int RefreshExpiresInDays { get; set; }
    }
}
