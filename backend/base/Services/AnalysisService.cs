using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Velum.Base.Data;
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
        var pendingAssessments = await _context.Assessments
            .Where(a => string.IsNullOrEmpty(a.AnalysisJson))
            .Include(a => a.Questionnaire)
            .Include(a => a.User)
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

    public async Task<OverallStats> GetOverallStatsAsync()
    {
        var totalAssessments = await _context.Assessments.CountAsync();
        var totalUsers = await _context.Users.CountAsync();

        var qStats = await _context.Assessments
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
                Result = a.Result
            })
            .ToListAsync();

        return new UserAnalysisResult
        {
            UserId = user.Id,
            Username = user.Username,
            History = history
        };
    }
}
