using Application.IService;
using Application.IRepositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Application.Services;

/// <summary>
/// JWT token service for authentication and authorization
/// </summary>
public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<TokenService> _logger;
    private readonly IUserRepository _userRepository;

    public TokenService(IConfiguration configuration, ILogger<TokenService> logger, IUserRepository userRepository)
    {
        _configuration = configuration;
        _logger = logger;
        _userRepository = userRepository;
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

    public async Task<string> GenerateTokenAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Attempting to generate token for email: {Email}", email);

            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
            {
                _logger.LogWarning("Token generation failed - user not found: {Email}", email);
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            if (!VerifyPassword(password, user.PasswordHash))
            {
                _logger.LogWarning("Token generation failed - invalid password for user: {Email}", email);
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Token generation failed - inactive user: {Email}", email);
                throw new UnauthorizedAccessException("User account is inactive");
            }

            return GenerateToken(user.Id, user.Email, user.Name);
        }
        catch (Exception ex) when (!(ex is UnauthorizedAccessException))
        {
            _logger.LogError(ex, "Error generating token for email: {Email}", email);
            throw new InvalidOperationException("Authentication service error", ex);
        }
    }

    public async Task<string> GenerateTokenAsync(Guid userId, string email, string username, CancellationToken cancellationToken = default)
    {
        try
        {
            // Verify user exists and is active
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || !user.IsActive)
            {
                _logger.LogWarning("Token generation failed - user not found or inactive: {UserId}", userId);
                throw new UnauthorizedAccessException("User not found or inactive");
            }

            return GenerateToken(userId, email, username);
        }
        catch (Exception ex) when (!(ex is UnauthorizedAccessException))
        {
            _logger.LogError(ex, "Error generating token for user: {UserId}", userId);
            throw new InvalidOperationException("Authentication service error", ex);
        }
    }

    public async Task<string> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Attempting to refresh token");

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
                ValidateLifetime = false, // Don't validate lifetime for refresh
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(refreshToken, validationParameters, out SecurityToken validatedToken);
            
            if (validatedToken is not JwtSecurityToken jwtToken || 
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var emailClaim = principal.FindFirst(ClaimTypes.Email)?.Value;
            var nameClaim = principal.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || string.IsNullOrEmpty(emailClaim) || string.IsNullOrEmpty(nameClaim))
            {
                throw new SecurityTokenException("Invalid token claims");
            }

            if (!Guid.TryParse(userIdClaim, out Guid userId))
            {
                throw new SecurityTokenException("Invalid user ID in token");
            }

            // Verify user still exists and is active
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || !user.IsActive)
            {
                throw new UnauthorizedAccessException("User not found or inactive");
            }

            return GenerateToken(userId, emailClaim, nameClaim);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            throw new UnauthorizedAccessException("Invalid refresh token", ex);
        }
    }

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
            ExpiresInMinutes = int.Parse(jwtSection["ExpiresInMinutes"] ?? "120")
        };
    }

    private class JwtSettings
    {
        public string Key { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int ExpiresInMinutes { get; set; }
    }
}
