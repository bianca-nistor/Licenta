using System.Text;
using JobCv.Api.Data;
using JobCv.Api.Dtos;
using JobCv.Api.Models;
using JobCv.Api.Services;
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
        private readonly ICvTextExtractionService _textExtractionService;
        private readonly ICvImportParserService _cvImportParserService;

        private readonly string[] _allowedExtensions = { ".pdf" };

        public UploadedCvsController(
            AppDbContext context,
            IWebHostEnvironment environment,
            ICvTextExtractionService textExtractionService,
            ICvImportParserService cvImportParserService)
        {
            _context = context;
            _environment = environment;
            _textExtractionService = textExtractionService;
            _cvImportParserService = cvImportParserService;
        }

        [HttpPost("user/{userId}")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(25_000_000)]
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

            var originalFileName = Path.GetFileName(file.FileName);
            var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
            var contentType = file.ContentType ?? string.Empty;

            var isPdfByExtension = extension == ".pdf";
            var isPdfByContentType = contentType.Contains("pdf", StringComparison.OrdinalIgnoreCase);
            var isPdfBySignature = await LooksLikePdfAsync(file);

            if (!isPdfByExtension && !isPdfByContentType && !isPdfBySignature)
            {
                return BadRequest("Only PDF files are allowed. Make sure the selected file is a real PDF file.");
            }

            if (extension != ".pdf")
            {
                extension = ".pdf";
            }

            if (string.IsNullOrWhiteSpace(originalFileName))
            {
                originalFileName = $"uploaded-cv{extension}";
            }

            var maxFileSize = 25 * 1024 * 1024;

            if (file.Length > maxFileSize)
            {
                return BadRequest("File size cannot exceed 25 MB.");
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
                OriginalFileName = originalFileName,
                StoredFileName = storedFileName,
                ContentType = "application/pdf",
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

        [HttpPost("user/{userId:int}/{fileId:int}/import-to-cv")]
        public async Task<IActionResult> ImportUploadedCvToEditableCv(
            int userId,
            int fileId,
            [FromBody] ImportUploadedCvRequestDto dto)
        {
            var uploadedFile = await _context.UploadedCvFiles
                .FirstOrDefaultAsync(x => x.Id == fileId && x.UserId == userId);

            if (uploadedFile == null)
                return NotFound("Uploaded CV was not found for this user.");

            if (!System.IO.File.Exists(uploadedFile.FilePath))
                return NotFound("The file no longer exists on the server.");

            var extension = Path.GetExtension(uploadedFile.OriginalFileName).ToLowerInvariant();

            if (extension != ".pdf")
                return BadRequest("Only PDF files can be imported into editable CVs.");

            string extractedText;

            try
            {
                extractedText = await _textExtractionService.ExtractTextAsync(uploadedFile.FilePath, uploadedFile.OriginalFileName);
            }
            catch (NotSupportedException ex)
            {
                return BadRequest(ex.Message);
            }
            catch
            {
                return BadRequest("Could not read text from this PDF file.");
            }

            if (string.IsNullOrWhiteSpace(extractedText) || extractedText.Trim().Length < 15)
            {
                return BadRequest("This PDF was uploaded, but no readable text could be extracted from it. It may be scanned, image-only, protected, or exported in a way that hides the text.");
            }

            var parsed = await _cvImportParserService.ParseAsync(extractedText, dto.UseAi);

            var cv = new Cv
            {
                UserId = userId,
                Title = string.IsNullOrWhiteSpace(dto.Title)
                    ? $"Imported - {Path.GetFileNameWithoutExtension(uploadedFile.OriginalFileName)}"
                    : dto.Title.Trim(),
                Language = string.IsNullOrWhiteSpace(dto.Language) ? "ro" : dto.Language.Trim(),
                TemplateName = string.IsNullOrWhiteSpace(dto.TemplateName) ? "modern-blue" : dto.TemplateName.Trim(),
                FullName = Clean(parsed.FullName),
                Email = Clean(parsed.Email),
                Phone = Clean(parsed.Phone),
                Location = Clean(parsed.Location),
                LinkedInUrl = Clean(parsed.LinkedInUrl),
                GitHubUrl = Clean(parsed.GitHubUrl),
                PortfolioUrl = Clean(parsed.PortfolioUrl),
                Summary = Clean(parsed.Summary),
                IsBaseCv = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Cvs.Add(cv);
            await _context.SaveChangesAsync();

            AddImportedSections(cv.Id, parsed);
            await _context.SaveChangesAsync();

            var importedCv = await _context.Cvs
                .Include(x => x.Skills)
                .Include(x => x.Experiences)
                .Include(x => x.Educations)
                .Include(x => x.Projects)
                .Include(x => x.Languages)
                .Include(x => x.Certifications)
                .FirstOrDefaultAsync(x => x.Id == cv.Id);

            if (importedCv == null)
                return BadRequest("The imported CV could not be loaded.");

            return Ok(ToCvResponse(importedCv));
        }

        private void AddImportedSections(int cvId, ParsedCvImportDto parsed)
        {
            foreach (var skillName in CleanList(parsed.Skills, 40))
            {
                _context.CvSkills.Add(new CvSkill { CvId = cvId, Name = skillName });
            }

            foreach (var experience in (parsed.Experiences ?? new List<ParsedExperienceDto>()).Take(12))
            {
                if (string.IsNullOrWhiteSpace(experience.JobTitle) && string.IsNullOrWhiteSpace(experience.Company) && string.IsNullOrWhiteSpace(experience.Description))
                    continue;

                _context.CvExperiences.Add(new CvExperience
                {
                    CvId = cvId,
                    JobTitle = Clean(experience.JobTitle),
                    Company = Clean(experience.Company),
                    Description = Clean(experience.Description),
                    StartDate = experience.StartDate,
                    EndDate = experience.EndDate,
                    IsCurrent = experience.IsCurrent
                });
            }

            foreach (var education in (parsed.Educations ?? new List<ParsedEducationDto>()).Take(10))
            {
                if (string.IsNullOrWhiteSpace(education.Institution) && string.IsNullOrWhiteSpace(education.Degree) && string.IsNullOrWhiteSpace(education.Description))
                    continue;

                _context.CvEducations.Add(new CvEducation
                {
                    CvId = cvId,
                    Institution = Clean(education.Institution),
                    Degree = Clean(education.Degree),
                    Description = Clean(education.Description),
                    StartDate = education.StartDate,
                    EndDate = education.EndDate
                });
            }

            foreach (var project in (parsed.Projects ?? new List<ParsedProjectDto>()).Take(12))
            {
                if (string.IsNullOrWhiteSpace(project.Title) && string.IsNullOrWhiteSpace(project.Description))
                    continue;

                _context.CvProjects.Add(new CvProject
                {
                    CvId = cvId,
                    Title = Clean(project.Title),
                    Description = Clean(project.Description),
                    Technologies = Clean(project.Technologies),
                    ProjectUrl = Clean(project.ProjectUrl),
                    GitHubUrl = Clean(project.GitHubUrl)
                });
            }

            foreach (var language in (parsed.Languages ?? new List<ParsedLanguageDto>()).Take(12))
            {
                if (string.IsNullOrWhiteSpace(language.Name))
                    continue;

                _context.CvLanguages.Add(new CvLanguage
                {
                    CvId = cvId,
                    Name = Clean(language.Name),
                    Level = Clean(language.Level)
                });
            }

            foreach (var certification in (parsed.Certifications ?? new List<ParsedCertificationDto>()).Take(20))
            {
                if (string.IsNullOrWhiteSpace(certification.Name))
                    continue;

                _context.CvCertifications.Add(new CvCertification
                {
                    CvId = cvId,
                    Name = Clean(certification.Name),
                    Issuer = Clean(certification.Issuer),
                    Date = certification.Date,
                    Url = Clean(certification.Url)
                });
            }
        }

        private static async Task<bool> LooksLikePdfAsync(IFormFile file)
        {
            try
            {
                await using var stream = file.OpenReadStream();

                var buffer = new byte[5];
                var read = await stream.ReadAsync(buffer, 0, buffer.Length);

                if (read < 5)
                    return false;

                var signature = Encoding.ASCII.GetString(buffer);
                return signature == "%PDF-";
            }
            catch
            {
                return false;
            }
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

        private static List<string> CleanList(IEnumerable<string>? values, int maxCount)
        {
            return (values ?? Enumerable.Empty<string>())
                .Select(Clean)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(maxCount)
                .ToList();
        }

        private static string Clean(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            return value.Trim();
        }

        private static object ToCvResponse(Cv cv)
        {
            return new
            {
                cv.Id,
                cv.UserId,
                cv.Title,
                cv.Language,
                cv.Summary,
                cv.FullName,
                cv.Email,
                cv.Phone,
                cv.Location,
                cv.LinkedInUrl,
                cv.GitHubUrl,
                cv.PortfolioUrl,
                cv.TemplateName,
                cv.IsBaseCv,
                cv.ParentCvId,
                cv.TargetJobId,
                cv.CreatedAt,
                cv.PhotoFileName,
                HasPhoto = !string.IsNullOrWhiteSpace(cv.PhotoFileName),
                Skills = cv.Skills.Select(skill => new { skill.Id, skill.CvId, skill.Name }).ToList(),
                Educations = cv.Educations.Select(education => new
                {
                    education.Id,
                    education.CvId,
                    education.Institution,
                    education.Degree,
                    education.Description,
                    education.StartDate,
                    education.EndDate
                }).ToList(),
                Experiences = cv.Experiences.Select(experience => new
                {
                    experience.Id,
                    experience.CvId,
                    experience.JobTitle,
                    experience.Company,
                    experience.Description,
                    experience.StartDate,
                    experience.EndDate,
                    experience.IsCurrent
                }).ToList(),
                Projects = cv.Projects.Select(project => new
                {
                    project.Id,
                    project.CvId,
                    project.Title,
                    project.Description,
                    project.Technologies,
                    project.ProjectUrl,
                    project.GitHubUrl
                }).ToList(),
                Languages = cv.Languages.Select(language => new { language.Id, language.CvId, language.Name, language.Level }).ToList(),
                Certifications = cv.Certifications.Select(certification => new
                {
                    certification.Id,
                    certification.CvId,
                    certification.Name,
                    certification.Issuer,
                    certification.Date,
                    certification.Url
                }).ToList()
            };
        }
    }
}
