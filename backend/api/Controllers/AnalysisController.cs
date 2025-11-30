using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Velum.Core.Interfaces;

namespace Velum.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AnalysisController(IAnalysisService analysisService) : ControllerBase
{
    [HttpPost("run")]
    public async Task<IActionResult> RunAnalysis()
    {
        await analysisService.RunAnalysisAsync();
        return Ok(new { message = "Analysis started." });
    }
}
