using Application.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "RequireCoach")]
[Produces("application/json")]
public class ReportController : ControllerBase
{
    private readonly IAnalyticsService _analytics;
    public ReportController(IAnalyticsService analytics) => _analytics = analytics;

    [HttpGet("user-report/{userId:guid}")]
    public async Task<IActionResult> UserReport(Guid userId)
    {
    await _analytics.GenerateUserReportAsync(userId);
    return Ok(new { success = true });
    }
}
