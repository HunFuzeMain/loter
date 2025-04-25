namespace VizsgaremekApp.Models;

public class Package
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Price { get; set; }
    public ICollection<Appointment>? Appointments { get; set; }
}