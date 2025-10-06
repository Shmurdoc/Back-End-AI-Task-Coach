using Application.DTOs.AuthDtos;
using Application.IService;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IUserService _users;
    public UsersController(IUserService users) => _users = users;

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var u = await _users.GetUserAsync(id);
        if (u==null) return NotFound(new { success=false, error = "User not found" });
        return Ok(new { success=true, data = u });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UserDto dto) { if (id!=dto.Id) return BadRequest(new { success=false, error = "Id mismatch"}); await _users.UpdateUserAsync(dto); return Ok(new { success=true }); }

    [HttpPost("preferences/{userId:guid}")]
    public async Task<IActionResult> UpdatePreferences(Guid userId, [FromBody] UserPreferences prefs) { await _users.UpdatePreferencesAsync(userId, prefs); return Ok(new { success=true }); }
}
