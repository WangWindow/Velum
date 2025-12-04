using Velum.Core.Models;

namespace Velum.Core.Interfaces;

public interface ITaskService
{
    Task<IEnumerable<UserTask>> GetAllTasksAsync();
    Task<IEnumerable<UserTask>> GetUserTasksAsync(int userId);
    Task<UserTask> AssignTaskAsync(int userId, int questionnaireId, DateTime? dueDate);
    Task<bool> DeleteTaskAsync(int id);
}
