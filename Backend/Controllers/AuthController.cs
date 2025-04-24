using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VizsgaremekApp.Models;

namespace ShootingRangeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly VizsgaremekContext _context;

        public AuthController(VizsgaremekContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if(loginDto.Username == "admin" && loginDto.Password == "admin")
            {
                return Ok(new { success = true });
            }
            var instructor = await _context.Instructors
                .FirstOrDefaultAsync(i => i.Name == loginDto.Username && i.Password == loginDto.Password);

            if (instructor == null)
                return Unauthorized("Hibás felhasználónév vagy jelszó");

            return Ok(new { success = true });
        }
    }

    public class LoginDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}