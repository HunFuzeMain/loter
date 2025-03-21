using System;

namespace VizsgaremekApp.Models
{
    public class Appointment
    {
        public int Id { get; set; } // Primary key (optional, depending on your database setup)
        public string Nev { get; set; } // Name
        public string Email { get; set; } // Email
        public DateTime DatumIdo { get; set; } // Date and time
        public string Tel { get; set; } // Phone number
        public int ValasztottOktatoId { get; set; } // Selected instructor ID
        public int ValasztottCsomagId { get; set; } // Selected package ID
    }
}