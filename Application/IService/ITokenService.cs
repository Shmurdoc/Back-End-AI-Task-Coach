namespace Application.IService;

public interface ITokenService
{
    string GenerateToken(Guid userId, string email, string username);

    // Controller-compatible stubs
    Task<string> GenerateTokenAsync(string email, string password, CancellationToken cancellationToken = default);
    Task<string> GenerateTokenAsync(Guid userId, string email, string username, CancellationToken cancellationToken = default);
    Task<string> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
}

