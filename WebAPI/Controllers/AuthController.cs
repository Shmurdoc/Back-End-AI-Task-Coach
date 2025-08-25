using Application.IService;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly ITokenService _tokenService;
    private readonly IUserService? _userService;

    public AuthController(ITokenService tokenService, IUserService? userService = null) { _tokenService = tokenService; _userService = userService; }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody, Required] LoginRequest req)
    {
        var token = await _tokenService.GenerateTokenAsync(req.Email, req.Password);
        if (string.IsNullOrEmpty(token)) return Unauthorized(new { success = false, error = "Invalid credentials" });
        return Ok(new { success = true, token });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody, Required] RefreshRequest req)
    {
        var token = await _tokenService.RefreshTokenAsync(req.RefreshToken);
        if (string.IsNullOrEmpty(token)) return Unauthorized(new { success = false, error = "Invalid refresh token" });
        return Ok(new { success = true, token });
    }

    public record LoginRequest([Required] string Email, [Required] string Password);
    public record RefreshRequest([Required] string RefreshToken);
}
