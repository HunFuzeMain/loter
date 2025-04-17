using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VizsgaremekApp.Models;

[Route("api/[controller]")]
[ApiController]
public class InstructorsController : ControllerBase
{
    private readonly VizsgaremekContext _context;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<InstructorsController> _logger;

    public InstructorsController(
        VizsgaremekContext context,
        IWebHostEnvironment env,
        ILogger<InstructorsController> logger)
    {
        _context = context;
        _env = env;
        _logger = logger;
    }

    // GET: api/Instructors
    [HttpGet]
    public async Task<ActionResult<IEnumerable<InstructorDto>>> GetInstructors()
    {
        try
        {
            var instructors = await _context.Instructors
                .Where(i => i.Status == InstructorStatus.Hired) // Only show hired instructors
                .Select(i => new InstructorDto
                {
                    Id = i.Id,
                    Name = i.Name,
                    Email = i.Email,
                    Phone = i.Phone,
                    Status = i.Status.ToString()
                })
                .ToListAsync();

            return Ok(instructors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting instructors");
            return StatusCode(500, "Error retrieving instructors");
        }
    }

    // GET: api/Instructors/5
    [HttpGet("{id}")]
    public async Task<ActionResult<InstructorDetailDto>> GetInstructor(int id)
    {
        var instructor = await _context.Instructors
            .Where(i => i.Id == id)
            .Select(i => new InstructorDetailDto
            {
                Id = i.Id,
                Name = i.Name,
                Email = i.Email,
                Phone = i.Phone,
                Address = i.Address,
                Status = i.Status,
                HireDate = i.HireDate,
                QualificationFileUrl = i.QualificationFileName,
                IdCardFileUrl = i.IdCardFileName,
                CVFileUrl = i.CVFileName
            })
            .FirstOrDefaultAsync();

        if (instructor == null)
        {
            return NotFound();
        }

        return instructor;
    }

    // POST: api/Instructors
    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(5 * 1024 * 1024)] // 5MB
    public async Task<ActionResult<Instructor>> CreateInstructor([FromForm] InstructorCreateDto instructorDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var instructor = new Instructor
            {
                Name = instructorDto.Name,
                Email = instructorDto.Email,
                Phone = instructorDto.Phone,
                Address = instructorDto.Address,
                Status = InstructorStatus.Pending,
                ApplicationDate = DateTime.UtcNow,
                IsActive = false
            };

            // Process file uploads
            instructor.QualificationFileName = await SaveFile(
                instructorDto.QualificationFile,
                "qualifications",
                new[] { ".pdf", ".jpg", ".jpeg", ".png" });

            instructor.IdCardFileName = await SaveFile(
                instructorDto.IdCardFile,
                "idcards",
                new[] { ".pdf", ".jpg", ".jpeg", ".png" });

            instructor.CVFileName = await SaveFile(
                instructorDto.CVFile,
                "cvs",
                new[] { ".pdf", ".doc", ".docx" });

            _context.Instructors.Add(instructor);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetInstructor), new { id = instructor.Id }, instructor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating instructor");
            return StatusCode(500, "Error creating instructor");
        }
    }

    // PATCH: api/Instructors/5/status
    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] InstructorStatusUpdateDto updateDto)
    {
        var instructor = await _context.Instructors.FindAsync(id);
        if (instructor == null)
        {
            return NotFound();
        }

        instructor.Status = updateDto.NewStatus;

        // Update relevant dates
        if (updateDto.NewStatus == InstructorStatus.Hired)
        {
            instructor.HireDate = DateTime.UtcNow;
            instructor.IsActive = true;
        }
        else if (updateDto.NewStatus == InstructorStatus.Terminated)
        {
            instructor.TerminationDate = DateTime.UtcNow;
            instructor.IsActive = false;
        }
        else if (updateDto.NewStatus == InstructorStatus.OnLeave)
        {
            instructor.IsActive = false;
        }

        instructor.Notes = updateDto.Notes;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    private async Task<string> SaveFile(IFormFile file, string folder, string[] allowedExtensions)
    {
        if (file == null || file.Length == 0)
        {
            throw new Exception("File upload is required");
        }

        // Validate size (5MB max)
        if (file.Length > 5 * 1024 * 1024)
        {
            throw new Exception($"File size too large (max 5MB). Uploaded: {file.Length / 1024 / 1024}MB");
        }

        // Validate extension
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
        {
            throw new Exception($"Invalid file type ({extension}). Allowed: {string.Join(", ", allowedExtensions)}");
        }

        // Create upload directory if not exists
        var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", folder);
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        // Generate unique filename
        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        // Save file
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return $"/uploads/{folder}/{fileName}";
    }
}