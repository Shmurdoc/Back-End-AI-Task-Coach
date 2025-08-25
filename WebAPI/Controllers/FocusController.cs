using Application.IService.Focus;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class FocusController : ControllerBase
{
    private readonly IFocusToolIntegrationService _focus;
    public FocusController(IFocusToolIntegrationService focus) => _focus = focus;

    [HttpPost("start-session/{userId:guid}")]
    public async Task<IActionResult> StartSession(Guid userId, [FromQuery, Range(1,180)] int minutes = 25)
    {
        var ok = await _focus.StartFocusSessionAsync(userId, minutes);
        return Ok(new { success = ok });
    }

    [HttpPost("end-session/{userId:guid}")]
    public async Task<IActionResult> EndSession(Guid userId)
    {
        var ok = await _focus.EndFocusSessionAsync(userId);
        return Ok(new { success = ok });
    }

    [HttpPost("sync-session/{userId:guid}")]
    public async Task<IActionResult> SyncSession(Guid userId, [FromBody] SyncFocusSessionRequest req)
    {
        var ok = await _focus.SyncFocusSessionAsync(userId, req.Start, req.End);
        return Ok(new { success = ok });
    }

    public record SyncFocusSessionRequest([Required] DateTime Start, [Required] DateTime End);
}
