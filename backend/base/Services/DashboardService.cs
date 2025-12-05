using Microsoft.EntityFrameworkCore;
using Velum.Base.Data;
using Velum.Core.Interfaces;

namespace Velum.Base.Services;

public class DashboardService(ApplicationDbContext context) : IDashboardService
{
    private readonly ApplicationDbContext _context = context;

    public async Task<DashboardStats> GetStatsAsync()
    {
        var totalUsers = await _context.Users.CountAsync();
        var activeTasks = await _context.UserTasks.CountAsync(t => t.Status == "Pending");

        var recentActivity = await _context.UserTasks
            .Include(t => t.User)
            .OrderByDescending(t => t.AssignedAt)
            .Take(5)
            .Select(t => new ActivityItem
            {
                Title = $"Task assigned to {t.User!.Username}",
                Time = t.AssignedAt,
                Avatar = ""
            })
            .ToListAsync();

        var recentAssessments = await _context.Assessments
            .Include(a => a.User)
            .Include(a => a.Questionnaire)
            .OrderByDescending(a => a.Date)
            .Take(5)
            .Select(a => new RecentAssessmentItem
            {
                Id = a.Id,
                UserName = a.User!.Username,
                QuestionnaireTitle = a.Questionnaire!.Title,
                Score = a.Score,
                Date = a.Date,
                Result = a.Result ?? "N/A"
            })
            .ToListAsync();

        return new DashboardStats
        {
            TotalUsers = totalUsers,
            ActiveTasks = activeTasks,
            Revenue = 0,
            SystemStatus = "Healthy",
            Activities = recentActivity,
            RecentAssessments = recentAssessments
        };
    }

    public async Task<DashboardChartData> GetChartDataAsync()
    {
        // 1. Assessments over the last 7 days
        var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
        var assessments = await _context.Assessments
            .Where(a => a.Date >= sevenDaysAgo)
            .GroupBy(a => a.Date.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync();

        // Fill in missing days
        var trendData = Enumerable.Range(0, 7)
            .Select(offset => sevenDaysAgo.AddDays(offset).Date)
            .Select(date => new
            {
                Date = date.ToString("MM-dd"),
                Count = assessments.FirstOrDefault(a => a.Date == date)?.Count ?? 0
            })
            .ToList();

        // 2. Task Status Distribution
        var taskDistribution = await _context.UserTasks
            .GroupBy(t => t.Status)
            .Select(g => new { Name = g.Key, Value = g.Count() })
            .ToListAsync();

        return new DashboardChartData
        {
            Trend = trendData,
            Distribution = taskDistribution
        };
    }
}
