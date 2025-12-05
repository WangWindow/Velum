namespace Velum.Core.Interfaces;

public interface IDashboardService
{
    Task<DashboardStats> GetStatsAsync();
    Task<DashboardChartData> GetChartDataAsync();
}

public class DashboardStats
{
    public int TotalUsers { get; set; }
    public int ActiveTasks { get; set; }
    public decimal Revenue { get; set; }
    public string SystemStatus { get; set; } = "Healthy";
    public IEnumerable<ActivityItem> Activities { get; set; } = [];
    public IEnumerable<RecentAssessmentItem> RecentAssessments { get; set; } = [];
}

public class ActivityItem
{
    public string Title { get; set; } = string.Empty;
    public DateTime Time { get; set; }
    public string Avatar { get; set; } = string.Empty;
}

public class RecentAssessmentItem
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string QuestionnaireTitle { get; set; } = string.Empty;
    public int Score { get; set; }
    public DateTime Date { get; set; }
    public string Result { get; set; } = string.Empty;
}

public class DashboardChartData
{
    public IEnumerable<object> Trend { get; set; } = [];
    public IEnumerable<object> Distribution { get; set; } = [];
}
