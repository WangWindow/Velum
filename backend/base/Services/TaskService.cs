using Microsoft.EntityFrameworkCore;
using Velum.Base.Data;
using Velum.Core.Interfaces;
using Velum.Core.Models;

namespace Velum.Base.Services;

public class TaskService(ApplicationDbContext context) : ITaskService
{
    private readonly ApplicationDbContext _context = context;

    public async Task<IEnumerable<UserTask>> GetAllTasksAsync()
    {
        return await _context.UserTasks
            .Include(t => t.User)
            .Include(t => t.Questionnaire)
            .OrderByDescending(t => t.AssignedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserTask>> GetUserTasksAsync(int userId)
    {
        return await _context.UserTasks
            .Include(t => t.Questionnaire)
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.AssignedAt)
            .ToListAsync();
    }

    public async Task<UserTask> AssignTaskAsync(int userId, int questionnaireId, DateTime? dueDate)
    {
        var task = new UserTask
        {
            UserId = userId,
            QuestionnaireId = questionnaireId,
            DueDate = dueDate,
            Status = "Pending",
            AssignedAt = DateTime.UtcNow
        };

        _context.UserTasks.Add(task);
        await _context.SaveChangesAsync();

        // Reload to get navigation properties
        await _context.Entry(task).Reference(t => t.User).LoadAsync();
        await _context.Entry(task).Reference(t => t.Questionnaire).LoadAsync();

        return task;
    }

    public async Task<bool> DeleteTaskAsync(int id)
    {
        var task = await _context.UserTasks.FindAsync(id);
        if (task == null) return false;

        _context.UserTasks.Remove(task);
        await _context.SaveChangesAsync();
        return true;
    }
}
