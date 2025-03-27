// Models/InstructorApplication.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace VizsgaremekApp.Models
{
    public class Registration
    {
        public int Id { get; set; }

        [Required]
        public string Nev { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Tel { get; set; }

        [Required]
        public string Lakcim { get; set; }

        [Required]
        public string Minosites { get; set; }

        [Required]
        public DateTime MinositesIdeje { get; set; }

        public DateTime ApplicationDate { get; set; }

        public string Status { get; set; } // Pending, Approved, Rejected
    }
}