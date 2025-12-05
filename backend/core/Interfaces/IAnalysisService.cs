using System.Threading.Tasks;

namespace Velum.Core.Interfaces;

using Velum.Core.Models;

public interface IAnalysisService
{
    Task RunAnalysisAsync();
    Task<OverallStats> GetOverallStatsAsync();
    Task<UserAnalysisResult?> GetUserAnalysisAsync(int userId);
}
