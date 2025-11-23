using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Velum.Core.Interfaces;
using Velum.Core.Models;
using Velum.Infrastructure.Data;

namespace Velum.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class QuestionnaireController(ApplicationDbContext context, IOpenAIService openAIService) : ControllerBase
{
    private readonly ApplicationDbContext _context = context;
    private readonly IOpenAIService _openAIService = openAIService;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Questionnaire>>> GetQuestionnaires()
    {
        return await _context.Questionnaires.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Questionnaire>> GetQuestionnaire(int id)
    {
        var questionnaire = await _context.Questionnaires.FindAsync(id);

        if (questionnaire == null)
        {
            return NotFound();
        }

        return questionnaire;
    }

    [HttpPost("parse")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<QuestionnaireTemplate>> ParseQuestionnaire([FromBody] ParseRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Text))
        {
            return BadRequest("Text is required.");
        }

        try
        {
            var jsonResult = await _openAIService.ParseQuestionnaireFromTextAsync(request.Text);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var questionnaireTemplate = JsonSerializer.Deserialize<QuestionnaireTemplate>(jsonResult, options);
            return Ok(questionnaireTemplate);
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to parse questionnaire: {ex.Message}");
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Questionnaire>> CreateQuestionnaire(QuestionnaireTemplate template)
    {
        var questionnaire = new Questionnaire
        {
            Title = template.Title,
            Description = template.Description,
            QuestionsJson = JsonSerializer.Serialize(template.Questions),
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Questionnaires.Add(questionnaire);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetQuestionnaire), new { id = questionnaire.Id }, questionnaire);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateQuestionnaire(int id, QuestionnaireTemplate template)
    {
        var questionnaire = await _context.Questionnaires.FindAsync(id);
        if (questionnaire == null)
        {
            return NotFound();
        }

        questionnaire.Title = template.Title;
        questionnaire.Description = template.Description;
        questionnaire.QuestionsJson = JsonSerializer.Serialize(template.Questions);
        // Keep CreatedAt and IsActive as is, or update if needed

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteQuestionnaire(int id)
    {
        var questionnaire = await _context.Questionnaires.FindAsync(id);
        if (questionnaire == null)
        {
            return NotFound();
        }

        _context.Questionnaires.Remove(questionnaire);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
