using System;

namespace VizsgaremekApp.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public string Nev { get; set; }
        public string Email { get; set; }
        public DateTime Datum { get; set; }
        public TimeSpan Ido { get; set; }
        public string Tel { get; set; }
        public int ValasztottOktatoId { get; set; }
        public int ValasztottCsomagId { get; set; }

        public Instructor ValasztottOktato { get; set; }
        public Package ValasztottCsomag { get; set; }
    }
}