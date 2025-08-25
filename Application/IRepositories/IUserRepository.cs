using Domain.Entities;

namespace Application.IRepositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task<bool> ExistsAsync(string email);
    Task<IEnumerable<User>> GetActiveUsersAsync(); Task<IEnumerable<User>> GetAllAsync();
    Task<User> AddAsync(User user);
    Task<User> UpdateAsync(User user);
    Task<bool> DeleteAsync(Guid id);

    Task SaveChangesAsync();

    Task<UserPreferences?> GetPreferencesAsync(Guid userId);
    Task UpdatePreferencesAsync(Guid userId, UserPreferences prefs);
}
