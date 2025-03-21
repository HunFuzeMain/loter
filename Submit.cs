using Microsoft.AspNetCore.Mvc;
using VizsgaremekApp.Models;

[ApiController]
[Route("api/[controller]")]
public class SubmitController : ControllerBase
{
    private readonly VizsgaremekContext _context;

    public SubmitController(VizsgaremekContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] AppointmentData data)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Map the data to the Appointment entity
        var appointment = new Appointment
        {
            Nev = data.Nev,
            Email = data.Email,
            Datum = DateTime.Parse(data.Datum),
            Ido = TimeSpan.Parse(data.Ido),
            Tel = data.Tel,
            ValasztottOktatoId = data.ValasztottOktatoId,
            ValasztottCsomagId = data.ValasztottCsomagId
        };

        // Add the appointment to the database
        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Sikeres időpontfoglalás!" });
    }
}

public class AppointmentData
{
    public string Nev { get; set; }
    public string Email { get; set; }
    public string Datum { get; set; }
    public string Ido { get; set; }
    public string Tel { get; set; }
    public int ValasztottOktatoId { get; set; }
    public int ValasztottCsomagId { get; set; }
}