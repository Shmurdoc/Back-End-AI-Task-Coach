namespace Application.IService;

using Application.DTOs.AuthDtos;
using Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

public interface IUserService
{
    Task<UserDto?> GetUserAsync(Guid id);
    Task UpdateUserAsync(UserDto dto);
    Task UpdatePreferencesAsync(Guid userId, UserPreferences prefs);
}
