using Microsoft.AspNetCore.Mvc;
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
                return BadRequest(new
                {
                    error = "Az email és kérdés mező nem lehet üres"
                });
            }

            var question = new Question
            {
                Text = questionDto.Text,
                Email = questionDto.Email
            };

            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "A kérdés sikeresen elmentve",
                id = question.Id
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Hiba a kérdés mentése során");
            return StatusCode(500, new
            {
                error = "Hiba történt a kérdés mentése közben"
            });
        }

        
    }
    [HttpDelete("{id}/DeleteQuestion")]
    public async Task<IActionResult> DeleteQuestion(int id)
        {
            try
            {
                // Find the question by ID
                var question = await _context.Questions.FindAsync(id);
                if (question == null)
                {
                    return NotFound(new { Success = false, Message = "Kérdés nem található" });
                }

                // Delete the question from the database
                _context.Questions.Remove(question);
                await _context.SaveChangesAsync();

                return Ok(new { Success = true, Message = "Kérdés sikeresen törölve" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting question with id {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Hiba történt");
            }
        }

}

public class QuestionDto
{
    public string Text { get; set; }
    public string Email {get; set; }
}
