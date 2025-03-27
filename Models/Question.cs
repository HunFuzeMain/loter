namespace tempbackend.Models
{
    public class Question
    {
            public int Id { get; set; }
            public string Text { get; set; }
            public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
