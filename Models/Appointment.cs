namespace VizsgaremekApp.Models;

public class Appointment
{
    public int Id { get; set; }
    public string Nev { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime DatumIdo { get; set; }
    public string Tel { get; set; } = string.Empty;
    public int ValasztottOktatoId { get; set; }
    public int ValasztottCsomagId { get; set; }
    public Instructor? Instructor { get; set; }
    public Package? Package { get; set; }
}