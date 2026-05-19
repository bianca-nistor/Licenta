using JobCv.Api.Data;
using JobCv.Api.Dtos;
using JobCv.Api.Models;
using JobCv.Api.Pdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;

namespace JobCv.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CvsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CvsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateCv(CreateCvDto dto)
        {
            var userExists = await _context.Users.AnyAsync(x => x.Id == dto.UserId);

            if (!userExists)
                return NotFound("User was not found.");

            if (string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest("CV title is required.");

            var cv = new Cv
            {
                UserId = dto.UserId,
                Title = dto.Title.Trim(),
                Language = string.IsNullOrWhiteSpace(dto.Language) ? "en" : dto.Language.Trim(),
                Summary = dto.Summary?.Trim() ?? string.Empty,
                FullName = dto.FullName?.Trim() ?? string.Empty,
                Email = dto.Email?.Trim() ?? string.Empty,
                Phone = dto.Phone?.Trim() ?? string.Empty,
                Location = dto.Location?.Trim() ?? string.Empty,
                LinkedInUrl = dto.LinkedInUrl?.Trim() ?? string.Empty,
                GitHubUrl = dto.GitHubUrl?.Trim() ?? string.Empty,
                PortfolioUrl = dto.PortfolioUrl?.Trim() ?? string.Empty,
                TemplateName = string.IsNullOrWhiteSpace(dto.TemplateName) ? "modern-blue" : dto.TemplateName.Trim(),
                IsBaseCv = dto.IsBaseCv,
                ParentCvId = dto.ParentCvId,
                TargetJobId = dto.TargetJobId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Cvs.Add(cv);
            await _context.SaveChangesAsync();

            return Ok(ToCvResponse(cv));
        }

        [HttpGet("user/{userId:int}")]
        public async Task<IActionResult> GetUserCvs(int userId)
        {
            var cvs = await _context.Cvs
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .Select(cv => new
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
                    HasPhoto = !string.IsNullOrWhiteSpace(cv.PhotoFileName)
                })
                .ToListAsync();

            return Ok(cvs);
        }

        [HttpGet("{cvId:int}")]
        public async Task<IActionResult> GetCvById(int cvId)
        {
            var cv = await _context.Cvs
                .Include(x => x.Skills)
                .Include(x => x.Experiences)
                .Include(x => x.Educations)
                .Include(x => x.Projects)
                .Include(x => x.Languages)
                .Include(x => x.Certifications)
                .FirstOrDefaultAsync(x => x.Id == cvId);

            if (cv == null)
                return NotFound("CV was not found.");

            return Ok(ToCvResponse(cv));
        }

        [HttpPut("{cvId:int}")]
        public async Task<IActionResult> UpdateCv(int cvId, UpdateCvDto dto)
        {
            var cv = await _context.Cvs.FindAsync(cvId);

            if (cv == null)
                return NotFound("CV was not found.");

            if (string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest("CV title is required.");

            cv.Title = dto.Title.Trim();
            cv.Language = string.IsNullOrWhiteSpace(dto.Language) ? "en" : dto.Language.Trim();
            cv.Summary = dto.Summary?.Trim() ?? string.Empty;
            cv.TemplateName = string.IsNullOrWhiteSpace(dto.TemplateName) ? "modern-blue" : dto.TemplateName.Trim();
            cv.IsBaseCv = dto.IsBaseCv;
            cv.TargetJobId = dto.TargetJobId;

            await _context.SaveChangesAsync();

            return Ok(ToCvResponse(cv));
        }

        [HttpPut("{cvId:int}/personal-info")]
        public async Task<IActionResult> UpdatePersonalInfo(int cvId, UpdateCvPersonalInfoDto dto)
        {
            var cv = await _context.Cvs.FindAsync(cvId);

            if (cv == null)
                return NotFound("CV was not found.");

            cv.FullName = dto.FullName?.Trim() ?? string.Empty;
            cv.Email = dto.Email?.Trim() ?? string.Empty;
            cv.Phone = dto.Phone?.Trim() ?? string.Empty;
            cv.Location = dto.Location?.Trim() ?? string.Empty;
            cv.LinkedInUrl = dto.LinkedInUrl?.Trim() ?? string.Empty;
            cv.GitHubUrl = dto.GitHubUrl?.Trim() ?? string.Empty;
            cv.PortfolioUrl = dto.PortfolioUrl?.Trim() ?? string.Empty;

            await _context.SaveChangesAsync();

            return Ok(ToCvResponse(cv));
        }

        [HttpDelete("{cvId:int}")]
        public async Task<IActionResult> DeleteCv(int cvId)
        {
            var cv = await _context.Cvs.FindAsync(cvId);

            if (cv == null)
                return NotFound("CV was not found.");

            if (!string.IsNullOrWhiteSpace(cv.PhotoPath) && System.IO.File.Exists(cv.PhotoPath))
            {
                System.IO.File.Delete(cv.PhotoPath);
            }

            _context.Cvs.Remove(cv);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "CV deleted successfully." });
        }

        [HttpPost("{cvId:int}/skills")]
        public async Task<IActionResult> AddSkill(int cvId, AddSkillDto dto)
        {
            var cvExists = await _context.Cvs.AnyAsync(x => x.Id == cvId);

            if (!cvExists)
                return NotFound("CV was not found.");

            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest("Skill name is required.");

            var skill = new CvSkill
            {
                CvId = cvId,
                Name = dto.Name.Trim()
            };

            _context.CvSkills.Add(skill);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                skill.Id,
                skill.CvId,
                skill.Name
            });
        }

        [HttpPut("skills/{skillId:int}")]
        public async Task<IActionResult> UpdateSkill(int skillId, UpdateSkillDto dto)
        {
            var skill = await _context.CvSkills.FindAsync(skillId);

            if (skill == null)
                return NotFound("Skill was not found.");

            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest("Skill name is required.");

            skill.Name = dto.Name.Trim();

            await _context.SaveChangesAsync();

            return Ok(new
            {
                skill.Id,
                skill.CvId,
                skill.Name
            });
        }

        [HttpDelete("skills/{skillId:int}")]
        public async Task<IActionResult> DeleteSkill(int skillId)
        {
            var skill = await _context.CvSkills.FindAsync(skillId);

            if (skill == null)
                return NotFound("Skill was not found.");

            _context.CvSkills.Remove(skill);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Skill deleted successfully." });
        }

        [HttpPost("{cvId:int}/educations")]
        public async Task<IActionResult> AddEducation(int cvId, AddEducationDto dto)
        {
            var cvExists = await _context.Cvs.AnyAsync(x => x.Id == cvId);

            if (!cvExists)
                return NotFound("CV was not found.");

            if (string.IsNullOrWhiteSpace(dto.Institution))
                return BadRequest("Institution is required.");

            if (string.IsNullOrWhiteSpace(dto.Degree))
                return BadRequest("Degree is required.");

            var education = new CvEducation
            {
                CvId = cvId,
                Institution = dto.Institution.Trim(),
                Degree = dto.Degree.Trim(),
                Description = dto.Description?.Trim() ?? string.Empty,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate
            };

            _context.CvEducations.Add(education);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                education.Id,
                education.CvId,
                education.Institution,
                education.Degree,
                education.Description,
                education.StartDate,
                education.EndDate
            });
        }

        [HttpPut("educations/{educationId:int}")]
        public async Task<IActionResult> UpdateEducation(int educationId, UpdateEducationDto dto)
        {
            var education = await _context.CvEducations.FindAsync(educationId);

            if (education == null)
                return NotFound("Education entry was not found.");

            if (string.IsNullOrWhiteSpace(dto.Institution))
                return BadRequest("Institution is required.");

            if (string.IsNullOrWhiteSpace(dto.Degree))
                return BadRequest("Degree is required.");

            education.Institution = dto.Institution.Trim();
            education.Degree = dto.Degree.Trim();
            education.Description = dto.Description?.Trim() ?? string.Empty;
            education.StartDate = dto.StartDate;
            education.EndDate = dto.EndDate;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                education.Id,
                education.CvId,
                education.Institution,
                education.Degree,
                education.Description,
                education.StartDate,
                education.EndDate
            });
        }

        [HttpDelete("educations/{educationId:int}")]
        public async Task<IActionResult> DeleteEducation(int educationId)
        {
            var education = await _context.CvEducations.FindAsync(educationId);

            if (education == null)
                return NotFound("Education entry was not found.");

            _context.CvEducations.Remove(education);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Education entry deleted successfully." });
        }

        [HttpPost("{cvId:int}/experiences")]
        public async Task<IActionResult> AddExperience(int cvId, AddExperienceDto dto)
        {
            var cvExists = await _context.Cvs.AnyAsync(x => x.Id == cvId);

            if (!cvExists)
                return NotFound("CV was not found.");

            if (string.IsNullOrWhiteSpace(dto.JobTitle))
                return BadRequest("Job title is required.");

            if (string.IsNullOrWhiteSpace(dto.Company))
                return BadRequest("Company is required.");

            var experience = new CvExperience
            {
                CvId = cvId,
                JobTitle = dto.JobTitle.Trim(),
                Company = dto.Company.Trim(),
                Description = dto.Description?.Trim() ?? string.Empty,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                IsCurrent = dto.IsCurrent
            };

            _context.CvExperiences.Add(experience);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                experience.Id,
                experience.CvId,
                experience.JobTitle,
                experience.Company,
                experience.Description,
                experience.StartDate,
                experience.EndDate,
                experience.IsCurrent
            });
        }

        [HttpPut("experiences/{experienceId:int}")]
        public async Task<IActionResult> UpdateExperience(int experienceId, UpdateExperienceDto dto)
        {
            var experience = await _context.CvExperiences.FindAsync(experienceId);

            if (experience == null)
                return NotFound("Experience entry was not found.");

            if (string.IsNullOrWhiteSpace(dto.JobTitle))
                return BadRequest("Job title is required.");

            if (string.IsNullOrWhiteSpace(dto.Company))
                return BadRequest("Company is required.");

            experience.JobTitle = dto.JobTitle.Trim();
            experience.Company = dto.Company.Trim();
            experience.Description = dto.Description?.Trim() ?? string.Empty;
            experience.StartDate = dto.StartDate;
            experience.EndDate = dto.EndDate;
            experience.IsCurrent = dto.IsCurrent;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                experience.Id,
                experience.CvId,
                experience.JobTitle,
                experience.Company,
                experience.Description,
                experience.StartDate,
                experience.EndDate,
                experience.IsCurrent
            });
        }

        [HttpDelete("experiences/{experienceId:int}")]
        public async Task<IActionResult> DeleteExperience(int experienceId)
        {
            var experience = await _context.CvExperiences.FindAsync(experienceId);

            if (experience == null)
                return NotFound("Experience entry was not found.");

            _context.CvExperiences.Remove(experience);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Experience entry deleted successfully." });
        }

        [HttpPost("{cvId:int}/projects")]
        public async Task<IActionResult> AddProject(int cvId, AddProjectDto dto)
        {
            var cvExists = await _context.Cvs.AnyAsync(x => x.Id == cvId);

            if (!cvExists)
                return NotFound("CV was not found.");

            if (string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest("Project title is required.");

            var project = new CvProject
            {
                CvId = cvId,
                Title = dto.Title.Trim(),
                Description = dto.Description?.Trim() ?? string.Empty,
                Technologies = dto.Technologies?.Trim() ?? string.Empty,
                ProjectUrl = dto.ProjectUrl?.Trim() ?? string.Empty,
                GitHubUrl = dto.GitHubUrl?.Trim() ?? string.Empty
            };

            _context.CvProjects.Add(project);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                project.Id,
                project.CvId,
                project.Title,
                project.Description,
                project.Technologies,
                project.ProjectUrl,
                project.GitHubUrl
            });
        }

        [HttpPut("projects/{projectId:int}")]
        public async Task<IActionResult> UpdateProject(int projectId, UpdateProjectDto dto)
        {
            var project = await _context.CvProjects.FindAsync(projectId);

            if (project == null)
                return NotFound("Project was not found.");

            if (string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest("Project title is required.");

            project.Title = dto.Title.Trim();
            project.Description = dto.Description?.Trim() ?? string.Empty;
            project.Technologies = dto.Technologies?.Trim() ?? string.Empty;
            project.ProjectUrl = dto.ProjectUrl?.Trim() ?? string.Empty;
            project.GitHubUrl = dto.GitHubUrl?.Trim() ?? string.Empty;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                project.Id,
                project.CvId,
                project.Title,
                project.Description,
                project.Technologies,
                project.ProjectUrl,
                project.GitHubUrl
            });
        }

        [HttpDelete("projects/{projectId:int}")]
        public async Task<IActionResult> DeleteProject(int projectId)
        {
            var project = await _context.CvProjects.FindAsync(projectId);

            if (project == null)
                return NotFound("Project was not found.");

            _context.CvProjects.Remove(project);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Project deleted successfully." });
        }

        [HttpPost("{cvId:int}/languages")]
        public async Task<IActionResult> AddLanguage(int cvId, AddLanguageDto dto)
        {
            var cvExists = await _context.Cvs.AnyAsync(x => x.Id == cvId);

            if (!cvExists)
                return NotFound("CV was not found.");

            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest("Language name is required.");

            if (string.IsNullOrWhiteSpace(dto.Level))
                return BadRequest("Language level is required.");

            var language = new CvLanguage
            {
                CvId = cvId,
                Name = dto.Name.Trim(),
                Level = dto.Level.Trim()
            };

            _context.CvLanguages.Add(language);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                language.Id,
                language.CvId,
                language.Name,
                language.Level
            });
        }

        [HttpPut("languages/{languageId:int}")]
        public async Task<IActionResult> UpdateLanguage(int languageId, UpdateLanguageDto dto)
        {
            var language = await _context.CvLanguages.FindAsync(languageId);

            if (language == null)
                return NotFound("Language was not found.");

            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest("Language name is required.");

            if (string.IsNullOrWhiteSpace(dto.Level))
                return BadRequest("Language level is required.");

            language.Name = dto.Name.Trim();
            language.Level = dto.Level.Trim();

            await _context.SaveChangesAsync();

            return Ok(new
            {
                language.Id,
                language.CvId,
                language.Name,
                language.Level
            });
        }

        [HttpDelete("languages/{languageId:int}")]
        public async Task<IActionResult> DeleteLanguage(int languageId)
        {
            var language = await _context.CvLanguages.FindAsync(languageId);

            if (language == null)
                return NotFound("Language was not found.");

            _context.CvLanguages.Remove(language);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Language deleted successfully." });
        }

        [HttpPost("{cvId:int}/certifications")]
        public async Task<IActionResult> AddCertification(int cvId, AddCertificationDto dto)
        {
            var cvExists = await _context.Cvs.AnyAsync(x => x.Id == cvId);

            if (!cvExists)
                return NotFound("CV was not found.");

            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest("Certification name is required.");

            var certification = new CvCertification
            {
                CvId = cvId,
                Name = dto.Name.Trim(),
                Issuer = dto.Issuer?.Trim() ?? string.Empty,
                Date = dto.Date,
                Url = dto.Url?.Trim() ?? string.Empty
            };

            _context.CvCertifications.Add(certification);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                certification.Id,
                certification.CvId,
                certification.Name,
                certification.Issuer,
                certification.Date,
                certification.Url
            });
        }

        [HttpPut("certifications/{certificationId:int}")]
        public async Task<IActionResult> UpdateCertification(int certificationId, UpdateCertificationDto dto)
        {
            var certification = await _context.CvCertifications.FindAsync(certificationId);

            if (certification == null)
                return NotFound("Certification was not found.");

            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest("Certification name is required.");

            certification.Name = dto.Name.Trim();
            certification.Issuer = dto.Issuer?.Trim() ?? string.Empty;
            certification.Date = dto.Date;
            certification.Url = dto.Url?.Trim() ?? string.Empty;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                certification.Id,
                certification.CvId,
                certification.Name,
                certification.Issuer,
                certification.Date,
                certification.Url
            });
        }

        [HttpDelete("certifications/{certificationId:int}")]
        public async Task<IActionResult> DeleteCertification(int certificationId)
        {
            var certification = await _context.CvCertifications.FindAsync(certificationId);

            if (certification == null)
                return NotFound("Certification was not found.");

            _context.CvCertifications.Remove(certification);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Certification deleted successfully." });
        }

        [HttpPost("{cvId:int}/duplicate")]
        public async Task<IActionResult> DuplicateCv(int cvId, DuplicateCvDto dto)
        {
            var sourceCv = await _context.Cvs
                .Include(x => x.Skills)
                .Include(x => x.Experiences)
                .Include(x => x.Educations)
                .Include(x => x.Projects)
                .Include(x => x.Languages)
                .Include(x => x.Certifications)
                .FirstOrDefaultAsync(x => x.Id == cvId);

            if (sourceCv == null)
                return NotFound("Source CV was not found.");

            if (string.IsNullOrWhiteSpace(dto.NewTitle))
                return BadRequest("New CV title is required.");

            if (dto.TargetJobId.HasValue)
            {
                var jobExists = await _context.Jobs.AnyAsync(x => x.Id == dto.TargetJobId.Value);

                if (!jobExists)
                    return BadRequest("Target job was not found.");
            }

            var newCv = new Cv
            {
                UserId = sourceCv.UserId,
                Title = dto.NewTitle.Trim(),
                Language = sourceCv.Language,
                Summary = sourceCv.Summary,
                FullName = sourceCv.FullName,
                Email = sourceCv.Email,
                Phone = sourceCv.Phone,
                Location = sourceCv.Location,
                LinkedInUrl = sourceCv.LinkedInUrl,
                GitHubUrl = sourceCv.GitHubUrl,
                PortfolioUrl = sourceCv.PortfolioUrl,
                TemplateName = string.IsNullOrWhiteSpace(dto.TemplateName)
                    ? sourceCv.TemplateName
                    : dto.TemplateName.Trim(),
                IsBaseCv = false,
                ParentCvId = sourceCv.Id,
                TargetJobId = dto.TargetJobId,
                CreatedAt = DateTime.UtcNow,

                PhotoFileName = sourceCv.PhotoFileName,
                PhotoPath = sourceCv.PhotoPath,
                PhotoContentType = sourceCv.PhotoContentType
            };

            _context.Cvs.Add(newCv);
            await _context.SaveChangesAsync();

            foreach (var skill in sourceCv.Skills)
                _context.CvSkills.Add(new CvSkill { CvId = newCv.Id, Name = skill.Name });

            foreach (var experience in sourceCv.Experiences)
            {
                _context.CvExperiences.Add(new CvExperience
                {
                    CvId = newCv.Id,
                    JobTitle = experience.JobTitle,
                    Company = experience.Company,
                    Description = experience.Description,
                    StartDate = experience.StartDate,
                    EndDate = experience.EndDate,
                    IsCurrent = experience.IsCurrent
                });
            }

            foreach (var education in sourceCv.Educations)
            {
                _context.CvEducations.Add(new CvEducation
                {
                    CvId = newCv.Id,
                    Institution = education.Institution,
                    Degree = education.Degree,
                    Description = education.Description,
                    StartDate = education.StartDate,
                    EndDate = education.EndDate
                });
            }

            foreach (var project in sourceCv.Projects)
            {
                _context.CvProjects.Add(new CvProject
                {
                    CvId = newCv.Id,
                    Title = project.Title,
                    Description = project.Description,
                    Technologies = project.Technologies,
                    ProjectUrl = project.ProjectUrl,
                    GitHubUrl = project.GitHubUrl
                });
            }

            foreach (var language in sourceCv.Languages)
                _context.CvLanguages.Add(new CvLanguage { CvId = newCv.Id, Name = language.Name, Level = language.Level });

            foreach (var certification in sourceCv.Certifications)
            {
                _context.CvCertifications.Add(new CvCertification
                {
                    CvId = newCv.Id,
                    Name = certification.Name,
                    Issuer = certification.Issuer,
                    Date = certification.Date,
                    Url = certification.Url
                });
            }

            await _context.SaveChangesAsync();

            var duplicatedCv = await _context.Cvs
                .Include(x => x.Skills)
                .Include(x => x.Experiences)
                .Include(x => x.Educations)
                .Include(x => x.Projects)
                .Include(x => x.Languages)
                .Include(x => x.Certifications)
                .FirstOrDefaultAsync(x => x.Id == newCv.Id);

            if (duplicatedCv == null)
                return BadRequest("The duplicated CV could not be loaded.");

            return Ok(ToCvResponse(duplicatedCv));
        }

        [HttpGet("templates")]
        public IActionResult GetCvTemplates()
        {
            var templates = new[]
            {
                new
                {
                    Id = "modern-blue",
                    Name = "Modern Blue",
                    Description = "A clean professional template with navy and blue accents."
                },
                new
                {
                    Id = "classic",
                    Name = "Classic",
                    Description = "A simple traditional CV layout suitable for formal applications."
                },
                new
                {
                    Id = "minimal-green",
                    Name = "Minimal Green",
                    Description = "A minimalist layout with subtle green accents."
                }
            };

            return Ok(templates);
        }

        [HttpGet("{cvId:int}/export-pdf")]
        public async Task<IActionResult> ExportCvPdf(int cvId)
        {
            var cv = await _context.Cvs
                .Include(x => x.Skills)
                .Include(x => x.Experiences)
                .Include(x => x.Educations)
                .Include(x => x.Projects)
                .Include(x => x.Languages)
                .Include(x => x.Certifications)
                .FirstOrDefaultAsync(x => x.Id == cvId);

            if (cv == null)
                return NotFound("CV was not found.");

            var document = new CvPdfDocument(cv);
            var pdfBytes = document.GeneratePdf();

            var safeTitle = string.IsNullOrWhiteSpace(cv.Title)
                ? "cv"
                : string.Join("_", cv.Title.Split(Path.GetInvalidFileNameChars()));

            var fileName = $"{safeTitle}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }

        [HttpGet("{cvId:int}/preview-pdf")]
        public async Task<IActionResult> PreviewCvPdf(int cvId)
        {
            var cv = await _context.Cvs
                .Include(x => x.Skills)
                .Include(x => x.Experiences)
                .Include(x => x.Educations)
                .Include(x => x.Projects)
                .Include(x => x.Languages)
                .Include(x => x.Certifications)
                .FirstOrDefaultAsync(x => x.Id == cvId);

            if (cv == null)
                return NotFound("CV was not found.");

            var document = new CvPdfDocument(cv);
            var pdfBytes = document.GeneratePdf();

            return File(pdfBytes, "application/pdf");
        }

        [HttpPost("{cvId:int}/photo")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(5_000_000)]
        public async Task<IActionResult> UploadCvPhoto(int cvId, IFormFile file)
        {
            var cv = await _context.Cvs.FindAsync(cvId);

            if (cv == null)
                return NotFound("CV was not found.");

            if (file == null || file.Length == 0)
                return BadRequest("No image was uploaded.");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

            if (!allowedExtensions.Contains(extension))
                return BadRequest("Only JPG, JPEG, PNG and WEBP images are allowed.");

            var maxFileSize = 5 * 1024 * 1024;

            if (file.Length > maxFileSize)
                return BadRequest("Image size cannot exceed 5 MB.");

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "CvPhotos");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            if (!string.IsNullOrWhiteSpace(cv.PhotoPath) && System.IO.File.Exists(cv.PhotoPath))
                System.IO.File.Delete(cv.PhotoPath);

            var storedFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, storedFileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            cv.PhotoFileName = storedFileName;
            cv.PhotoPath = filePath;
            cv.PhotoContentType = string.IsNullOrWhiteSpace(file.ContentType)
                ? "image/jpeg"
                : file.ContentType;

            await _context.SaveChangesAsync();

            var updatedCv = await _context.Cvs
                .Include(x => x.Skills)
                .Include(x => x.Experiences)
                .Include(x => x.Educations)
                .Include(x => x.Projects)
                .Include(x => x.Languages)
                .Include(x => x.Certifications)
                .FirstOrDefaultAsync(x => x.Id == cvId);

            if (updatedCv == null)
                return BadRequest("The updated CV could not be loaded.");

            return Ok(ToCvResponse(updatedCv));
        }

        [HttpGet("{cvId:int}/photo")]
        public async Task<IActionResult> GetCvPhoto(int cvId)
        {
            var cv = await _context.Cvs.FindAsync(cvId);

            if (cv == null)
                return NotFound("CV was not found.");

            if (string.IsNullOrWhiteSpace(cv.PhotoPath) || !System.IO.File.Exists(cv.PhotoPath))
                return NotFound("CV photo was not found.");

            var imageBytes = await System.IO.File.ReadAllBytesAsync(cv.PhotoPath);

            var contentType = string.IsNullOrWhiteSpace(cv.PhotoContentType)
                ? "image/jpeg"
                : cv.PhotoContentType;

            return File(imageBytes, contentType);
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

                Skills = cv.Skills.Select(skill => new
                {
                    skill.Id,
                    skill.CvId,
                    skill.Name
                }).ToList(),

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

                Languages = cv.Languages.Select(language => new
                {
                    language.Id,
                    language.CvId,
                    language.Name,
                    language.Level
                }).ToList(),

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