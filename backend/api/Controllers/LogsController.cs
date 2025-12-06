using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Velum.Core.Interfaces;
using Velum.Core.Models;

namespace Velum.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "admin")]
public class LogsController(ILogService logService) : ControllerBase
{
    private readonly ILogService _logService = logService;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SystemLog>>> GetLogs(
        [FromQuery] int count = 100,
        [FromQuery] int skip = 0,
        [FromQuery] string? level = null,
        [FromQuery] string? search = null)
    {
        var logs = await _logService.GetLogsAsync(count, skip, level, search);
        return Ok(logs);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteLog(int id)
    {
        await _logService.DeleteLogAsync(id);
        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteLogs([FromBody] List<int> ids)
    {
        await _logService.DeleteLogsAsync(ids);
        return NoContent();
    }
}
