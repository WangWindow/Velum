using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Velum.Core.Interfaces;
using Velum.Core.Models;

namespace Velum.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ChatController(IChatService chatService, ILogService logService) : ControllerBase
{
    private readonly IChatService _chatService = chatService;
    private readonly ILogService _logService = logService;

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

        await _logService.LogInfoAsync(
            message: $"Chat session created: {session.Title}",
            userId: userId,
            action: "CreateChatSession",
            resource: "Chat"
        );

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

    [HttpPut("sessions/{id}")]
    public async Task<ActionResult<ChatSession>> UpdateSession(int id, [FromBody] UpdateSessionRequest request)
    {
        var userIdClaim = User.FindFirst("UserId");
        if (userIdClaim == null) return Unauthorized();
        var userId = int.Parse(userIdClaim.Value);

        var session = await _chatService.UpdateSessionAsync(id, userId, request.Title);
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

        await _logService.LogInfoAsync(
            message: $"Chat session deleted: {id}",
            userId: userId,
            action: "DeleteChatSession",
            resource: "Chat"
        );

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

        await _logService.LogWarningAsync(
            message: "Chat history cleared",
            userId: userId,
            action: "ClearChatHistory",
            resource: "Chat"
        );

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

    [HttpPost("stream")]
    public async Task SendMessageStream([FromBody] ChatRequest request)
    {
        var userIdClaim = User.FindFirst("UserId");
        if (userIdClaim == null)
        {
            Response.StatusCode = 401;
            return;
        }
        var userId = int.Parse(userIdClaim.Value);

        Response.ContentType = "text/plain";

        try
        {
            await foreach (var chunk in _chatService.ProcessUserMessageStreamingAsync(userId, request.Message, request.SessionId))
            {
                await Response.WriteAsync(chunk);
                await Response.Body.FlushAsync();
            }
        }
        catch (Exception ex)
        {
            // Log error?
            Console.WriteLine($"Error in stream: {ex.Message}");
            Response.StatusCode = 500;
        }
    }
}

public class CreateSessionRequest
{
    public string? Title { get; set; }
}

public class UpdateSessionRequest
{
    public required string Title { get; set; }
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
