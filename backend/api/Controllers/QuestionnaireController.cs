using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Velum.Base.Data;
using Velum.Core.Interfaces;
using Velum.Core.Models;

namespace Velum.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class QuestionnaireController(ApplicationDbContext context, IOpenAIService openAIService) : ControllerBase
{
    private readonly ApplicationDbContext _context = context;
    private readonly IOpenAIService _openAIService = openAIService;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

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
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<QuestionnaireTemplate>> ParseQuestionnaire([FromBody] ParseRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Text))
        {
            return BadRequest("Text is required.");
        }

        try
        {
            var jsonResult = await _openAIService.ParseQuestionnaireFromTextAsync(request.Text);
            var questionnaireTemplate = JsonSerializer.Deserialize<QuestionnaireTemplate>(jsonResult, _jsonOptions);
            return Ok(questionnaireTemplate);
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to parse questionnaire: {ex.Message}");
        }
    }

    [HttpPost("parse-bilingual")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<BilingualQuestionnaireTemplate>> ParseBilingualQuestionnaire([FromBody] ParseRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Text))
        {
            return BadRequest("Text is required.");
        }

        try
        {
            var jsonResult = await _openAIService.ParseAndTranslateQuestionnaireAsync(request.Text);
            var template = JsonSerializer.Deserialize<BilingualQuestionnaireTemplate>(jsonResult, _jsonOptions);
            return Ok(template);
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to parse bilingual questionnaire: {ex.Message}");
        }
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<Questionnaire>> CreateQuestionnaire(QuestionnaireTemplate template)
    {
        var questionnaire = new Questionnaire
        {
            Title = template.Title,
            Description = template.Description,
            InterpretationGuide = template.InterpretationGuide,
            QuestionsJson = JsonSerializer.Serialize(template.Questions, _jsonOptions),
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Questionnaires.Add(questionnaire);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetQuestionnaire), new { id = questionnaire.Id }, questionnaire);
    }

    [HttpPost("bilingual")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> CreateBilingualQuestionnaire(BilingualQuestionnaireTemplate template)
    {
        var groupId = Guid.NewGuid();

        var enQuestionnaire = new Questionnaire
        {
            Title = template.En.Title,
            Description = template.En.Description,
            InterpretationGuide = template.En.InterpretationGuide,
            QuestionsJson = JsonSerializer.Serialize(template.En.Questions, _jsonOptions),
            Language = "en",
            GroupId = groupId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var zhQuestionnaire = new Questionnaire
        {
            Title = template.Zh.Title,
            Description = template.Zh.Description,
            InterpretationGuide = template.Zh.InterpretationGuide,
            QuestionsJson = JsonSerializer.Serialize(template.Zh.Questions, _jsonOptions),
            Language = "zh",
            GroupId = groupId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Questionnaires.AddRange(enQuestionnaire, zhQuestionnaire);
        await _context.SaveChangesAsync();

        return Ok(new { GroupId = groupId, EnId = enQuestionnaire.Id, ZhId = zhQuestionnaire.Id });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> UpdateQuestionnaire(int id, QuestionnaireTemplate template)
    {
        var questionnaire = await _context.Questionnaires.FindAsync(id);
        if (questionnaire == null)
        {
            return NotFound();
        }

        questionnaire.Title = template.Title;
        questionnaire.Description = template.Description;
        questionnaire.QuestionsJson = JsonSerializer.Serialize(template.Questions, _jsonOptions);
        // Keep CreatedAt and IsActive as is, or update if needed

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
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

    [HttpPost("parse-file")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<QuestionnaireTemplate>> ParseQuestionnaireFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("File is empty or not provided.");
        }

        // Basic file type check (optional, but good practice)
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension != ".txt" && extension != ".md" && extension != ".json")
        {
            return BadRequest("Only .txt, .md, or .json files are supported.");
        }

        string text;
        using (var reader = new StreamReader(file.OpenReadStream()))
        {
            text = await reader.ReadToEndAsync();
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            return BadRequest("File content is empty.");
        }

        try
        {
            var jsonResult = await _openAIService.ParseQuestionnaireFromTextAsync(text);
            var questionnaireTemplate = JsonSerializer.Deserialize<QuestionnaireTemplate>(jsonResult, _jsonOptions);

            // Basic Format Validation
            if (questionnaireTemplate == null)
            {
                return BadRequest("Failed to deserialize AI response into a valid template.");
            }

            if (string.IsNullOrWhiteSpace(questionnaireTemplate.Title))
            {
                questionnaireTemplate.Title = Path.GetFileNameWithoutExtension(file.FileName); // Fallback title
            }

            if (questionnaireTemplate.Questions == null || questionnaireTemplate.Questions.Count == 0)
            {
                return BadRequest("The parsed result contains no questions. Please check the file content.");
            }

            return Ok(questionnaireTemplate);
        }
        catch (JsonException jsonEx)
        {
            return BadRequest($"AI output was not valid JSON: {jsonEx.Message}");
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to parse questionnaire file: {ex.Message}");
        }
    }
}
