using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Velum.Core.Interfaces;

namespace Velum.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "admin")]
public class DashboardController(IDashboardService dashboardService) : ControllerBase
{
    private readonly IDashboardService _dashboardService = dashboardService;

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var stats = await _dashboardService.GetStatsAsync();
        return Ok(stats);
    }

    [HttpGet("chart-data")]
    public async Task<IActionResult> GetChartData()
    {
        var data = await _dashboardService.GetChartDataAsync();
        return Ok(data);
    }
}
