using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VizsgaremekApp.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;

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

        // GET: api/Appointments
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

        // GET: api/Appointments/5
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

            if (appointment == null)
                return NotFound();

            return appointment;
        }

        // GET: api/Appointments/availability?instructorId=1&date=2025-04-23&timeRange=10:00-11:00
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
                _logger.LogError(ex, "Hiba az elérhetőség ellenőrzése során");
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Hiba történt az időpont elérhetőségének ellenőrzésekor",
                    Error = ex.Message
                });
            }
        }

        // GET: api/Appointments/daterange?startDate=2025-04-01&endDate=2025-04-30
        [HttpGet("daterange")]
        public async Task<ActionResult<IEnumerable<object>>> GetAppointmentsByDateRange(DateTime startDate, DateTime endDate)
        {
            var appointments = await _context.Appointments
                .Include(a => a.Instructor)
                .Include(a => a.Package)
                .Where(a => a.StartTime >= startDate && a.EndTime <= endDate)
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

            return Ok(appointments);
        }

        // POST: api/Appointments
        [HttpPost]
        public async Task<ActionResult<Appointment>> CreateAppointment([FromBody] AppointmentCreateDto dto)
        {
            try
            {
                _logger.LogInformation("Bejövő foglalás kérés: {@Dto}", dto);

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Érvénytelen adat: {@ModelState}", ModelState);
                    return BadRequest(ModelState);
                }

                var (startTime, endTime) = ParseTimeRange(dto.Date, dto.TimeRange);
                _logger.LogInformation("Időintervallum: {StartTime} - {EndTime}", startTime, endTime);

                // Ellenőrizzük a helyet
                var currentCount = await _context.Appointments
                    .CountAsync(a => a.InstructorId == dto.InstructorId &&
                                    a.StartTime == startTime &&
                                    a.EndTime == endTime);

                if (currentCount >= MAX_PARTICIPANTS)
                {
                    _logger.LogWarning("Időpont betelt: {StartTime} - {EndTime}", startTime, endTime);
                    return BadRequest(new
                    {
                        Success = false,
                        Message = $"Ez az időpont már betelt (maximális létszám: {MAX_PARTICIPANTS})"
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

                _logger.LogInformation("Foglalás sikeresen létrehozva: {Id}", appointment.Id);

                return CreatedAtAction(nameof(GetAppointment), new { id = appointment.Id }, new
                {
                    Success = true,
                    Message = "Időpont sikeresen létrehozva",
                    Appointment = appointment
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Hiba a foglalás létrehozása során");
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Hiba történt az időpont létrehozásakor",
                    Error = ex.Message,
                    StackTrace = ex.StackTrace
                });
            }
        }

        // PUT: api/Appointments/5
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
                _logger.LogError(ex, "Hiba a foglalás frissítése során");
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Hiba történt az időpont frissítésekor"
                });
            }
        }

        // PUT: api/Appointments/cancel/5
        [HttpPut("cancel/{id}")]
        public async Task<IActionResult> CancelAppointment(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            appointment.IsActive = false; // Felmondás jelölése

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new
                {
                    Success = true,
                    Message = "Időpont sikeresen lemondva"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Hiba az időpont lemondása során");
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Hiba történt az időpont lemondásakor"
                });
            }
        }

        // DELETE: api/Appointments/5
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
                _logger.LogError(ex, "Hiba az időpont törlése során");
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Hiba történt az időpont törlésekor"
                });
            }
        }

        private bool AppointmentExists(int id)
        {
            return _context.Appointments.Any(e => e.Id == id);
        }

        /// <summary>
        /// Helper: Parses a time range string like "10:00-11:00" into start/end DateTime
        /// </summary>
        private (DateTime startTime, DateTime endTime) ParseTimeRange(DateTime date, string timeRange)
        {
            var parts = timeRange.Split('-');
            var start = TimeSpan.Parse(parts[0].Trim());
            var end = TimeSpan.Parse(parts[1].Trim());
            return (date.Date.Add(start), date.Date.Add(end));
        }
    }

    /// <summary>
    /// DTO for creating a new appointment.
    /// </summary>
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

    /// <summary>
    /// DTO for updating appointment.
    /// </summary>
    public class AppointmentUpdateDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Név megadása kötelező")]
        public string ClientName { get; set; }

        [Required(ErrorMessage = "Email cím megadása kötelező")]
        [EmailAddress(ErrorMessage = "Érvénytelen email cím")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Telefonszám megadása kötelező")]
        public string Phone { get; set; }

        public string? Notes { get; set; }  // Optional field
    }
}
