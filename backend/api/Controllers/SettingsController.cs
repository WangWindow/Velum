using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Velum.Base.Data;
using Velum.Core.Interfaces;
using Velum.Core.Models;

namespace Velum.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "admin")]
public class SettingsController(ApplicationDbContext context, ILogService logService) : ControllerBase
{
    private readonly ApplicationDbContext _context = context;
    private readonly ILogService _logService = logService;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AppSetting>>> GetSettings()
    {
        return await _context.AppSettings.ToListAsync();
    }

    [HttpPost]
    public async Task<IActionResult> UpdateSettings(List<AppSetting> settings)
    {
        foreach (var setting in settings)
        {
            var existing = await _context.AppSettings.FindAsync(setting.Key);
            if (existing != null)
            {
                existing.Value = setting.Value;
            }
            else
            {
                _context.AppSettings.Add(setting);
            }
        }

        await _context.SaveChangesAsync();

        await _logService.LogInfoAsync(
            message: "System settings updated",
            userId: GetCurrentUserId(),
            action: "UpdateSettings",
            resource: "Settings"
        );

        return Ok();
    }

    [HttpDelete]
    public async Task<IActionResult> ResetSettings()
    {
        var settings = await _context.AppSettings.ToListAsync();
        _context.AppSettings.RemoveRange(settings);
        await _context.SaveChangesAsync();

        await _logService.LogWarningAsync(
            message: "System settings reset to default",
            userId: GetCurrentUserId(),
            action: "ResetSettings",
            resource: "Settings"
        );

        return Ok();
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("UserId");
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
        {
            return userId;
        }
        return null;
    }
}
