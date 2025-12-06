using System.ComponentModel.DataAnnotations;

namespace Velum.Core.Models;

public class SystemLog
{
    public int Id { get; set; }
    public string Level { get; set; } = "Info"; // Info, Warning, Error
    public string Message { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public User? User { get; set; }
    public string? UserName { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? Action { get; set; } // e.g., "Login", "UpdateSettings"
    public string? Resource { get; set; } // e.g., "User:123", "Settings"
    public string? IpAddress { get; set; }
}
