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

    [HttpGet("history")]
    public async Task<ActionResult<IEnumerable<ChatMessage>>> GetHistory()
    {
        var userIdClaim = User.FindFirst("UserId");
        if (userIdClaim == null) return Unauthorized();
        var userId = int.Parse(userIdClaim.Value);

        var history = await _chatService.GetChatHistoryAsync(userId);
        return Ok(history);
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
    {
        var userIdClaim = User.FindFirst("UserId");
        if (userIdClaim == null) return Unauthorized();
        var userId = int.Parse(userIdClaim.Value);

        try
        {
            var aiMessage = await _chatService.ProcessUserMessageAsync(userId, request.Message);
            return Ok(aiMessage);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error processing message: {ex.Message}");
        }
    }
}


public class ChatRequest
{
    public required string Message { get; set; }
}

public class ChatResponse
{
    public required string Response { get; set; }
}
