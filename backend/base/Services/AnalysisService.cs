using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Velum.Base.Data;
using Velum.Core.Enums;
using Velum.Core.Interfaces;
using Velum.Core.Models;

namespace Velum.Base.Services;

public class AnalysisService(IServiceProvider serviceProvider, ApplicationDbContext context) : IAnalysisService
{
    private readonly ApplicationDbContext _context = context;

    public async Task RunAnalysisAsync()
    {
        using var scope = serviceProvider.CreateScope();
        var openAIService = scope.ServiceProvider.GetRequiredService<IOpenAIService>();

        // Example: Analyze recent assessments that haven't been analyzed yet
        // Only analyze assessments from regular users, not admins
        var pendingAssessments = await _context.Assessments
            .Include(a => a.User)
            .Where(a => string.IsNullOrEmpty(a.AnalysisJson) && a.User != null && a.User.Role == UserRoleType.User)
            .Include(a => a.Questionnaire)
            .Take(5) // Process in batches
            .ToListAsync();

        foreach (var assessment in pendingAssessments)
        {
            if (assessment.Questionnaire == null || assessment.User == null) continue;

            var prompt = $"""
                Analyze the following psychological assessment result:
                User: {assessment.User.Username}
                Scale: {assessment.Questionnaire.Title}
                Score: {assessment.Score}
                Result Summary: {assessment.Result}

                Provide a brief psychological interpretation and recommendation.
                """;

            var analysis = await openAIService.GetChatCompletionAsync(prompt);
            assessment.AnalysisJson = analysis;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<string?> AnalyzeAssessmentAsync(int assessmentId)
    {
        using var scope = serviceProvider.CreateScope();
        var openAIService = scope.ServiceProvider.GetRequiredService<IOpenAIService>();

        var assessment = await _context.Assessments
            .Include(a => a.Questionnaire)
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Id == assessmentId);

        if (assessment == null || assessment.Questionnaire == null || assessment.User == null)
            return null;

        var prompt = $"""
            Analyze the following psychological assessment result:
            User: {assessment.User.Username}
            Scale: {assessment.Questionnaire.Title}
            Score: {assessment.Score}
            Result Summary: {assessment.Result}
            Interpretation Guide: {assessment.Questionnaire.InterpretationGuide ?? "N/A"}

            Provide a detailed psychological interpretation and recommendation.
            """;

        var analysis = await openAIService.GetChatCompletionAsync(prompt);
        assessment.AnalysisJson = analysis;
        await _context.SaveChangesAsync();

        return analysis;
    }

    public async Task<OverallStats> GetOverallStatsAsync()
    {
        // Filter stats to only include regular users
        var totalAssessments = await _context.Assessments
            .Include(a => a.User)
            .Where(a => a.User != null && a.User.Role == UserRoleType.User)
            .CountAsync();

        var totalUsers = await _context.Users
            .Where(u => u.Role == UserRoleType.User)
            .CountAsync();

        var qStats = await _context.Assessments
            .Include(a => a.User)
            .Where(a => a.User != null && a.User.Role == UserRoleType.User)
            .Include(a => a.Questionnaire)
            .GroupBy(a => a.QuestionnaireId)
            .Select(g => new QuestionnaireStat
            {
                QuestionnaireId = g.Key ?? 0,
                Title = g.First().Questionnaire != null ? g.First().Questionnaire!.Title : "Unknown",
                Count = g.Count(),
                AverageScore = g.Average(a => a.Score)
            })
            .ToListAsync();

        return new OverallStats
        {
            TotalAssessments = totalAssessments,
            TotalUsers = totalUsers,
            QuestionnaireStats = qStats
        };
    }

    public async Task<UserAnalysisResult?> GetUserAnalysisAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return null;

        var history = await _context.Assessments
            .Where(a => a.UserId == userId)
            .Include(a => a.Questionnaire)
            .OrderByDescending(a => a.Date)
            .Select(a => new AssessmentSummary
            {
                Id = a.Id,
                QuestionnaireTitle = a.Questionnaire != null ? a.Questionnaire.Title : "Unknown",
                Date = a.Date,
                Score = a.Score,
                Result = a.Result,
                AnalysisJson = a.AnalysisJson
            })
            .ToListAsync();

        return new UserAnalysisResult
        {
            UserId = user.Id,
            Username = user.Username,
            History = history
        };
    }

    public async Task<AssessmentExportData> GetAssessmentExportDataAsync(int questionnaireId)
    {
        var questionnaire = await _context.Questionnaires.FindAsync(questionnaireId);
        if (questionnaire == null) return new AssessmentExportData();

        var assessments = await _context.Assessments
            .Include(a => a.User)
            .Where(a => a.QuestionnaireId == questionnaireId && a.User != null && a.User.Role == UserRoleType.User)
            .OrderByDescending(a => a.Date)
            .ToListAsync();

        // Parse questions to get columns
        var columns = new List<string> { "User", "Date", "Score", "Result" };
        var questions = new List<dynamic>();
        try
        {
            questions = JsonSerializer.Deserialize<List<dynamic>>(questionnaire.QuestionsJson) ?? [];
            foreach (var q in questions)
            {
                // Assuming question object has "text" property
                // We use JsonElement if dynamic deserialization results in JsonElement
                if (q is JsonElement element)
                {
                    if (element.TryGetProperty("text", out var textProp))
                    {
                        columns.Add(textProp.GetString() ?? "Question");
                    }
                }
            }
        }
        catch { /* Ignore parsing errors */ }

        var rows = new List<Dictionary<string, object>>();

        foreach (var assessment in assessments)
        {
            var row = new Dictionary<string, object>
            {
                { "User", assessment.User!.Username },
                { "Date", assessment.Date.ToString("yyyy-MM-dd HH:mm") },
                { "Score", assessment.Score },
                { "Result", assessment.Result ?? "" }
            };

            // Parse answers
            if (!string.IsNullOrEmpty(assessment.DetailsJson))
            {
                try
                {
                    // DetailsJson is likely { questionId: answer }
                    // But we need to map it to columns.
                    // If we don't have a reliable mapping, we might just list them as Q1, Q2...
                    // For now, let's assume we can map by index if the order is preserved,
                    // or we need to know the question IDs.

                    // Let's try to parse as Dictionary<string, object>
                    var answers = JsonSerializer.Deserialize<Dictionary<string, object>>(assessment.DetailsJson);

                    if (answers != null)
                    {
                        // This part is tricky without a strict schema.
                        // If the frontend sends { "1": "Option A", "2": "Option B" } where keys are Question IDs.
                        // And QuestionsJson has IDs.

                        int qIndex = 0;
                        foreach (var q in questions)
                        {
                            if (q is JsonElement element && element.TryGetProperty("id", out var idProp))
                            {
                                var qId = idProp.ToString();
                                if (answers.TryGetValue(qId, out var ans))
                                {
                                    // Get the column name corresponding to this question
                                    // We added columns in order, so we can skip the first 4 fixed columns
                                    if (4 + qIndex < columns.Count)
                                    {
                                        row[columns[4 + qIndex]] = ans.ToString() ?? "";
                                    }
                                }
                            }
                            qIndex++;
                        }
                    }
                }
                catch { }
            }

            rows.Add(row);
        }

        return new AssessmentExportData
        {
            Columns = columns,
            Rows = rows
        };
    }
}
