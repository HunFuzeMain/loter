using Microsoft.AspNetCore.Mvc;
using tempbackend.Models;
using VizsgaremekApp.Models;

[ApiController]
[Route("api/[controller]")]
public class QuestionsController : ControllerBase
{
    private readonly VizsgaremekContext _context;
    private readonly ILogger<QuestionsController> _logger;

    public QuestionsController(VizsgaremekContext context, ILogger<QuestionsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] QuestionDto questionDto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(questionDto.Text))
            {
                return BadRequest(new { error = "Question text cannot be empty" });
            }

            var question = new Question
            {
                Text = questionDto.Text
            };

            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Question saved successfully", id = question.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving question");
            return StatusCode(500, new { error = "An error occurred while saving your question" });
        }
    }
}

public class QuestionDto
{
    public string Text { get; set; }
}