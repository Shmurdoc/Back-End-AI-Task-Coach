using Application.DTOs.Analytics;
using Application.IService.Analytics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using WebAPI.Filters;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnsureSelfUser]
[Produces("application/json")]
public class EnergyMoodController : ControllerBase
{
    private readonly IEnergyMoodService _service;
    public EnergyMoodController(IEnergyMoodService service) => _service = service;

    [HttpPost("record")]
    public async Task<IActionResult> Record([FromBody] RecordRequest req, CancellationToken ct)
    {
        var entry = new EnergyMoodEntryDto(req.UserId, req.Timestamp ?? DateTime.UtcNow, req.EnergyLevel, req.MoodLevel, req.Note);
        await _service.RecordEntryAsync(entry, ct);
        return Ok(new { success = true, data = entry });
    }

    [HttpGet("entries/{userId:guid}")]
    public async Task<IActionResult> GetEntries(Guid userId, [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null, CancellationToken ct = default)
    {
        var entries = await _service.GetEntriesAsync(userId, from, to, ct);
        return Ok(new { success = true, data = entries });
    }

    [HttpGet("averages/{userId:guid}")]
    public async Task<IActionResult> GetAverages(Guid userId, [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null, CancellationToken ct = default)
    {
        var (avgEnergy, avgMood) = await _service.GetAveragesAsync(userId, from, to, ct);
        return Ok(new { success = true, data = new { avgEnergy, avgMood } });
    }

    public record RecordRequest(
        [Required] Guid UserId,
        [Range(1,10)] int EnergyLevel,
        [Range(1,10)] int MoodLevel,
        string? Note = null,
        DateTime? Timestamp = null);
}
