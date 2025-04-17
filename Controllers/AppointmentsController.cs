using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using VizsgaremekApp.Models;

namespace ShootingRangeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentsController : ControllerBase
    {
        private const int MAX_PARTICIPANTS = 7; // Defined as constant at class level
        private readonly VizsgaremekContext _context;
        private readonly ILogger<AppointmentsController> _logger;

        public AppointmentsController(VizsgaremekContext context, ILogger<AppointmentsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Appointment>>> GetAppointments()
        {
            return await _context.Appointments
                .Include(a => a.Instructor)
                .Include(a => a.Package)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Appointment>> GetAppointment(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Instructor)
                .Include(a => a.Package)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null) return NotFound();
            return appointment;
        }

        [HttpGet("availability")]
        public async Task<ActionResult<AvailabilityResponse>> CheckAvailability(
            [FromQuery] int instructorId,
            [FromQuery] DateTime date,
            [FromQuery] string timeRange)
        {
            try
            {
                var (startTime, endTime) = ParseTimeRange(date, timeRange);

                var currentCount = await _context.Appointments
                    .Where(a => a.InstructorId == instructorId &&
                                a.StartTime == startTime &&
                                a.EndTime == endTime)
                    .CountAsync();

                return Ok(new AvailabilityResponse
                {
                    IsAvailable = currentCount < MAX_PARTICIPANTS,
                    CurrentParticipants = currentCount,
                    MaxParticipants = MAX_PARTICIPANTS,
                    TimeSlot = timeRange
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking availability");
                return StatusCode(500, "Error checking availability");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Appointment>> CreateAppointment([FromBody] AppointmentCreateDto dto)
        {
            try
            {
                var (startTime, endTime) = ParseTimeRange(dto.Date, dto.TimeRange);

                // Check availability
                var currentCount = await _context.Appointments
                    .CountAsync(a => a.InstructorId == dto.InstructorId &&
                                    a.StartTime == startTime &&
                                    a.EndTime == endTime);

                if (currentCount >= MAX_PARTICIPANTS)
                {
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
                    BookingDate = DateTime.UtcNow
                };

                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();

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
                    Error = ex.Message
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutAppointment(int id, Appointment appointment)
        {
            if (id != appointment.Id) return BadRequest();

            _context.Entry(appointment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new
                {
                    Success = true,
                    Message = "Appointment updated successfully"
                });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AppointmentExists(id)) return NotFound();
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating appointment");
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Error updating appointment"
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAppointment(int id)
        {
            try
            {
                var appointment = await _context.Appointments.FindAsync(id);
                if (appointment == null) return NotFound();

                _context.Appointments.Remove(appointment);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Success = true,
                    Message = "Appointment deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting appointment");
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Error deleting appointment"
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

    public class AvailabilityResponse
    {
        public bool IsAvailable { get; set; }
        public int CurrentParticipants { get; set; }
        public int MaxParticipants { get; set; }
        public string TimeSlot { get; set; }
    }

    public class AppointmentCreateDto
    {
        [Required(ErrorMessage = "Client name is required")]
        public string ClientName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Date is required")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Time range is required")]
        public string TimeRange { get; set; }

        [Required(ErrorMessage = "Instructor is required")]
        public int InstructorId { get; set; }

        [Required(ErrorMessage = "Package is required")]
        public int PackageId { get; set; }
    }
}