using Application.IRepositories;
using Domain.Entities;
using Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// User repository implementation for managing user data and authentication
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(ApplicationDbContext context, ILogger<UserRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        try
        {
            return await _context.Users
                .Include(u => u.Habits)
                .Include(u => u.Goals)
                .Include(u => u.Tasks)
                .FirstOrDefaultAsync(u => u.Id == id && u.IsActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user by ID: {UserId}", id);
            throw;
        }
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() && u.IsActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user by email: {Email}", email);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(string email)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            return await _context.Users
                .AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if user exists: {Email}", email);
            throw;
        }
    }

    public async Task<IEnumerable<User>> GetActiveUsersAsync()
    {
        return await _context.Users
            .Where(u => u.IsActive)
            .OrderBy(u => u.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users.ToListAsync();
    }

    public async Task<User> AddAsync(User user)
    {
        try
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            // Validate required fields
            if (string.IsNullOrWhiteSpace(user.Email))
                throw new ArgumentException("Email is required", nameof(user));

            if (string.IsNullOrWhiteSpace(user.Name))
                throw new ArgumentException("Name is required", nameof(user));

            if (string.IsNullOrWhiteSpace(user.PasswordHash))
                throw new ArgumentException("Password hash is required", nameof(user));

            // Check if user already exists
            var existingUser = await GetByEmailAsync(user.Email);
            if (existingUser != null)
                throw new InvalidOperationException($"User with email {user.Email} already exists");

            user.Id = Guid.NewGuid();
            user.CreatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            user.IsActive = true;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User created successfully: {UserId} - {Email}", user.Id, user.Email);
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding user: {Email}", user?.Email);
            throw;
        }
    }

    public async Task<User> UpdateAsync(User user)
    {
        try
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var existingUser = await _context.Users.FindAsync(user.Id);
            if (existingUser == null)
                throw new InvalidOperationException($"User with ID {user.Id} not found");

            // Update properties
            existingUser.Name = user.Name;
            existingUser.Email = user.Email;
            // existingUser.PhoneNumber = user.PhoneNumber;
            existingUser.IsActive = user.IsActive;
            existingUser.UpdatedAt = DateTime.UtcNow;

            // Only update password hash if it's provided
            if (!string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                existingUser.PasswordHash = user.PasswordHash;
            }

            _context.Users.Update(existingUser);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User updated successfully: {UserId}", user.Id);
            return existingUser;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {UserId}", user?.Id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return false;

            // Soft delete - mark as inactive
            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User deleted (soft delete): {UserId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user: {UserId}", id);
            throw;
        }
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<UserPreferences?> GetPreferencesAsync(Guid userId)
    {
        try
        {
            return await _context.UserPreferences
                .FirstOrDefaultAsync(p => p.UserId == userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user preferences: {UserId}", userId);
            throw;
        }
    }

    public async Task UpdatePreferencesAsync(Guid userId, UserPreferences prefs)
    {
        try
        {
            if (prefs == null)
                throw new ArgumentNullException(nameof(prefs));

            var existingPrefs = await GetPreferencesAsync(userId);
            
            if (existingPrefs == null)
            {
                // Create new preferences
                prefs.Id = Guid.NewGuid();
                prefs.UserId = userId;
                prefs.CreatedAt = DateTime.UtcNow;
                prefs.UpdatedAt = DateTime.UtcNow;
                
                _context.UserPreferences.Add(prefs);
            }
            else
            {
                // Update existing preferences
                existingPrefs.UseEmail = prefs.UseEmail;
                existingPrefs.UseSms = prefs.UseSms;
                existingPrefs.QuietFromHour = prefs.QuietFromHour;
                existingPrefs.QuietToHour = prefs.QuietToHour;
                existingPrefs.UpdatedAt = DateTime.UtcNow;
                
                _context.UserPreferences.Update(existingPrefs);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("User preferences updated: {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user preferences: {UserId}", userId);
            throw;
        }
    }
}


   
