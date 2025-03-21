using System;

namespace VizsgaremekApp.Models
{
    public class Registration
    {
        public int Id { get; set; }
        public string Nev { get; set; }
        public string Email { get; set; }
        public string Tel { get; set; }
        public string Lakcim { get; set; }
        public string Minosites { get; set; }
        public DateTime MinositesIdeje { get; set; }
    }
}