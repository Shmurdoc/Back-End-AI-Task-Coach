using Application.DTOs.HabitDtos;
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
public class HabitsController : ControllerBase
{
    private readonly IHabitService _habits;
    public HabitsController(IHabitService habits) => _habits = habits;

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var habit = await _habits.GetHabitByIdAsync(id, cancellationToken);
        if (habit == null)
            return NotFound(new { success = false, error = "Not found" });
        return Ok(new { success = true, data = habit });
    }

    [HttpGet("user/{userId:guid}")]
    public async Task<IActionResult> GetForUser(Guid userId)
        => Ok(new { success = true, data = await _habits.GetUserHabitsAsync(userId) });

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateHabitDto dto, CancellationToken cancellationToken)
    {
        var created = await _habits.CreateHabitAsync(dto, cancellationToken);
        if (created == null)
            return BadRequest(new { success = false, error = "Failed to create habit." });
        return CreatedAtAction(nameof(Get), new { id = created.Id }, new { success = true, data = created });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateHabitDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var updated = await _habits.UpdateHabitAsync(id, dto, cancellationToken);
            return Ok(new { success = true, data = updated });
        }
        catch (InvalidOperationException)
        {
            return NotFound(new { success = false, error = "Not found" });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _habits.DeleteHabitAsync(id, cancellationToken);
        if (!deleted)
            return NotFound(new { success = false, error = "Not found" });
        return Ok(new { success = true });
    }

    [HttpGet]
    public IActionResult Index()
    {
        return Ok(new { success = true, message = "Habits API root." });
    }
}
