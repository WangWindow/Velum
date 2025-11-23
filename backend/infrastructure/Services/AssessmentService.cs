using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Velum.Core.Interfaces;
using Velum.Core.Models;
using Velum.Infrastructure.Data;

namespace Velum.Infrastructure.Services;

public class AssessmentService(ApplicationDbContext context) : IAssessmentService
{
    private readonly ApplicationDbContext _context = context;

    public async Task<IEnumerable<Assessment>> GetUserAssessmentsAsync(int userId)
    {
        return await _context.Assessments
            .Include(a => a.Questionnaire)
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.Date)
            .ToListAsync();
    }

    public async Task<Assessment> SubmitAssessmentAsync(int userId, int questionnaireId, Dictionary<string, object> answers)
    {
        // Calculate score (simplified logic)
        int score = 0;
        foreach (var val in answers.Values)
        {
            if (val is JsonElement element && element.ValueKind == JsonValueKind.Number)
            {
                score += element.GetInt32();
            }
            else if (int.TryParse(val.ToString(), out int s))
            {
                score += s;
            }
        }

        var assessment = new Assessment
        {
            UserId = userId,
            QuestionnaireId = questionnaireId,
            Date = DateTime.UtcNow,
            DetailsJson = JsonSerializer.Serialize(answers),
            Score = score,
            Result = score > 50 ? "High Risk" : (score > 20 ? "Moderate" : "Normal") // Simple dummy logic
        };

        _context.Assessments.Add(assessment);

        // Check if there is a pending task for this questionnaire and user
        var pendingTask = await _context.UserTasks
            .FirstOrDefaultAsync(t => t.UserId == userId && t.QuestionnaireId == questionnaireId && t.Status != "Completed");

        if (pendingTask != null)
        {
            pendingTask.Status = "Completed";
            assessment.UserTaskId = pendingTask.Id;
        }

        await _context.SaveChangesAsync();
        return assessment;
    }
}
