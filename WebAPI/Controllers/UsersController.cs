using Application.DTOs.AuthDtos;
using Application.IRepositories;
using Application.IService;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Filters;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnsureSelfUser]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IUserService _users;
    private readonly IUserContext _userContext;
    public UsersController(IUserService users, IUserContext userContext)
    {
        _users = users;
        _userContext = userContext;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        if (id != _userContext.GetCurrentUserId())
            return NotFound(new { success = false, error = "User not found" });
        var user = await _users.GetUserAsync(id);
        if (user == null)
            return NotFound(new { success = false, error = "User not found" });
        return Ok(new { success = true, data = user });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UserDto dto)
    {
        var currentUserId = _userContext.GetCurrentUserId();
        if (id != currentUserId || dto.Id != currentUserId)
            return NotFound(new { success = false, error = "User not found" });
        try
        {
            await _users.UpdateUserAsync(dto);
        }
        catch (InvalidOperationException ex) when (ex.Message == "User not found.")
        {
            return NotFound(new { success = false, error = "User not found" });
        }
        catch (InvalidOperationException ex) when (ex.Message == "Email already in use.")
        {
            return Conflict(new { success = false, error = ex.Message });
        }
        return Ok(new { success = true });
    }

    [HttpPost("preferences/{userId:guid}")]
    public async Task<IActionResult> UpdatePreferences(Guid userId, [FromBody] UserPreferences prefs)
    {
        await _users.UpdatePreferencesAsync(userId, prefs);
        return Ok(new { success = true });
    }
}
