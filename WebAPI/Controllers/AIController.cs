using Application.IService;
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
public class AIController : ControllerBase
{
    private readonly IAIService _ai;
    private readonly ILogger<AIController> _logger;

    public AIController(IAIService ai, ILogger<AIController> logger)
    {
        _ai = ai;
        _logger = logger;
    }
    /// <summary>
    /// Returns an AI suggestion for approaching the given task (breakdown,
    /// prioritization, or execution tips). Falls back to a static message when
    /// no OpenAI key is configured.
    /// </summary>
    [HttpPost("predict-task")]
    public async Task<IActionResult> PredictTask([FromBody, Required] TaskPredictionRequest req, CancellationToken ct)
    {
        var data = await _ai.GetTaskSuggestionAsync($"Task: {req.Task.Title} - {req.Task.Description}", ct);
        return Ok(new { success = true, data });
    }

    /// <summary>
    /// Returns an AI suggestion for approaching the given goal. Falls back to
    /// a static message when no OpenAI key is configured.
    /// </summary>
    [HttpPost("predict-goal")]
    public async Task<IActionResult> PredictGoal([FromBody, Required] GoalPredictionRequest req, CancellationToken ct)
    {
        var data = await _ai.GetTaskSuggestionAsync($"Goal: {req.Goal.Title} - {req.Goal.Description}", ct);
        return Ok(new { success = true, data });
    }
    public record TaskPredictionRequest([Required] Application.DTOs.TaskDtos.TaskDto Task, [Required] Guid UserId);
    public record GoalPredictionRequest([Required] Application.DTOs.GoalDtos.GoalDto Goal, [Required] Guid UserId);

    /// <summary>
    /// Returns an AI suggestion for approaching the given task description.
    /// Falls back to a static message when no OpenAI key is configured.
    /// </summary>
    [HttpPost("suggest-task")]
    public async Task<IActionResult> SuggestTask([FromBody, Required] TaskSuggestRequest req, CancellationToken ct)
    {
        var data = await _ai.GetTaskSuggestionAsync(req.TaskDescription, ct);
        return Ok(new { success = true, data });
    }

    /// <summary>
    /// Analyzes the caller's real task/goal/habit/energy data and returns
    /// productivity insights. With no OpenAI key configured, returns local
    /// insights derived from the same data instead of an LLM response.
    /// </summary>
    [HttpGet("analyze/{userId:guid}")]
    public async Task<IActionResult> Analyze([FromRoute] Guid userId, CancellationToken ct)
    {
        var data = await _ai.AnalyzeUserPatternsAsync(userId, ct);
        return Ok(new { success = true, data });
    }

    /// <summary>
    /// Generates a weekly plan grounded in the caller's real goals and open
    /// tasks for the requested week. With no OpenAI key configured, returns a
    /// local outline derived from the same data.
    /// </summary>
    [HttpPost("weekly-plan/{userId:guid}")]
    public async Task<IActionResult> WeeklyPlan([FromRoute] Guid userId, [FromBody] WeeklyPlanRequest req, CancellationToken ct)
    {
        if (!DateTime.TryParse(req.Context, out var weekStart))
        {
            return BadRequest(new { success = false, error = "Invalid or missing week start date." });
        }
        var data = await _ai.GenerateWeeklyPlanAsync(userId, weekStart, ct);
        return Ok(new { success = true, data });
    }

    /// <summary>
    /// Returns a mindful coaching response to the caller's reflection text.
    /// Falls back to a static message when no OpenAI key is configured.
    /// </summary>
    [HttpPost("reflect/{userId:guid}")]
    public async Task<IActionResult> Reflect([FromRoute] Guid userId, [FromBody] ReflectionRequest req, CancellationToken ct)
    {
        var data = await _ai.ReflectAsync(userId, req.Text ?? "", ct);
        return Ok(new { success = true, data });
    }

    public record TaskSuggestRequest([Required] string TaskDescription);
    public record WeeklyPlanRequest(string? Context);
    public record ReflectionRequest(string? Text);
}
