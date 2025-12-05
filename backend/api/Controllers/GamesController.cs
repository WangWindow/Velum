using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Velum.Core.Interfaces;
using Velum.Core.Models;

namespace Velum.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class GamesController(IGameService gameService) : ControllerBase
{
    private readonly IGameService _gameService = gameService;

    [HttpPost("score")]
    public async Task<IActionResult> SubmitScore([FromBody] SubmitScoreRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var score = await _gameService.SubmitScoreAsync(userId, request.GameName, request.Score, request.Duration);
        return Ok(score);
    }

    [HttpGet("my-scores")]
    public async Task<IActionResult> GetMyScores()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var scores = await _gameService.GetUserScoresAsync(userId);
        return Ok(scores);
    }

    [HttpGet("leaderboard/{gameName}")]
    public async Task<IActionResult> GetLeaderboard(string gameName)
    {
        var scores = await _gameService.GetTopScoresAsync(gameName);
        return Ok(scores);
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
