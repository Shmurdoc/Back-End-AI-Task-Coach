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
public class HabitAnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analytics;
    public HabitAnalyticsController(IAnalyticsService analytics) => _analytics = analytics;

    [HttpGet("habit-summary/{userId:guid}")]
    public async Task<IActionResult> HabitSummary(Guid userId, CancellationToken cancellationToken)
    {
        var summary = await _analytics.GetHabitSummaryAsync(userId, cancellationToken);
        return Ok(new { success = true, data = summary });
    }
}
