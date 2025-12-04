using Velum.Core.Enums;

namespace Velum.Core.Models;

public class ChatMessage
{
    public int Id { get; set; }
    public int? SessionId { get; set; }
    public ChatSession? Session { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public ChatRoleType Role { get; set; } = ChatRoleType.User;
    public required string Content { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
