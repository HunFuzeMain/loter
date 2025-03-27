using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using VizsgaremekApp.Models;

namespace ShootingRangeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegistrationsController : ControllerBase
    {
        private readonly VizsgaremekContext _context;

        public RegistrationsController(VizsgaremekContext context)
        {
            _context = context;
        }

        // GET: api/Registrations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Registration>>> GetRegistrations()
        {
            return await _context.Registrations.ToListAsync();
        }

        // GET: api/Registrations/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Registration>> GetRegistration(int id)
        {
            var registration = await _context.Registrations.FindAsync(id);

            if (registration == null)
            {
                return NotFound();
            }

            return registration;
        }

        // POST: api/Registrations/InstructorApplication
        [HttpPost("InstructorApplication")]
        public async Task<ActionResult<Registration>> PostInstructorApplication([FromBody] InstructorApplicationDto applicationDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var application = new Registration
                {
                    Nev = applicationDto.Nev,
                    Email = applicationDto.Email,
                    Tel = applicationDto.Tel,
                    Lakcim = applicationDto.Lakcim,
                    Minosites = applicationDto.Minosites,
                    MinositesIdeje = applicationDto.MinositesIdeje,
                    ApplicationDate = DateTime.UtcNow,
                    Status = "Pending"
                };

                _context.Registrations.Add(application);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetInstructorApplication), new { id = application.Id }, application);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "Hiba történt a jelentkezés feldolgozása során",
                    Error = ex.Message
                });
            }
        }

        // GET: api/Registrations/InstructorApplication/5
        [HttpGet("InstructorApplication/{id}")]
        public async Task<ActionResult<Registration>> GetInstructorApplication(int id)
        {
            var application = await _context.Registrations.FindAsync(id);

            if (application == null)
            {
                return NotFound();
            }

            return application;
        }

        // POST: api/Registrations
        [HttpPost]
        public async Task<ActionResult<Registration>> PostRegistration(Registration registration)
        {
            _context.Registrations.Add(registration);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRegistration), new { id = registration.Id }, registration);
        }

        // PUT: api/Registrations/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRegistration(int id, Registration registration)
        {
            if (id != registration.Id)
            {
                return BadRequest();
            }

            _context.Entry(registration).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RegistrationExists(id))
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

        // DELETE: api/Registrations/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRegistration(int id)
        {
            var registration = await _context.Registrations.FindAsync(id);
            if (registration == null)
            {
                return NotFound();
            }

            _context.Registrations.Remove(registration);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool RegistrationExists(int id)
        {
            return _context.Registrations.Any(e => e.Id == id);
        }
    }

    public class InstructorApplicationDto
    {
        [Required]
        public string Nev { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Tel { get; set; }

        [Required]
        public string Lakcim { get; set; }

        [Required]
        public string Minosites { get; set; }

        [Required]
        public DateTime MinositesIdeje { get; set; }
    }
}