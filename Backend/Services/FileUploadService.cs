using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace VizsgaremekApp.Services;

public class FileUploadService
{
    private readonly IWebHostEnvironment _env;
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".pdf", ".doc", ".docx" };
    private const long _maxFileSize = 5 * 1024 * 1024; // 5MB

    public FileUploadService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string?> SaveFile(IFormFile file, string prefix)
    {
        if (file == null || file.Length == 0)
            return null;

        if (file.Length > _maxFileSize)
            throw new Exception("A fájl mérete meghaladja a maximálisan engedélyezett korlátot (5MB)");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(extension) || !_allowedExtensions.Contains(extension))
            throw new Exception("Érvénytelen fájltípus");

        var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var uniqueFileName = $"{prefix}_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return $"/uploads/{uniqueFileName}";
    }


    public void DeleteFile(string? filePath)
    {
        if (!string.IsNullOrEmpty(filePath))
        {
            var physicalPath = Path.Combine(_env.WebRootPath, filePath.TrimStart('/'));
            if (File.Exists(physicalPath))
            {
                File.Delete(physicalPath);
            }
        }
    }
}