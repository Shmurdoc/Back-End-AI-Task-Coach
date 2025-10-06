using Application.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
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
    /// Predicts risk and suggests scheduling for a task using AI/ML.
    /// </summary>
    [HttpPost("predict-task")]
    public async Task<IActionResult> PredictTask([FromBody, Required] TaskPredictionRequest req)
    {
        var result = await _ai.GetTaskSuggestionAsync($"Task: {req.Task.Title} - {req.Task.Description}");
        return Ok(new { success = true, result });
    }

    /// <summary>
    /// Gets AI/ML-based suggestions for a user.
    /// </summary>
    [HttpGet("suggestions/{userId:guid}")]
    public async Task<IActionResult> GetSuggestions([FromRoute] Guid userId)
    {
        var suggestions = await _ai.AnalyzeUserPatternsAsync(userId);
        return Ok(new { success = true, suggestions });
    }

    /// <summary>
    /// Predicts risk and decomposition for a goal using AI/ML.
    /// </summary>
    [HttpPost("predict-goal")]
    public async Task<IActionResult> PredictGoal([FromBody, Required] GoalPredictionRequest req)
    {
        var result = await _ai.GetTaskSuggestionAsync($"Goal: {req.Goal.Title} - {req.Goal.Description}");
        return Ok(new { success = true, result });
    }
    public record TaskPredictionRequest([Required] Application.DTOs.TaskDtos.TaskDto Task, [Required] Guid UserId);
    public record GoalPredictionRequest([Required] Application.DTOs.GoalDtos.GoalDto Goal, [Required] Guid UserId);

    [HttpPost("suggest-task")]
    public async Task<IActionResult> SuggestTask([FromBody, Required] TaskSuggestRequest req, CancellationToken ct)
    {
        var result = await _ai.GetTaskSuggestionAsync(req.TaskDescription);
        return Ok(new { success = true, result });
    }

    [HttpGet("analyze/{userId:guid}")]
    public async Task<IActionResult> Analyze([FromRoute] Guid userId, CancellationToken ct)
    {
        var result = await _ai.AnalyzeUserPatternsAsync(userId);
        return Ok(new { success = true, result });
    }

    [HttpPost("weekly-plan/{userId:guid}")]
    public async Task<IActionResult> WeeklyPlan([FromRoute] Guid userId, [FromBody] WeeklyPlanRequest req, CancellationToken ct)
    {
        if (!DateTime.TryParse(req.Context, out var weekStart))
        {
            return BadRequest(new { success = false, error = "Invalid or missing week start date." });
        }
        var result = await _ai.GenerateWeeklyPlanAsync(userId, weekStart, ct);
        return Ok(new { success = true, result });
    }

    [HttpPost("reflect/{userId:guid}")]
    public async Task<IActionResult> Reflect([FromRoute] Guid userId, [FromBody] ReflectionRequest req, CancellationToken ct)
    {
        var result = await _ai.ReflectAsync(userId, req.Text ?? "");
        return Ok(new { success = true, result });
    }

    public record TaskSuggestRequest([Required] string TaskDescription);
    public record WeeklyPlanRequest(string? Context);
    public record ReflectionRequest(string? Text);
}
