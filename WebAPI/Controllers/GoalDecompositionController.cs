using Application.IService.Goal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class GoalDecompositionController : ControllerBase
{
    private readonly IGoalDecompositionService _service;
    public GoalDecompositionController(IGoalDecompositionService service) => _service = service;

    /// <summary>
    /// Decomposes a stored goal the caller owns into category-aware milestone
    /// steps. The title is read from the database, not the request body.
    /// Returns 404 when the goal is missing or owned by another user.
    /// </summary>
    [HttpPost("decompose")]
    public async Task<IActionResult> Decompose([FromBody] DecomposeRequest req, CancellationToken ct)
    {
        var result = await _service.DecomposeGoalAsync(req.GoalId, ct);
        if (result is null)
            return NotFound(new { success = false, error = "Goal not found" });
        return Ok(new { success = true, data = result });
    }

    public record DecomposeRequest([Required] Guid GoalId);
}
