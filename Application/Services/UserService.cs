using Application.DTOs.AuthDtos;
using Application.IRepositories;
using Application.IService;
using Domain.Entities;

namespace Application.Services;

/// <summary>
/// Profile reads and edits for the authenticated user. Repositories stage;
/// the unit of work commits (repository convention).
/// </summary>
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UserService(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<UserDto?> GetUserAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return null;
        return MapToDto(user);
    }

    public async Task UpdateUserAsync(UserDto dto)
    {
        var user = await _userRepository.GetByIdAsync(dto.Id)
            ?? throw new InvalidOperationException("User not found.");

        // Email is the login identity: changing it here would silently break
        // GetByEmail lookups for duplicate accounts, so a changed email is
        // rejected instead of ignored. IsActive is an account-admin flag, not
        // self-service; CreatedAt is immutable.
        if (!string.Equals(user.Email, dto.Email, StringComparison.OrdinalIgnoreCase)
            && await _userRepository.ExistsAsync(dto.Email))
            throw new InvalidOperationException("Email already in use.");

        user.Name = dto.Name;
        user.Email = dto.Email;
        user.PhoneNumber = dto.PhoneNumber;
        await _userRepository.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UpdatePreferencesAsync(Guid userId, UserPreferences prefs)
    {
        await _userRepository.UpdatePreferencesAsync(userId, prefs);
        await _unitOfWork.SaveChangesAsync();
    }

    private static UserDto MapToDto(User user) => new()
    {
        Id = user.Id,
        Email = user.Email,
        Name = user.Name,
        PhoneNumber = user.PhoneNumber,
        IsActive = user.IsActive,
        CreatedAt = user.CreatedAt
    };
}
