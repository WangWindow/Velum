using Velum.Core.Models;

namespace Velum.Core.Interfaces;

public interface IChatService
{
    Task<IEnumerable<ChatMessage>> GetChatHistoryAsync(int userId);
    Task<ChatMessage> ProcessUserMessageAsync(int userId, string message);
}
