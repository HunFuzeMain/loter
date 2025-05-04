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

        [HttpGet]
        public IActionResult GetAll()
        {
            var questions = _context.Questions
                            .Select(q => new {q.Id, q.Email, q.Text})
                            .ToList();
            return Ok(questions);
        }
    }

}

public class QuestionDto
{
    public string Text { get; set; }
    public string Email {get; set; }
}
