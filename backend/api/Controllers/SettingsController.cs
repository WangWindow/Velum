using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Velum.Base.Data;
using Velum.Core.Models;

namespace Velum.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "admin")]
public class SettingsController(ApplicationDbContext context) : ControllerBase
{
    private readonly ApplicationDbContext _context = context;

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
        return Ok();
    }
}
