﻿namespace VizsgaremekApp.Models;

public class Question
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public string Email {get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
