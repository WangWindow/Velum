using System.ComponentModel.DataAnnotations;

namespace Velum.Core.Models;

public class GameScore
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }

    [Required]
    public string GameName { get; set; } = string.Empty;

    public int Score { get; set; }

    // Duration in seconds, optional depending on game
    public double Duration { get; set; }

    public DateTime PlayedAt { get; set; } = DateTime.UtcNow;
}
