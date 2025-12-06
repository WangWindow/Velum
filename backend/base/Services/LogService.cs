using Microsoft.EntityFrameworkCore;
using Velum.Base.Data;
using Velum.Core.Interfaces;
using Velum.Core.Models;

namespace Velum.Base.Services;

public class LogService(ApplicationDbContext context) : ILogService
{
    private readonly ApplicationDbContext _context = context;

    public async Task LogAsync(string level, string message, int? userId = null, string? userName = null, string? action = null, string? resource = null, string? ipAddress = null)
    {
        var log = new SystemLog
        {
            Level = level,
            Message = message,
            UserId = userId,
            UserName = userName,
            Action = action,
            Resource = resource,
            IpAddress = ipAddress,
            Timestamp = DateTime.UtcNow
        };

        _context.SystemLogs.Add(log);
        await _context.SaveChangesAsync();
    }

    public Task LogInfoAsync(string message, int? userId = null, string? userName = null, string? action = null, string? resource = null)
    {
        return LogAsync("Info", message, userId, userName, action, resource);
    }

    public Task LogWarningAsync(string message, int? userId = null, string? userName = null, string? action = null, string? resource = null)
    {
        return LogAsync("Warning", message, userId, userName, action, resource);
    }

    public Task LogErrorAsync(string message, int? userId = null, string? userName = null, string? action = null, string? resource = null)
    {
        return LogAsync("Error", message, userId, userName, action, resource);
    }

    public async Task<IEnumerable<SystemLog>> GetLogsAsync(int count = 100, int skip = 0, string? level = null, string? search = null)
    {
        var query = _context.SystemLogs.AsQueryable();

        if (!string.IsNullOrEmpty(level))
        {
            query = query.Where(l => l.Level == level);
        }

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(l => l.Message.Contains(search) ||
                                     (l.Action != null && l.Action.Contains(search)) ||
                                     (l.Resource != null && l.Resource.Contains(search)) ||
                                     (l.UserName != null && l.UserName.Contains(search)));
        }

        return await query
            .OrderByDescending(l => l.Timestamp)
            .Skip(skip)
            .Take(count)
            .ToListAsync();
    }

    public async Task DeleteLogAsync(int id)
    {
        var log = await _context.SystemLogs.FindAsync(id);
        if (log != null)
        {
            _context.SystemLogs.Remove(log);
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteLogsAsync(IEnumerable<int> ids)
    {
        var logs = await _context.SystemLogs.Where(l => ids.Contains(l.Id)).ToListAsync();
        if (logs.Any())
        {
            _context.SystemLogs.RemoveRange(logs);
            await _context.SaveChangesAsync();
        }
    }
}
