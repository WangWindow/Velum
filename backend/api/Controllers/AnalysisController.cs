using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Velum.Core.Interfaces;
using Velum.Core.Models;

namespace Velum.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AnalysisController(IAnalysisService analysisService) : ControllerBase
{
    [HttpPost("run")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> RunAnalysis()
    {
        await analysisService.RunAnalysisAsync();
        return Ok(new { message = "Analysis started." });
    }

    [HttpPost("analyze/{assessmentId}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> AnalyzeAssessment(int assessmentId)
    {
        var result = await analysisService.AnalyzeAssessmentAsync(assessmentId);
        if (result == null) return NotFound("Assessment not found or failed to analyze.");
        return Ok(new { analysis = result });
    }

    [HttpGet("stats")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<OverallStats>> GetOverallStats()
    {
        var stats = await analysisService.GetOverallStatsAsync();
        return Ok(stats);
    }

    [HttpGet("user/{userId}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<UserAnalysisResult>> GetUserAnalysis(int userId)
    {
        var result = await analysisService.GetUserAnalysisAsync(userId);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("export/{questionnaireId}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<AssessmentExportData>> GetAssessmentExportData(int questionnaireId)
    {
        var data = await analysisService.GetAssessmentExportDataAsync(questionnaireId);
        return Ok(data);
    }
}
