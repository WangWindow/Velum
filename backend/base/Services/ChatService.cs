using Microsoft.EntityFrameworkCore;
using Velum.Base.Data;
using Velum.Core.Enums;
using Velum.Core.Interfaces;
using Velum.Core.Models;

namespace Velum.Base.Services;

public class ChatService(ApplicationDbContext context, IOpenAIService openAIService) : IChatService
{
    private readonly ApplicationDbContext _context = context;
    private readonly IOpenAIService _openAIService = openAIService;

    public async Task<IEnumerable<ChatSession>> GetUserSessionsAsync(int userId)
    {
        return await _context.ChatSessions
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.UpdatedAt)
            .ToListAsync();
    }

    public async Task<ChatSession> CreateSessionAsync(int userId, string? title = null)
    {
        var session = new ChatSession
        {
            UserId = userId,
            Title = title ?? "New Chat",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.ChatSessions.Add(session);
        await _context.SaveChangesAsync();
        return session;
    }

    public async Task<ChatSession?> GetSessionAsync(int sessionId, int userId)
    {
        return await _context.ChatSessions
            .Include(s => s.Messages.OrderBy(m => m.Timestamp))
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId);
    }

    public async Task DeleteSessionAsync(int sessionId, int userId)
    {
        var session = await _context.ChatSessions
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId);

        if (session != null)
        {
            _context.ChatSessions.Remove(session);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<ChatMessage>> GetSessionMessagesAsync(int sessionId, int userId)
    {
        return await _context.ChatMessages
            .Where(m => m.SessionId == sessionId && m.UserId == userId)
            .OrderBy(m => m.Timestamp)
            .ToListAsync();
    }

    public async Task<ChatMessage> ProcessUserMessageAsync(int userId, string message, int? sessionId = null)
    {
        ChatSession? session = null;
        if (sessionId.HasValue)
        {
            session = await _context.ChatSessions.FindAsync(sessionId.Value);
            if (session != null && session.UserId != userId)
            {
                throw new UnauthorizedAccessException("User does not own this session.");
            }
        }

        // If no session provided or found, create one?
        // For now, let's assume if sessionId is null, we might be in a "legacy" mode or just creating a new one implicitly?
        // Better to require session creation for "complete flow".
        // But to support the existing "ProcessUserMessageAsync" signature (mostly), let's handle the null case.

        if (session == null && sessionId.HasValue)
        {
            // If specific ID requested but not found, maybe error?
            // Or just proceed without session (legacy behavior)?
            // Let's proceed with session if we can.
        }

        // Save user message
        var userMessage = new ChatMessage
        {
            UserId = userId,
            SessionId = sessionId,
            Role = ChatRoleType.User,
            Content = message
        };
        _context.ChatMessages.Add(userMessage);

        // Update session timestamp and title if it's the first message
        if (session != null)
        {
            session.UpdatedAt = DateTime.UtcNow;
            if (session.Messages.Count == 0 && session.Title == "New Chat")
            {
                // Generate title from message? For now just use first few chars
                session.Title = message.Length > 30 ? message[..30] + "..." : message;
            }
        }

        await _context.SaveChangesAsync();

        // Get AI response
        // We might want to pass history to OpenAI service here!
        // The current OpenAIService.GetChatCompletionAsync only takes a string.
        // We should probably improve OpenAIService to take history.
        // For now, let's stick to the interface.
        var aiResponseContent = await _openAIService.GetChatCompletionAsync(message);

        // Save AI response
        var aiMessage = new ChatMessage
        {
            UserId = userId,
            SessionId = sessionId,
            Role = ChatRoleType.Assistant,
            Content = aiResponseContent
        };
        _context.ChatMessages.Add(aiMessage);

        if (session != null)
        {
            session.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return aiMessage;
    }

    public async IAsyncEnumerable<string> ProcessUserMessageStreamingAsync(int userId, string message, int? sessionId = null)
    {
        ChatSession? session = null;
        if (sessionId.HasValue)
        {
            session = await _context.ChatSessions.Include(s => s.Messages).FirstOrDefaultAsync(s => s.Id == sessionId.Value);
            if (session != null && session.UserId != userId)
            {
                throw new UnauthorizedAccessException("User does not own this session.");
            }
        }

        // Save user message
        var userMessage = new ChatMessage
        {
            UserId = userId,
            SessionId = sessionId,
            Role = ChatRoleType.User,
            Content = message
        };
        _context.ChatMessages.Add(userMessage);

        if (session != null)
        {
            session.UpdatedAt = DateTime.UtcNow;
            if (session.Messages.Count == 0 && session.Title == "New Chat")
            {
                session.Title = message.Length > 30 ? message[..30] + "..." : message;
            }
        }
        await _context.SaveChangesAsync();

        var fullResponse = new System.Text.StringBuilder();

        // Console output for user request
        Console.WriteLine($"[AI Stream] User: {message}");
        Console.Write("[AI Stream] AI: ");

        await foreach (var chunk in _openAIService.GetChatStreamingAsync(message))
        {
            fullResponse.Append(chunk);
            Console.Write(chunk); // Output to terminal
            yield return chunk;
        }
        Console.WriteLine(); // End line in terminal

        // Save AI response
        var aiMessage = new ChatMessage
        {
            UserId = userId,
            SessionId = sessionId,
            Role = ChatRoleType.Assistant,
            Content = fullResponse.ToString()
        };
        _context.ChatMessages.Add(aiMessage);

        if (session != null)
        {
            session.UpdatedAt = DateTime.UtcNow;
        }
        await _context.SaveChangesAsync();
    }

    // Legacy/Global history support
    public async Task<IEnumerable<ChatMessage>> GetChatHistoryAsync(int userId)
    {
        return await _context.ChatMessages
            .Where(m => m.UserId == userId && m.SessionId == null) // Only return orphaned messages? Or all?
                                                                   // Let's return all for now to not break things, or maybe just those without session if we migrate fully.
                                                                   // The user asked for "complete chat flow", so we should probably move to sessions.
                                                                   // But for backward compatibility during dev:
            .OrderBy(m => m.Timestamp)
            .ToListAsync();
    }

    public async Task ClearChatHistoryAsync(int userId)
    {
        // Clear all messages? Or just sessions?
        // Let's clear everything for the user.
        var sessions = await _context.ChatSessions.Where(s => s.UserId == userId).ToListAsync();
        _context.ChatSessions.RemoveRange(sessions);

        var messages = await _context.ChatMessages.Where(m => m.UserId == userId && m.SessionId == null).ToListAsync();
        _context.ChatMessages.RemoveRange(messages);

        await _context.SaveChangesAsync();
    }
}
