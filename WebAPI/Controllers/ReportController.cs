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
public class ReportController : ControllerBase
{
    private readonly IAnalyticsService _analytics;
    public ReportController(IAnalyticsService analytics) => _analytics = analytics;

    [HttpGet("user-report/{userId:guid}")]
    public async Task<IActionResult> UserReport(Guid userId, CancellationToken cancellationToken)
    {
        var report = await _analytics.GenerateUserReportAsync(userId, cancellationToken);
        return Ok(new { success = true, data = report });
    }
}
