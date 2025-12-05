using Velum.Core.Models;

namespace Velum.Core.Interfaces;

public interface IChatService
{
    Task<IEnumerable<ChatSession>> GetUserSessionsAsync(int userId);
    Task<ChatSession> CreateSessionAsync(int userId, string? title = null);
    Task<ChatSession?> GetSessionAsync(int sessionId, int userId);
    Task<ChatSession?> UpdateSessionAsync(int sessionId, int userId, string title);
    Task DeleteSessionAsync(int sessionId, int userId);
    Task<IEnumerable<ChatMessage>> GetSessionMessagesAsync(int sessionId, int userId);
    Task<ChatMessage> ProcessUserMessageAsync(int userId, string message, int? sessionId = null);
    IAsyncEnumerable<string> ProcessUserMessageStreamingAsync(int userId, string message, int? sessionId = null);
    Task<IEnumerable<ChatMessage>> GetChatHistoryAsync(int userId);
    Task ClearChatHistoryAsync(int userId); // Keep for backward compatibility or clear all
}
