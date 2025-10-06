using Application.IService;
using Application.DTOs.Gamification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class GamificationController : ControllerBase
{
    private readonly IGamificationService _service;
    public GamificationController(IGamificationService service) => _service = service;

    [HttpGet("summary/{userId:guid}")]
    public async Task<IActionResult> GetSummary(Guid userId)
    {
        var summary = await _service.GetGamificationSummaryAsync(userId);
        return Ok(new { success = true, summary });
    }

    [HttpGet("points/{userId:guid}")]
    public async Task<IActionResult> GetUserPoints(Guid userId)
    {
        var points = await _service.GetUserPointsAsync(userId);
        var level = await _service.GetUserLevelAsync(userId);
        var experience = await _service.GetUserExperienceAsync(userId);
        return Ok(new { success = true, points, level, experience });
    }

    [HttpGet("badges/{userId:guid}")]
    public async Task<IActionResult> GetUserBadges(Guid userId)
    {
        var badges = await _service.GetUserBadgesAsync(userId);
        return Ok(new { success = true, badges });
    }

    [HttpGet("achievements/{userId:guid}")]
    public async Task<IActionResult> GetAchievements(Guid userId)
    {
        var achievements = await _service.CheckAchievementsAsync(userId);
        return Ok(new { success = true, achievements });
    }

    [HttpGet("challenges/{userId:guid}")]
    public async Task<IActionResult> GetActiveChallenges(Guid userId)
    {
        var challenges = await _service.GetActiveChallengesAsync(userId);
        return Ok(new { success = true, challenges });
    }

    [HttpGet("leaderboard")]
    public async Task<IActionResult> GetLeaderboard([FromQuery] LeaderboardType type = LeaderboardType.Points, [FromQuery] int limit = 10)
    {
        var leaderboard = await _service.GetLeaderboardAsync(type, limit);
        return Ok(new { success = true, leaderboard, type });
    }

    [HttpGet("motivation/{userId:guid}")]
    public async Task<IActionResult> GetMotivationalMessage(Guid userId)
    {
        var message = await _service.GetMotivationalMessageAsync(userId);
        return Ok(new { success = true, message });
    }

    [HttpPost("complete-objective")]
    public async Task<IActionResult> CompleteObjective([FromBody] CompleteObjectiveRequest request)
    {
        var result = await _service.CompleteObjectiveAsync(request.UserId, request.ObjectiveType, request.Data);
        var celebrationMessage = await _service.GetCelebrationMessageAsync(request.UserId, request.ObjectiveType);
        return Ok(new { success = result, celebrationMessage });
    }

    [HttpPost("award-points")]
    public async Task<IActionResult> AwardPoints([FromBody] AwardPointsRequest request)
    {
        var newTotal = await _service.AwardPointsAsync(request.UserId, request.Points, request.Reason);
        return Ok(new { success = true, newTotal, awarded = request.Points });
    }
}

public record CompleteObjectiveRequest(Guid UserId, string ObjectiveType, object Data);
public record AwardPointsRequest(Guid UserId, int Points, string Reason);
