using Application.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class HabitAnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analytics;
    public HabitAnalyticsController(IAnalyticsService analytics) => _analytics = analytics;

    [HttpGet("habit-summary/{userId:guid}")]
    public async Task<IActionResult> HabitSummary(Guid userId)
    {
    await _analytics.GetHabitSummaryAsync(userId);
    return Ok(new { success = true });
    }
}
