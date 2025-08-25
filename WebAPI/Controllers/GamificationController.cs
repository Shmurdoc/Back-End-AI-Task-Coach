using Application.IService.Analytics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class GamificationController : ControllerBase
{
    private readonly IGamificationService _service;
    public GamificationController(IGamificationService service) => _service = service;

    [HttpGet("summary/{userId:guid}")]
    public async Task<IActionResult> GetSummary(Guid userId)
    {
        var summary = await _service.GetGamificationSummaryAsync(userId);
        return Ok(new { success = true, summary });
    }
}
