using System.ComponentModel.DataAnnotations;

public class Instructor
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Phone { get; set; } = string.Empty;

    [Required]
    public string Address { get; set; } = string.Empty;

    // File paths
    [Required]
    public string QualificationFileName { get; set; } = string.Empty;
    [Required]
    public string IdCardFileName { get; set; } = string.Empty;
    [Required]
    public string CVFileName { get; set; } = string.Empty;

    // Status tracking
    public DateTime ApplicationDate { get; set; } = DateTime.UtcNow;
    public DateTime? HireDate { get; set; }
    public DateTime? TerminationDate { get; set; }
    public bool IsActive { get; set; } = false;
    public InstructorStatus Status { get; set; } = InstructorStatus.Pending;
    public string? Notes { get; set; }
}
public enum InstructorStatus
{
    Pending,        // Default status for new applications
    UnderReview,    // Application is being reviewed
    Approved,       // Application approved but not hired yet
    Hired,          // Currently working
    Rejected,       // Application rejected
    OnLeave,        // Temporary leave
    Terminated      // No longer works here
}


