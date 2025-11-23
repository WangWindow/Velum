using Velum.Core.Enums;

namespace Velum.Core.Models;

public class User
{
    public int Id { get; set; }
    public required string Username { get; set; }
    [System.Text.Json.Serialization.JsonIgnore]
    public string PasswordHash { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public UserRoleType Role { get; set; } = UserRoleType.User;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLogin { get; set; }
}
