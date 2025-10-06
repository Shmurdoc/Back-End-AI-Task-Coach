using Application.DTOs.Analytics;
using Application.IService.Analytics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class EnergyMoodController : ControllerBase
{
    private readonly IEnergyMoodService _service;
    public EnergyMoodController(IEnergyMoodService service) => _service = service;

    [HttpPost("record")]
    public async Task<IActionResult> Record([FromBody] RecordRequest req)
    {
        var entry = new EnergyMoodEntryDto(req.UserId, req.Timestamp ?? DateTime.UtcNow, req.EnergyLevel, req.MoodLevel, req.Note);
        await _service.RecordEntryAsync(entry);
        return Ok(new { success = true });
    }

    [HttpGet("entries/{userId:guid}")]
    public async Task<IActionResult> GetEntries(Guid userId, [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
    {
        var entries = await _service.GetEntriesAsync(userId, from, to);
        return Ok(new { success = true, entries });
    }

    [HttpGet("averages/{userId:guid}")]
    public async Task<IActionResult> GetAverages(Guid userId, [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
    {
        var (avgEnergy, avgMood) = await _service.GetAveragesAsync(userId, from, to);
        return Ok(new { success = true, avgEnergy, avgMood });
    }

    public record RecordRequest(
        [Required] Guid UserId,
        [Range(1,10)] int EnergyLevel,
        [Range(1,10)] int MoodLevel,
        string? Note = null,
        DateTime? Timestamp = null);
}
