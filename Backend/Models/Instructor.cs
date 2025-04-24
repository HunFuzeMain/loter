using System.ComponentModel.DataAnnotations;
using VizsgaremekApp.Models;

// Add to your Instructor model (Models/Instructor.cs)
public class Instructor
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Address { get; set; }
    public string Password { get; set; }
    public string QualificationFileName { get; set; }
    public string IdCardFileName { get; set; }
    public string CVFileName { get; set; }
    public InstructorStatus Status { get; set; }
    public DateTime ApplicationDate { get; set; }
    public DateTime? HireDate { get; set; }
    public bool IsActive { get; set; }

    // Add navigation property
    public virtual ICollection<Appointment> Appointments { get; set; }
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


