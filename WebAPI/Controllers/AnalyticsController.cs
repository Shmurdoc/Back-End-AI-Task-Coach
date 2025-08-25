using Application.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "RequireCoach")]
[Produces("application/json")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analytics;
    public AnalyticsController(IAnalyticsService analytics) => _analytics = analytics;

    [HttpGet("productivity/{userId:guid}")]
    public async Task<IActionResult> ProductivitySummary([FromRoute] Guid userId)
    {
        var result = await _analytics.GetProductivitySummaryAsync(userId);
        return Ok(new { success = true, result });
    }
}
