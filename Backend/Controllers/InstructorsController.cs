using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VizsgaremekApp.Models;
using VizsgaremekApp.Services;

namespace ShootingRangeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InstructorsController : ControllerBase
    {
        private readonly VizsgaremekContext _context;
        private readonly ILogger<InstructorsController> _logger;
        private readonly EmailService _emailService;
        private readonly IWebHostEnvironment _environment;

        /// <summary>
        /// Constructor injects database context, logger, email service, and hosting environment.
        /// </summary>
        public InstructorsController(
            VizsgaremekContext context,
            ILogger<InstructorsController> logger,
            EmailService emailService,
            IWebHostEnvironment environment)
        {
            _context = context;
            _logger = logger;
            _emailService = emailService;
            _environment = environment;
        }

        /// <summary>
        /// Retrieves all active instructors with basic info and count of upcoming appointments.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<InstructorDto>>> GetInstructors()
        {
            try { 
            var instructors = await _context.Instructors
                //.Where(i => i.IsActive)
                .Select(i => new InstructorDto
                {
                    Id = i.Id,
                    Name = i.Name,
                    Email = i.Email,
                    Phone = i.Phone,
                    HireDate = i.HireDate,
                    ApplicationDate = i.ApplicationDate,   
                    Status = i.Status.ToString(),
                    ActiveAppointmentsCount = _context.Appointments
                        .Count(a => a.InstructorId == i.Id && a.StartTime >= DateTime.Now),
                    IsActive = i.IsActive
                })
                .ToListAsync();
                return Ok(instructors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving instructors");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

        /// <summary>
        /// Retrieves detailed info for a specific instructor, including upcoming appointments.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<InstructorDetailDto>> GetInstructor(int id)
        {
            try
            {
                var instructor = await _context.Instructors
                    .Include(i => i.Appointments)
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (instructor == null)
                    return NotFound();

                var detail = new InstructorDetailDto
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
                            PackageName = _context.Packages
                                .FirstOrDefault(p => p.Id == a.PackageId)?.Name
                        }).ToList()
                };

                return Ok(detail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving instructor with id {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }
        // GET: api/Instructors/all
        // GET: api/Instructors/all
        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<InstructorAllDto>>> GetAllInstructors()
        {
            var all = await _context.Instructors
                .Select(i => new InstructorAllDto
                {
                    Id = i.Id,
                    Name = i.Name,
                    Status = i.Status.ToString(),
                    ApplicationDate = i.ApplicationDate,
                    HireDate = i.HireDate,
                    IsActive = i.IsActive
                })
                .ToListAsync();
            return Ok(all);
        }


        /// <summary>
        /// Creates a new instructor application, saves uploaded files, generates a password, and sends a welcome email.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> CreateInstructor([FromForm] InstructorCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check for duplicate name
            if (await _context.Instructors.AnyAsync(i => i.Name == dto.Name))
                return Conflict("Instructor with this name already exists");

            // Set up upload directories under wwwroot/uploads
            string root = Path.Combine(_environment.WebRootPath, "uploads");
            string qualDir = Path.Combine(root, "qualifications");
            string idCardDir = Path.Combine(root, "idcards");
            string cvDir = Path.Combine(root, "cvs");
            Directory.CreateDirectory(qualDir);
            Directory.CreateDirectory(idCardDir);
            Directory.CreateDirectory(cvDir);

            // Save files with unique names
            string qualName = $"{Guid.NewGuid()}{Path.GetExtension(dto.QualificationFileName.FileName)}";
            string idCardName = $"{Guid.NewGuid()}{Path.GetExtension(dto.IdCardFileName.FileName)}";
            string cvName = $"{Guid.NewGuid()}{Path.GetExtension(dto.CVFileName.FileName)}";

            string qualPath = Path.Combine(qualDir, qualName);
            string idCardPath = Path.Combine(idCardDir, idCardName);
            string cvPath = Path.Combine(cvDir, cvName);

            using (var fs = new FileStream(qualPath, FileMode.Create))
                await dto.QualificationFileName.CopyToAsync(fs);
            using (var fs = new FileStream(idCardPath, FileMode.Create))
                await dto.IdCardFileName.CopyToAsync(fs);
            using (var fs = new FileStream(cvPath, FileMode.Create))
                await dto.CVFileName.CopyToAsync(fs);

            // Generate a random temporary password
            string tempPassword = Guid.NewGuid().ToString("N").Substring(0, 10);

            // Create and save the instructor entity
            var instructor = new Instructor
            {
                Name = dto.Name,
                Email = dto.Email,
                Phone = dto.Phone,
                Address = dto.Address,
                Password = tempPassword,
                Status = InstructorStatus.Pending,
                ApplicationDate = DateTime.UtcNow,
                IsActive = true,
                QualificationFileName = Path.Combine("uploads", "qualifications", qualName),
                IdCardFileName = Path.Combine("uploads", "idcards", idCardName),
                CVFileName = Path.Combine("uploads", "cvs", cvName)
            };
            _context.Instructors.Add(instructor);
            await _context.SaveChangesAsync();

            // Send welcome email with temporary password
            try
            {
                await _emailService.SendPasswordEmail(instructor.Email, instructor.Name, tempPassword);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send welcome email to {Email}", instructor.Email);
            }

            return CreatedAtAction(nameof(GetInstructor), new { id = instructor.Id },
                new { Message = "Sikeres jelentkezés!", InstructorId = instructor.Id });
        }


        // PATCH: api/Instructors/5/password
        //Jelszó frissítése
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
                instructor.Password = dto.NewPassword; 
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating password for instructor with id {id}");
                return StatusCode(500, "Internal server error");
            }
        }
        // PATCH: api/Instructors/5/hire
        // Állítja az oktató státuszát „Hired”-re, és beállítja a HireDate-et
        [HttpPatch("{id}/hire")]
        public async Task<IActionResult> HireInstructor(int id)
        {
            var instructor = await _context.Instructors.FindAsync(id);
            if (instructor == null)
                return NotFound(new { Success = false, Message = "Oktató nem található" });

            // Csak akkor engedélyezzük, ha még nem hired
            if (instructor.Status == InstructorStatus.Hired)
                return BadRequest(new { Success = false, Message = "Oktató már felvett státuszú" });

            instructor.Status = InstructorStatus.Hired;
            instructor.HireDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { Success = true, Message = "Oktató sikeresen felvéve" });
        }
        // PATCH: api/Instructors/5/reactivate
        [HttpPatch("{id}/reactivate")]
        public async Task<IActionResult> ReactivateInstructor(int id)
        {
            var instructor = await _context.Instructors.FindAsync(id);
            if (instructor == null)
                return NotFound(new { Success = false, Message = "Oktató nem található" });

            // Újra aktívvá tesszük
            instructor.IsActive = true;

            // instructor.Status = InstructorStatus.Pending; //Státusz visszaállítása Pending-re

            await _context.SaveChangesAsync();
            return Ok(new { Success = true, Message = "Oktató sikeresen reaktiválva" });
        }


        // DELETE: api/Instructors/5
        // Törli az oktatót, ha nincsenek előre foglalt órái
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInstructor(int id)
        {
            var instructor = await _context.Instructors.FindAsync(id);
            if (instructor == null)
                return NotFound(new { Success = false, Message = "Oktató nem található" });

            // Ellenőrizzük, hogy vannak-e jövőbeli foglalások
            bool hasFuture = await _context.Appointments
                .AnyAsync(a => a.InstructorId == id && a.StartTime >= DateTime.Now);

            if (hasFuture)
                return BadRequest(new { Success = false, Message = "Nem törölhető oktató, mert vannak jövőbeli időpontjai" });

            // Soft delete (inaktívvá tesszük)
            instructor.IsActive = false;
            await _context.SaveChangesAsync();

            return Ok(new { Success = true, Message = "Oktató sikeresen törölve" });
        }

        [HttpDelete("{id}/permanent")]
        public async Task<IActionResult> FullDeleteInstructor(int id)
        {
            try
            {
                // Find the instructor by ID
                var instructor = await _context.Instructors.FindAsync(id);
                if (instructor == null)
                {
                    return NotFound(new { Success = false, Message = "Oktató nem található" });
                }

                // Delete the instructor's files from the server
                DeleteFileIfExists(instructor.QualificationFileName);
                DeleteFileIfExists(instructor.IdCardFileName);
                DeleteFileIfExists(instructor.CVFileName);

                // Delete the instructor from the database
                _context.Instructors.Remove(instructor);
                await _context.SaveChangesAsync();

                return Ok(new { Success = true, Message = "Oktató és fájlok véglegesen törölve" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting instructor with id {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Hiba történt");
            }
        }


        // Ha létezik a megadott relatív útvonalú fájl a wwwroot alatt, törli.
        private void DeleteFileIfExists(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return;

            // A relatív útvonal összefűzése a wwwroot gyökérrel
            var fullPath = Path.Combine(_environment.WebRootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(fullPath))
            {
                try
                {
                    System.IO.File.Delete(fullPath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Nem sikerült törölni a fájlt: {Path}", fullPath);
                }
            }
        }




        private bool InstructorExists(int id) => _context.Instructors.Any(e => e.Id == id);
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
        public DateTime ApplicationDate { get; set; }
        public bool IsActive { get; set; }
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

        public IFormFile QualificationFileName { get; set; }
        public IFormFile IdCardFileName { get; set; }
        public IFormFile CVFileName { get; set; }
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

    public class InstructorAllDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public DateTime ApplicationDate { get; set; }
        public DateTime? HireDate { get; set; }
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
