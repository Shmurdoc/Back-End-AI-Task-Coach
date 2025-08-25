using Application.DTOs.HabitDtos;
using Application.IService;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class HabitsController : ControllerBase
{
    private readonly IHabitService _habits;
    public HabitsController(IHabitService habits) => _habits = habits;

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id) { var h = await _habits.GetHabitAsync(id); if (h==null) return NotFound(new { success=false, error="Not found" }); return Ok(new { success=true, data=h }); }

    [HttpGet("user/{userId:guid}")]
    public async Task<IActionResult> GetForUser(Guid userId) => Ok(new { success = true, data = await _habits.GetUserHabitsAsync(userId) });

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] HabitDto dto)
    {
        var created = await _habits.CreateHabitAsync(dto);
        if (created == null)
            return BadRequest(new { success = false, error = "Failed to create habit." });
        return CreatedAtAction(nameof(Get), new { id = created.Id }, new { success = true, data = created });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] HabitDto dto) { if (id!=dto.Id) return BadRequest(new { success=false, error="Id mismatch" }); await _habits.UpdateHabitAsync(dto); return Ok(new { success=true }); }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id) { await _habits.DeleteHabitAsync(id); return Ok(new { success=true }); }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index()
        {
            WebAPI.Extensions.ObservabilityExtensions.NudgesDelivered.Add(1);
            return Ok(new { success = true, message = "Habits API root." });
        }
}
