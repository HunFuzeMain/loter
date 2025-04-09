using System;
using System.ComponentModel.DataAnnotations;

namespace VizsgaremekApp.Models
{
    public class Appointment
    {
        public int Id { get; set; }

        [Required]
        public string ClientName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Phone { get; set; }

        [Required]
        public int InstructorId { get; set; }

        [Required]
        public int PackageId { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        public string? Notes { get; set; }  // New notes field

        [Required]
        public DateTime BookingDate { get; set; }

        public Instructor Instructor { get; set; }
        public Package Package { get; set; }
    }
}