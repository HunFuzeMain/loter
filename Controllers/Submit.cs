using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
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
    public async Task<IActionResult> Post([FromBody] Appointment data)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Log the received data for debugging
            Console.WriteLine($"Received data: {JsonSerializer.Serialize(data)}");

            // Map the data to the Appointment entity
            var appointment = new Appointment
            {
                ClientName = data.ClientName,
                Email = data.Email,
                StartTime = data.StartTime, // Directly assign the DateTime value
                EndTime = data.EndTime, // Directly assign the DateTime value
                Phone = data.Phone,
                InstructorId = data.InstructorId,
                PackageId = data.PackageId,
                BookingDate = data.BookingDate // Directly assign the DateTime value
            };

            // Add the appointment to the database
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Sikeres időpontfoglalás!" });
        }
        catch (Exception ex)
        {
            // Log the exception for debugging
            Console.Error.WriteLine($"Error: {ex.Message}");
            Console.Error.WriteLine($"Stack Trace: {ex.StackTrace}");

            // Return a 500 error with the exception message
            return StatusCode(500, new { Message = "Hiba történt az időpontfoglalás során.", Error = ex.Message });
        }
    }
}
