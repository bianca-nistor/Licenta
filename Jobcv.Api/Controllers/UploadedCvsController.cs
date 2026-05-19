using JobCv.Api.Data;
using JobCv.Api.Dtos;
using JobCv.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobCv.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadedCvsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        private readonly string[] _allowedExtensions = { ".pdf", ".doc", ".docx" };

        public UploadedCvsController(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [HttpPost("user/{userId}")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(10_000_000)]
        public async Task<IActionResult> UploadCvFile(int userId, IFormFile file)
        {
            var userExists = await _context.Users.AnyAsync(x => x.Id == userId);

            if (!userExists)
            {
                return NotFound("User was not found.");
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest("No file was uploaded.");
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!_allowedExtensions.Contains(extension))
            {
                return BadRequest("Only PDF, DOC and DOCX files are allowed.");
            }

            var maxFileSize = 10 * 1024 * 1024;

            if (file.Length > maxFileSize)
            {
                return BadRequest("File size cannot exceed 10 MB.");
            }

            var uploadsFolder = Path.Combine(_environment.ContentRootPath, "Uploads", "Cvs");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var storedFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, storedFileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var uploadedFile = new UploadedCvFile
            {
                UserId = userId,
                OriginalFileName = file.FileName,
                StoredFileName = storedFileName,
                ContentType = file.ContentType,
                FilePath = filePath,
                FileSize = file.Length,
                UploadedAt = DateTime.UtcNow
            };

            _context.UploadedCvFiles.Add(uploadedFile);
            await _context.SaveChangesAsync();

            return Ok(ToDto(uploadedFile));
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserUploadedCvs(int userId)
        {
            var files = await _context.UploadedCvFiles
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.UploadedAt)
                .Select(x => new UploadedCvFileDto
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    OriginalFileName = x.OriginalFileName,
                    ContentType = x.ContentType,
                    FileSize = x.FileSize,
                    UploadedAt = x.UploadedAt
                })
                .ToListAsync();

            return Ok(files);
        }

        [HttpGet("{fileId}/download")]
        public async Task<IActionResult> DownloadUploadedCv(int fileId)
        {
            var uploadedFile = await _context.UploadedCvFiles.FindAsync(fileId);

            if (uploadedFile == null)
            {
                return NotFound("Uploaded CV was not found.");
            }

            if (!System.IO.File.Exists(uploadedFile.FilePath))
            {
                return NotFound("The file no longer exists on the server.");
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(uploadedFile.FilePath);

            return File(fileBytes, uploadedFile.ContentType, uploadedFile.OriginalFileName);
        }

        [HttpDelete("{fileId}")]
        public async Task<IActionResult> DeleteUploadedCv(int fileId)
        {
            var uploadedFile = await _context.UploadedCvFiles.FindAsync(fileId);

            if (uploadedFile == null)
            {
                return NotFound("Uploaded CV was not found.");
            }

            if (System.IO.File.Exists(uploadedFile.FilePath))
            {
                System.IO.File.Delete(uploadedFile.FilePath);
            }

            _context.UploadedCvFiles.Remove(uploadedFile);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Uploaded CV deleted successfully." });
        }

        private static UploadedCvFileDto ToDto(UploadedCvFile file)
        {
            return new UploadedCvFileDto
            {
                Id = file.Id,
                UserId = file.UserId,
                OriginalFileName = file.OriginalFileName,
                ContentType = file.ContentType,
                FileSize = file.FileSize,
                UploadedAt = file.UploadedAt
            };
        }
        [HttpGet("{fileId}/preview")]
        public async Task<IActionResult> PreviewUploadedCv(int fileId)
        {
            var uploadedFile = await _context.UploadedCvFiles.FindAsync(fileId);

            if (uploadedFile == null)
            {
                return NotFound("Uploaded CV was not found.");
            }

            if (!System.IO.File.Exists(uploadedFile.FilePath))
            {
                return NotFound("The file no longer exists on the server.");
            }

            var extension = Path.GetExtension(uploadedFile.OriginalFileName).ToLowerInvariant();

            if (extension != ".pdf")
            {
                return BadRequest("Only PDF files can be previewed in the application.");
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(uploadedFile.FilePath);

            return File(fileBytes, "application/pdf");
        }
    }
}