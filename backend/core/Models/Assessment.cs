namespace Velum.Core.Models;

public class Assessment
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public int? QuestionnaireId { get; set; }
    public Questionnaire? Questionnaire { get; set; }
    public int? UserTaskId { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public int Score { get; set; }
    public string? Result { get; set; }
    public string? DetailsJson { get; set; } // Store detailed answers as JSON
    public string? AnalysisJson { get; set; } // AI Analysis result
}
