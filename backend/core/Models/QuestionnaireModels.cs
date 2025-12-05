using Velum.Core.Enums;

namespace Velum.Core.Models;

public class QuestionnaireTemplate
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? InterpretationGuide { get; set; } // Guide for AI to interpret scores
    public List<Question> Questions { get; set; } = [];
}

public class Question
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public QuestionType Type { get; set; } = QuestionType.SingleChoice;
    public List<Option> Options { get; set; } = [];
}

public class Option
{
    public string Text { get; set; } = string.Empty;
    public int Score { get; set; }
}

public class AssessmentSubmission
{
    public int QuestionnaireId { get; set; }
    public int? UserTaskId { get; set; }
    // Key: QuestionId, Value: Answer (Index for SingleChoice, Array of Indices for MultipleChoice, String for Text, Value for Scale)
    public Dictionary<string, object> Answers { get; set; } = [];
}

public class AssessmentResult
{
    public int TotalScore { get; set; }
    public string AIAnalysis { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
}

public class ParseRequest
{
    public string Text { get; set; } = string.Empty;
}

public class BilingualQuestionnaireTemplate
{
    public QuestionnaireTemplate En { get; set; } = new();
    public QuestionnaireTemplate Zh { get; set; } = new();
}

public class OverallStats
{
    public int TotalAssessments { get; set; }
    public int TotalUsers { get; set; }
    public List<QuestionnaireStat> QuestionnaireStats { get; set; } = [];
}

public class QuestionnaireStat
{
    public int QuestionnaireId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Count { get; set; }
    public double AverageScore { get; set; }
}

public class UserAnalysisResult
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public List<AssessmentSummary> History { get; set; } = [];
}

public class AssessmentSummary
{
    public int Id { get; set; }
    public string QuestionnaireTitle { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int Score { get; set; }
    public string? Result { get; set; }
    public string? AnalysisJson { get; set; }
}
