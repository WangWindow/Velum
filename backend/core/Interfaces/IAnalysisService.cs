using System.Threading.Tasks;

namespace Velum.Core.Interfaces;

using Velum.Core.Models;

public interface IAnalysisService
{
    Task RunAnalysisAsync();
    Task<string?> AnalyzeAssessmentAsync(int assessmentId);
    Task<OverallStats> GetOverallStatsAsync();
    Task<UserAnalysisResult?> GetUserAnalysisAsync(int userId);
    Task<AssessmentExportData> GetAssessmentExportDataAsync(int questionnaireId);
}

public class AssessmentExportData
{
    public List<string> Columns { get; set; } = [];
    public List<Dictionary<string, object>> Rows { get; set; } = [];
}
