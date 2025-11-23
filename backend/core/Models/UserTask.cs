namespace Velum.Core.Models;

public class UserTask
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public int QuestionnaireId { get; set; }
    public Questionnaire? Questionnaire { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DueDate { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Completed, Overdue
}
