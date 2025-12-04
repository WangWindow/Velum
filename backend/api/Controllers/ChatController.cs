using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Velum.Core.Interfaces;
using Velum.Core.Models;

namespace Velum.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ChatController(IChatService chatService) : ControllerBase
{
    private readonly IChatService _chatService = chatService;

    [HttpGet("sessions")]
    public async Task<ActionResult<IEnumerable<ChatSession>>> GetSessions()
    {
        var userIdClaim = User.FindFirst("UserId");
        if (userIdClaim == null) return Unauthorized();
        var userId = int.Parse(userIdClaim.Value);

        var sessions = await _chatService.GetUserSessionsAsync(userId);
        return Ok(sessions);
    }

    [HttpPost("sessions")]
    public async Task<ActionResult<ChatSession>> CreateSession([FromBody] CreateSessionRequest? request)
    {
        var userIdClaim = User.FindFirst("UserId");
        if (userIdClaim == null) return Unauthorized();
        var userId = int.Parse(userIdClaim.Value);

        var session = await _chatService.CreateSessionAsync(userId, request?.Title);
        return Ok(session);
    }

    [HttpGet("sessions/{id}")]
    public async Task<ActionResult<ChatSession>> GetSession(int id)
    {
        var userIdClaim = User.FindFirst("UserId");
        if (userIdClaim == null) return Unauthorized();
        var userId = int.Parse(userIdClaim.Value);

        var session = await _chatService.GetSessionAsync(id, userId);
        if (session == null) return NotFound();

        return Ok(session);
    }

    [HttpDelete("sessions/{id}")]
    public async Task<IActionResult> DeleteSession(int id)
    {
        var userIdClaim = User.FindFirst("UserId");
        if (userIdClaim == null) return Unauthorized();
        var userId = int.Parse(userIdClaim.Value);

        await _chatService.DeleteSessionAsync(id, userId);
        return NoContent();
    }

    [HttpGet("history")]
    public async Task<ActionResult<IEnumerable<ChatMessage>>> GetHistory()
    {
        var userIdClaim = User.FindFirst("UserId");
        if (userIdClaim == null) return Unauthorized();
        var userId = int.Parse(userIdClaim.Value);

        var history = await _chatService.GetChatHistoryAsync(userId);
        return Ok(history);
    }

    [HttpDelete("history")]
    public async Task<IActionResult> ClearHistory()
    {
        var userIdClaim = User.FindFirst("UserId");
        if (userIdClaim == null) return Unauthorized();
        var userId = int.Parse(userIdClaim.Value);

        await _chatService.ClearChatHistoryAsync(userId);
        return NoContent();
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
    {
        var userIdClaim = User.FindFirst("UserId");
        if (userIdClaim == null) return Unauthorized();
        var userId = int.Parse(userIdClaim.Value);

        try
        {
            var aiMessage = await _chatService.ProcessUserMessageAsync(userId, request.Message, request.SessionId);
            return Ok(aiMessage);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error processing message: {ex.Message}");
        }
    }
}

public class CreateSessionRequest
{
    public string? Title { get; set; }
}

public class ChatRequest
{
    public required string Message { get; set; }
    public int? SessionId { get; set; }
}

public class ChatResponse
{
    public required string Response { get; set; }
}
