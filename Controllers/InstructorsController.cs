using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VizsgaremekApp.Models;
using System.ComponentModel.DataAnnotations;

namespace ShootingRangeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InstructorsController : ControllerBase
    {
        private readonly VizsgaremekContext _context;
        private readonly ILogger<InstructorsController> _logger;

        public InstructorsController(VizsgaremekContext context, ILogger<InstructorsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Instructors
        [HttpGet]
        public async Task<ActionResult<IEnumerable<InstructorDto>>> GetInstructors()
        {
            try
            {
                var instructors = await _context.Instructors
                    .Where(i => i.IsActive)
                    .Select(i => new InstructorDto
                    {
                        Id = i.Id,
                        Name = i.Name,
                        Email = i.Email,
                        Phone = i.Phone,
                        HireDate = i.HireDate,
                        Status = i.Status.ToString(),
                        ActiveAppointmentsCount = _context.Appointments
                            .Count(a => a.InstructorId == i.Id && a.StartTime >= DateTime.Now)
                    })
                    .ToListAsync();

                return Ok(instructors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting instructors");
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/Instructors/5
        [HttpGet("{id}")]
        public async Task<ActionResult<InstructorDetailDto>> GetInstructor(int id)
        {
            try
            {
                var instructor = await _context.Instructors
                    .Include(i => i.Appointments)
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (instructor == null)
                {
                    return NotFound();
                }

                var instructorDetail = new InstructorDetailDto
                {
                    Id = instructor.Id,
                    Name = instructor.Name,
                    Email = instructor.Email,
                    Phone = instructor.Phone,
                    Address = instructor.Address,
                    Status = instructor.Status.ToString(),
                    ApplicationDate = instructor.ApplicationDate,
                    HireDate = instructor.HireDate,
                    IsActive = instructor.IsActive,
                    UpcomingAppointments = instructor.Appointments
                        .Where(a => a.StartTime >= DateTime.Now)
                        .OrderBy(a => a.StartTime)
                        .Select(a => new AppointmentDto
                        {
                            Id = a.Id,
                            ClientName = a.ClientName,
                            StartTime = a.StartTime,
                            EndTime = a.EndTime,
                            PackageName = _context.Packages.FirstOrDefault(p => p.Id == a.PackageId)?.Name
                        })
                        .ToList()
                };

                return Ok(instructorDetail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting instructor with id {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        // POST: api/Instructors
        [HttpPost]
        public async Task<ActionResult<Instructor>> CreateInstructor([FromBody] InstructorCreateDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Check if instructor with same name already exists
                if (await _context.Instructors.AnyAsync(i => i.Name == dto.Name))
                {
                    return Conflict("Instructor with this name already exists");
                }

                var instructor = new Instructor
                {
                    Name = dto.Name,
                    Email = dto.Email,
                    Phone = dto.Phone,
                    Address = dto.Address,
                    Password = dto.Password, // In production, hash this password
                    Status = InstructorStatus.Pending,
                    ApplicationDate = DateTime.UtcNow,
                    IsActive = true,
                    QualificationFileName = dto.QualificationFileName,
                    IdCardFileName = dto.IdCardFileName,
                    CVFileName = dto.CVFileName
                };

                _context.Instructors.Add(instructor);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetInstructor), new { id = instructor.Id }, instructor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating instructor");
                return StatusCode(500, "Internal server error");
            }
        }

        // PUT: api/Instructors/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateInstructor(int id, [FromBody] InstructorUpdateDto dto)
        {
            try
            {
                if (id != dto.Id)
                {
                    return BadRequest("ID mismatch");
                }

                var instructor = await _context.Instructors.FindAsync(id);
                if (instructor == null)
                {
                    return NotFound();
                }

                instructor.Name = dto.Name;
                instructor.Email = dto.Email;
                instructor.Phone = dto.Phone;
                instructor.Address = dto.Address;
                instructor.Status = dto.Status;
                instructor.IsActive = dto.IsActive;

                if (!string.IsNullOrEmpty(dto.Password))
                {
                    instructor.Password = dto.Password; // In production, hash this password
                }

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InstructorExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating instructor with id {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        // PATCH: api/Instructors/5/password
        [HttpPatch("{id}/password")]
        public async Task<IActionResult> UpdatePassword(int id, [FromBody] PasswordUpdateDto dto)
        {
            try
            {
                var instructor = await _context.Instructors.FindAsync(id);
                if (instructor == null)
                {
                    return NotFound();
                }

                // In production, verify old password first
                // if (instructor.Password != HashPassword(dto.OldPassword))
                // {
                //     return BadRequest("Current password is incorrect");
                // }

                instructor.Password = dto.NewPassword; // In production, hash this password
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating password for instructor with id {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        // DELETE: api/Instructors/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInstructor(int id)
        {
            try
            {
                var instructor = await _context.Instructors.FindAsync(id);
                if (instructor == null)
                {
                    return NotFound();
                }

                // Check if instructor has future appointments
                var hasFutureAppointments = await _context.Appointments
                    .AnyAsync(a => a.InstructorId == id && a.StartTime >= DateTime.Now);

                if (hasFutureAppointments)
                {
                    return BadRequest("Cannot delete instructor with future appointments");
                }

                // Soft delete
                instructor.IsActive = false;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting instructor with id {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        private bool InstructorExists(int id)
        {
            return _context.Instructors.Any(e => e.Id == id);
        }
    }

    // DTO Classes
    public class InstructorDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime? HireDate { get; set; }
        public string Status { get; set; }
        public int ActiveAppointmentsCount { get; set; }
    }

    public class InstructorDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Status { get; set; }
        public DateTime ApplicationDate { get; set; }
        public DateTime? HireDate { get; set; }
        public bool IsActive { get; set; }
        public List<AppointmentDto> UpcomingAppointments { get; set; }
    }

    public class InstructorCreateDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(20)]
        public string Phone { get; set; }

        [StringLength(200)]
        public string Address { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; }

        public string QualificationFileName { get; set; }
        public string IdCardFileName { get; set; }
        public string CVFileName { get; set; }
    }

    public class InstructorUpdateDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Password { get; set; } // Only used when changing password
        public InstructorStatus Status { get; set; }
        public bool IsActive { get; set; }
    }

    public class PasswordUpdateDto
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }

    public class AppointmentDto
    {
        public int Id { get; set; }
        public string ClientName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string PackageName { get; set; }
    }
}