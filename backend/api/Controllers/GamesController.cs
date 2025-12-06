using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Velum.Core.Interfaces;
using Velum.Core.Models;

namespace Velum.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class GamesController(IGameService gameService, ILogService logService) : ControllerBase
{
    private readonly IGameService _gameService = gameService;
    private readonly ILogService _logService = logService;

    [HttpPost("score")]
    public async Task<IActionResult> SubmitScore([FromBody] SubmitScoreRequest request)
    {
        var userIdClaim = User.FindFirst("UserId");
        if (userIdClaim == null) return Unauthorized();
        var userId = int.Parse(userIdClaim.Value);

        var score = await _gameService.SubmitScoreAsync(userId, request.GameName, request.Score, request.Duration);

        await _logService.LogInfoAsync(
            message: $"Score submitted for {request.GameName}: {request.Score} (Duration: {request.Duration}s)",
            userId: userId,
            action: "SubmitScore",
            resource: "Games"
        );

        return Ok(score);
    }

    [HttpGet("my-scores")]
    public async Task<IActionResult> GetMyScores()
    {
        var userIdClaim = User.FindFirst("UserId");
        if (userIdClaim == null) return Unauthorized();
        var userId = int.Parse(userIdClaim.Value);

        var scores = await _gameService.GetUserScoresAsync(userId);
        return Ok(scores);
    }

    [HttpGet("leaderboard/{gameName}")]
    public async Task<IActionResult> GetLeaderboard(string gameName)
    {
        var scores = await _gameService.GetTopScoresAsync(gameName, 5);
        var dtos = scores.Select(s => new LeaderboardEntryDto
        {
            Id = s.Id,
            GameName = s.GameName,
            Score = s.Score,
            Duration = s.Duration,
            PlayedAt = s.PlayedAt,
            Username = s.User?.Username ?? "Unknown",
            Avatar = s.User?.Avatar
        });
        return Ok(dtos);
    }

    [HttpGet("all")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetAllScores()
    {
        var scores = await _gameService.GetAllScoresAsync();
        return Ok(scores);
    }
}

public class SubmitScoreRequest
{
    public string GameName { get; set; } = string.Empty;
    public int Score { get; set; }
    public double Duration { get; set; }
}

public class LeaderboardEntryDto
{
    public int Id { get; set; }
    public string GameName { get; set; } = string.Empty;
    public int Score { get; set; }
    public double Duration { get; set; }
    public DateTime PlayedAt { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Avatar { get; set; }
}
