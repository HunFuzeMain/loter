using System.ComponentModel.DataAnnotations;
using VizsgaremekApp.Models;

public class InstructorStatusUpdateDto
{
    [Required]
    public InstructorStatus NewStatus { get; set; }
    public string Notes { get; set; }
}