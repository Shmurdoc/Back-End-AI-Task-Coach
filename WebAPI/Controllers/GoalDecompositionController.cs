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

    [HttpPost("decompose")]
    public async Task<IActionResult> Decompose([FromBody] DecomposeRequest req)
    {
        var result = await _service.DecomposeGoalAsync(req.GoalId, req.GoalTitle);
        return Ok(new { success = true, result });
    }

    public record DecomposeRequest(
        [Required] Guid GoalId,
        [Required] string GoalTitle);
}
