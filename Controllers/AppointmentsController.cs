using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VizsgaremekApp.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ShootingRangeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentsController : ControllerBase
    {
        private const int MAX_PARTICIPANTS = 7;
        private readonly VizsgaremekContext _context;
        private readonly ILogger<AppointmentsController> _logger;

        public AppointmentsController(VizsgaremekContext context, ILogger<AppointmentsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetAppointments()
        {
            return await _context.Appointments
                .Include(a => a.Instructor)
                .Include(a => a.Package)
                .Select(a => new {
                    a.Id,
                    a.ClientName,
                    a.Email,
                    a.Phone,
                    a.StartTime,
                    a.EndTime,
                    a.Notes,
                    Instructor = a.Instructor.Name,
                    Package = a.Package.Name,
                    a.BookingDate
                })
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetAppointment(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Instructor)
                .Include(a => a.Package)
                .Where(a => a.Id == id)
                .Select(a => new {
                    a.Id,
                    a.ClientName,
                    a.Email,
                    a.Phone,
                    a.StartTime,
                    a.EndTime,
                    a.Notes,
                    Instructor = a.Instructor.Name,
                    Package = a.Package.Name,
                    a.BookingDate
                })
                .FirstOrDefaultAsync();

            if (appointment == null) return NotFound();
            return appointment;
        }

        [HttpGet("availability")]
        public async Task<ActionResult<object>> CheckAvailability(int instructorId, DateTime date, string timeRange)
        {
            try
            {
                var (startTime, endTime) = ParseTimeRange(date, timeRange);

                var currentCount = await _context.Appointments
                    .CountAsync(a => a.InstructorId == instructorId &&
                                    a.StartTime == startTime &&
                                    a.EndTime == endTime);

                var isAvailable = currentCount < MAX_PARTICIPANTS;

                return Ok(new
                {
                    isAvailable,
                    currentParticipants = currentCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking availability");
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Error checking availability",
                    Error = ex.Message
                });
            }
        }


        [HttpPost]
        public async Task<ActionResult<Appointment>> CreateAppointment([FromBody] AppointmentCreateDto dto)
        {
            try
            {
                _logger.LogInformation("Received appointment creation request: {@Dto}", dto);

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state: {@ModelState}", ModelState);
                    return BadRequest(ModelState);
                }

                var (startTime, endTime) = ParseTimeRange(dto.Date, dto.TimeRange);
                _logger.LogInformation("Parsed time range: {StartTime} to {EndTime}", startTime, endTime);

                // Check availability
                var currentCount = await _context.Appointments
                    .CountAsync(a => a.InstructorId == dto.InstructorId &&
                                    a.StartTime == startTime &&
                                    a.EndTime == endTime);

                if (currentCount >= MAX_PARTICIPANTS)
                {
                    _logger.LogWarning("Time slot full for {StartTime}-{EndTime}", startTime, endTime);
                    return BadRequest(new
                    {
                        Success = false,
                        Message = $"This time slot is full (maximum {MAX_PARTICIPANTS} participants allowed)"
                    });
                }

                var appointment = new Appointment
                {
                    ClientName = dto.ClientName,
                    Email = dto.Email,
                    Phone = dto.Phone,
                    InstructorId = dto.InstructorId,
                    PackageId = dto.PackageId,
                    StartTime = startTime,
                    EndTime = endTime,
                    BookingDate = DateTime.UtcNow,
                    Notes = dto.Notes
                };

                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Appointment created successfully: {Id}", appointment.Id);

                return CreatedAtAction(nameof(GetAppointment), new { id = appointment.Id }, new
                {
                    Success = true,
                    Message = "Appointment created successfully",
                    Appointment = appointment
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating appointment");
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Error creating appointment",
                    Error = ex.Message,
                    StackTrace = ex.StackTrace
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAppointment(int id, [FromBody] AppointmentUpdateDto dto)
        {
            if (id != dto.Id) return BadRequest();

            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            appointment.ClientName = dto.ClientName;
            appointment.Email = dto.Email;
            appointment.Phone = dto.Phone;
            appointment.Notes = dto.Notes;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new
                {
                    Success = true,
                    Message = "Időpont sikeresen frissítve"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Hiba történt az időpont frissítése során");
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Hiba történt az időpont frissítése során"
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAppointment(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            try
            {
                _context.Appointments.Remove(appointment);
                await _context.SaveChangesAsync();
                return Ok(new
                {
                    Success = true,
                    Message = "Időpont sikeresen törölve"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Hiba történt az időpont törlése során");
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Hiba történt az időpont törlése során"
                });
            }
        }

        private bool AppointmentExists(int id)
        {
            return _context.Appointments.Any(e => e.Id == id);
        }

        private (DateTime startTime, DateTime endTime) ParseTimeRange(DateTime date, string timeRange)
        {
            var parts = timeRange.Split('-');
            var start = TimeSpan.Parse(parts[0].Trim());
            var end = TimeSpan.Parse(parts[1].Trim());
            return (date.Date.Add(start), date.Date.Add(end));
        }
    }
    public class AppointmentCreateDto
    {
        [Required(ErrorMessage = "Név megadása kötelező")]
        public string ClientName { get; set; }

        [Required(ErrorMessage = "Email cím megadása kötelező")]
        [EmailAddress(ErrorMessage = "Érvénytelen email cím")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Telefonszám megadása kötelező")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Dátum megadása kötelező")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Időpont kiválasztása kötelező")]
        public string TimeRange { get; set; }

        [Required(ErrorMessage = "Oktató kiválasztása kötelező")]
        public int InstructorId { get; set; }

        [Required(ErrorMessage = "Csomag kiválasztása kötelező")]
        public int PackageId { get; set; }

        public string? Notes { get; set; }  // Optional field
    }
    public class AppointmentUpdateDto
    {
        public int Id { get; set; }

        [Required]
        public string ClientName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Phone { get; set; }

        public string Notes { get; set; }  // Optional field
    }
}