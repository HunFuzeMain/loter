using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VizsgaremekApp.Models;

namespace ShootingRangeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PackagesController : ControllerBase
    {
        private readonly VizsgaremekContext _context;

        public PackagesController(VizsgaremekContext context)
        {
            _context = context;
        }

        // GET: api/Appointment
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Package>>> GetAppointments()
        {
            return await _context.Packages.ToListAsync();
        }

        // GET: api/Appointment/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Package>> GetAppointment(int id)
        {
            var package = await _context.Packages.FindAsync(id);

            if (package == null)
            {
                return NotFound();
            }

            return package;
        }

        // POST: api/Appointment
        [HttpPost]
        public async Task<ActionResult<Package>> PostAppointment(Package package)
        {
            _context.Packages.Add(package);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetAppointment", new { id = package.Id }, package);
        }

        // PUT: api/Appointment/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAppointment(int id, Package package)
        {
            if (id != package.Id)
            {
                return BadRequest();
            }

            _context.Entry(package).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AppointmentExists(id))
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

        // DELETE: api/Appointment/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAppointment(int id)
        {
            var package = await _context.Packages.FindAsync(id);
            if (package == null)
            {
                return NotFound();
            }

            _context.Packages.Remove(package);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AppointmentExists(int id)
        {
            return _context.Packages.Any(e => e.Id == id);
        }
    }
}