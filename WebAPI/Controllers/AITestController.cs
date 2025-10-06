using Application.IService;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

/// <summary>
/// Controller to test and demonstrate AI functionality integration
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AITestController : ControllerBase
{
    private readonly IAIService _aiService;
    private readonly ILogger<AITestController> _logger;

    public AITestController(IAIService aiService, ILogger<AITestController> logger)
    {
        _aiService = aiService;
        _logger = logger;
    }

    /// <summary>
    /// Test endpoint to verify AI task suggestion functionality
    /// </summary>
    /// <param name="taskDescription">The task to get AI suggestions for</param>
    /// <returns>AI-generated task suggestion</returns>
    [HttpPost("task-suggestion")]
    public async Task<IActionResult> GetTaskSuggestion([FromBody] string taskDescription)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(taskDescription))
            {
                return BadRequest("Task description cannot be empty");
            }

            _logger.LogInformation("Testing AI task suggestion for: {TaskDescription}", taskDescription);
            
            var suggestion = await _aiService.GetTaskSuggestionAsync(taskDescription);
            
            return Ok(new { 
                Task = taskDescription, 
                AISuggestion = suggestion,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing AI task suggestion");
            return StatusCode(500, "An error occurred while processing the AI request");
        }
    }

    /// <summary>
    /// Test endpoint to verify AI reflection functionality
    /// </summary>
    /// <param name="reflection">The reflection input to process</param>
    /// <returns>AI-generated reflection response</returns>
    [HttpPost("reflection")]
    public async Task<IActionResult> ProcessReflection([FromBody] string reflection)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(reflection))
            {
                return BadRequest("Reflection input cannot be empty");
            }

            _logger.LogInformation("Testing AI reflection processing");
            
            var response = await _aiService.ReflectAsync(Guid.NewGuid(), reflection);
            
            return Ok(new { 
                UserInput = reflection, 
                AIResponse = response,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing AI reflection");
            return StatusCode(500, "An error occurred while processing the AI reflection");
        }
    }

    /// <summary>
    /// Health check endpoint for AI service integration
    /// </summary>
    /// <returns>Service status</returns>
    [HttpGet("health")]
    public IActionResult HealthCheck()
    {
        return Ok(new { 
            Status = "AI Service Integrated", 
            Timestamp = DateTime.UtcNow,
            Message = "AI service is properly registered and ready to use (requires valid OpenAI API key)"
        });
    }
}
