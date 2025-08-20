namespace Application.IService;

public interface ITokenService
{
    string GenerateToken(Guid userId, string email, string username);
}

