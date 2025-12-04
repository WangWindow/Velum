using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Velum.Core.Interfaces;
using Velum.Core.Models;

namespace Velum.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TasksController(ITaskService taskService) : ControllerBase
{
    private readonly ITaskService _taskService = taskService;

    // Admin: Get all tasks
    [HttpGet]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<IEnumerable<UserTask>>> GetTasks()
    {
        var tasks = await _taskService.GetAllTasksAsync();
        return Ok(tasks);
    }

    // User: Get my tasks
    [HttpGet("my")]
    public async Task<ActionResult<IEnumerable<UserTask>>> GetMyTasks()
    {
        var userIdClaim = User.FindFirst("UserId");
        if (userIdClaim == null) return Unauthorized();

        var userId = int.Parse(userIdClaim.Value);

        var tasks = await _taskService.GetUserTasksAsync(userId);
        return Ok(tasks);
    }

    // Admin: Assign task
    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<UserTask>> AssignTask([FromBody] AssignTaskRequest request)
    {
        var task = await _taskService.AssignTaskAsync(request.UserId, request.QuestionnaireId, request.DueDate);
        return CreatedAtAction(nameof(GetTasks), new { id = task.Id }, task);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        var success = await _taskService.DeleteTaskAsync(id);
        if (!success) return NotFound();
        return NoContent();
    }
}


public class AssignTaskRequest
{
    public int UserId { get; set; }
    public int QuestionnaireId { get; set; }
    public DateTime? DueDate { get; set; }
}
