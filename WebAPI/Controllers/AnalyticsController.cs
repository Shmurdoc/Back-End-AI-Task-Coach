using Application.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Filters;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnsureSelfUser]
[Produces("application/json")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analytics;
    public AnalyticsController(IAnalyticsService analytics) => _analytics = analytics;

    [HttpGet("productivity/{userId:guid}")]
    public async Task<IActionResult> ProductivitySummary([FromRoute] Guid userId, CancellationToken cancellationToken)
    {
        var data = await _analytics.GetProductivitySummaryAsync(userId, cancellationToken);
        return Ok(new { success = true, data });
    }
}
