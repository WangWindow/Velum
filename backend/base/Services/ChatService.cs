using Microsoft.EntityFrameworkCore;
using Velum.Core.Enums;
using Velum.Core.Interfaces;
using Velum.Core.Models;
using Velum.Base.Data;

namespace Velum.Base.Services;

public class ChatService(ApplicationDbContext context, IOpenAIService openAIService) : IChatService
{
    private readonly ApplicationDbContext _context = context;
    private readonly IOpenAIService _openAIService = openAIService;

    public async Task<IEnumerable<ChatMessage>> GetChatHistoryAsync(int userId)
    {
        return await _context.ChatMessages
            .Where(m => m.UserId == userId)
            .OrderBy(m => m.Timestamp)
            .ToListAsync();
    }

    public async Task<ChatMessage> ProcessUserMessageAsync(int userId, string message)
    {
        // Save user message
        var userMessage = new ChatMessage
        {
            UserId = userId,
            Role = ChatRoleType.User,
            Content = message
        };
        _context.ChatMessages.Add(userMessage);
        await _context.SaveChangesAsync();

        // Get AI response
        var aiResponseContent = await _openAIService.GetChatCompletionAsync(message);

        // Save AI response
        var aiMessage = new ChatMessage
        {
            UserId = userId,
            Role = ChatRoleType.Assistant,
            Content = aiResponseContent
        };
        _context.ChatMessages.Add(aiMessage);
        await _context.SaveChangesAsync();

        return aiMessage;
    }
}
