namespace Velum.Core.Models;

public class Questionnaire
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public required string QuestionsJson { get; set; } // JSON array of questions
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
