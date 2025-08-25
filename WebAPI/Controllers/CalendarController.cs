using Application.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class CalendarController : ControllerBase
{
    private readonly ICalendarExportService _cal;
    public CalendarController(ICalendarExportService cal) => _cal = cal;

    [HttpGet("daily/{userId:guid}/{date}")]
    public async Task<IActionResult> Daily(Guid userId, DateTime date)
    {
        var ics = await _cal.GenerateDailyIcsAsync(userId, date);
        return File(System.Text.Encoding.UTF8.GetBytes(ics), "text/calendar", $"daily-{userId}-{date:yyyyMMdd}.ics");
    }

    [HttpGet("weekly/{userId:guid}/{startDate}")]
    public async Task<IActionResult> Weekly(Guid userId, DateTime startDate)
    {
        var ics = await _cal.GenerateWeeklyIcsAsync(userId, startDate);
        return File(System.Text.Encoding.UTF8.GetBytes(ics), "text/calendar", $"weekly-{userId}-{startDate:yyyyMMdd}.ics");
    }
}
