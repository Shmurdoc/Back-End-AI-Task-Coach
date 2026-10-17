using Application.DTOs.GoalDtos;
using Application.IRepositories;
using Application.IService;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Filters;
using WebAPI.Services;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnsureSelfUser]
[Produces("application/json")]
public class GoalsController : ControllerBase
{
    private readonly IGoalService _goalService;
    private readonly INotificationBroadcaster _broadcaster;
    private readonly IUserContext _userContext;
    public GoalsController(IGoalService goalService, INotificationBroadcaster broadcaster, IUserContext userContext)
    {
        _goalService = goalService;
        _broadcaster = broadcaster;
        _userContext = userContext;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var g = await _goalService.GetGoalByIdAsync(id, cancellationToken);
        if (g is null) return NotFound(new { success = false, error = "Goal not found" });
        return Ok(new { success = true, data = g });
    }

    [HttpGet("user/{userId:guid}")]
    public async Task<IActionResult> GetForUser(Guid userId, CancellationToken cancellationToken)
        => Ok(new { success = true, data = await _goalService.GetUserGoalsAsync(userId, cancellationToken) });

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateGoalDto dto, CancellationToken cancellationToken)
    {
        var created = await _goalService.CreateGoalAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, new { success = true, data = created });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateGoalDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var updated = await _goalService.UpdateGoalAsync(id, dto, cancellationToken);
            if (dto.Status == GoalStatus.Completed)
                await _broadcaster.GoalCompletedAsync(_userContext.GetCurrentUserId(), updated, cancellationToken);
            return Ok(new { success = true, data = updated });
        }
        catch (InvalidOperationException)
        {
            return NotFound(new { success = false, error = "Goal not found" });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _goalService.DeleteGoalAsync(id, cancellationToken);
        if (!deleted) return NotFound(new { success = false, error = "Goal not found" });
        return Ok(new { success = true });
    }
}
