using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Velum.Core.Interfaces;
using Velum.Core.Models;

namespace Velum.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AssessmentsController(IAssessmentService assessmentService) : ControllerBase
{
    private readonly IAssessmentService _assessmentService = assessmentService;

    [HttpGet("my")]
    public async Task<ActionResult<IEnumerable<Assessment>>> GetMyAssessments()
    {
        var userIdClaim = User.FindFirst("UserId");
        if (userIdClaim == null) return Unauthorized();
        var userId = int.Parse(userIdClaim.Value);

        var assessments = await _assessmentService.GetUserAssessmentsAsync(userId);
        return Ok(assessments);
    }

    [HttpPost]
    public async Task<ActionResult<Assessment>> SubmitAssessment([FromBody] SubmitAssessmentRequest request)
    {
        var userIdClaim = User.FindFirst("UserId");
        if (userIdClaim == null) return Unauthorized();
        var userId = int.Parse(userIdClaim.Value);

        var assessment = await _assessmentService.SubmitAssessmentAsync(userId, request.QuestionnaireId, request.Answers);
        return Ok(assessment);
    }
}

public class SubmitAssessmentRequest
{
    public int QuestionnaireId { get; set; }
    public Dictionary<string, object> Answers { get; set; } = [];
}
