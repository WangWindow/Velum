using Velum.Core.Models;

namespace Velum.Core.Interfaces;

public interface ILogService
{
    Task LogAsync(string level, string message, int? userId = null, string? userName = null, string? action = null, string? resource = null, string? ipAddress = null);
    Task LogInfoAsync(string message, int? userId = null, string? userName = null, string? action = null, string? resource = null);
    Task LogWarningAsync(string message, int? userId = null, string? userName = null, string? action = null, string? resource = null);
    Task LogErrorAsync(string message, int? userId = null, string? userName = null, string? action = null, string? resource = null);
    Task<IEnumerable<SystemLog>> GetLogsAsync(int count = 100, int skip = 0, string? level = null, string? search = null);
    Task DeleteLogAsync(int id);
    Task DeleteLogsAsync(IEnumerable<int> ids);
}
