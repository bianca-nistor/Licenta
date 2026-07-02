using JobCv.Api.Data;
using JobCv.Api.Dtos;
using JobCv.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobCv.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApplicationsController : ControllerBase
    {
        private readonly AppDbContext _context;

        private static readonly string[] AllowedStatuses =
        {
            "Saved",
            "Applied",
            "Interview Scheduled",
            "Interview Done",
            "Rejected",
            "Offer",
            "Accepted",
            "Withdrawn"
        };

        public ApplicationsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("user/{userId:int}")]
        public async Task<IActionResult> GetUserApplications(int userId)
        {
            var applications = await _context.JobApplications
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new
                {
                    x.Id,
                    x.UserId,
                    x.CvId,
                    CvTitle = x.Cv == null ? "" : x.Cv.Title,
                    x.JobExternalId,
                    x.JobTitle,
                    x.Company,
                    x.Location,
                    x.JobUrl,
                    x.Source,
                    x.Status,
                    x.AppliedAt,
                    x.InterviewAt,
                    x.Notes,
                    x.InterviewNotes,
                    x.SalaryRange,
                    x.ContactPerson,
                    x.CreatedAt
                })
                .ToListAsync();

            return Ok(applications);
        }

        [HttpGet("{applicationId:int}")]
        public async Task<IActionResult> GetApplicationById(int applicationId)
        {
            var application = await _context.JobApplications
                .Include(x => x.Cv)
                .FirstOrDefaultAsync(x => x.Id == applicationId);

            if (application == null)
            {
                return NotFound("Application was not found.");
            }

            return Ok(ToResponse(application));
        }

        [HttpPost]
        public async Task<IActionResult> CreateApplication(CreateJobApplicationDto dto)
        {
            var validationError = await ValidateApplicationInput(dto.UserId, dto.CvId, dto.JobTitle, dto.Company);

            if (validationError != null)
            {
                return validationError;
            }

            var status = NormalizeStatus(dto.Status);

            var application = new JobApplication
            {
                UserId = dto.UserId,
                CvId = dto.CvId,
                JobExternalId = dto.JobExternalId?.Trim() ?? string.Empty,
                JobTitle = dto.JobTitle.Trim(),
                Company = dto.Company.Trim(),
                Location = dto.Location?.Trim() ?? string.Empty,
                JobUrl = dto.JobUrl?.Trim() ?? string.Empty,
                Source = dto.Source?.Trim() ?? string.Empty,
                Status = status,
                AppliedAt = ResolveAppliedAt(status, dto.AppliedAt),
                InterviewAt = dto.InterviewAt,
                Notes = dto.Notes?.Trim() ?? string.Empty,
                InterviewNotes = dto.InterviewNotes?.Trim() ?? string.Empty,
                SalaryRange = dto.SalaryRange?.Trim() ?? string.Empty,
                ContactPerson = dto.ContactPerson?.Trim() ?? string.Empty,
                CreatedAt = DateTime.UtcNow
            };

            _context.JobApplications.Add(application);
            await _context.SaveChangesAsync();

            return Ok(ToResponse(application));
        }

        [HttpPut("{applicationId:int}")]
        public async Task<IActionResult> UpdateApplication(int applicationId, UpdateJobApplicationDto dto)
        {
            var application = await _context.JobApplications.FindAsync(applicationId);

            if (application == null)
            {
                return NotFound("Application was not found.");
            }

            var validationError = await ValidateApplicationInput(application.UserId, dto.CvId, dto.JobTitle, dto.Company);

            if (validationError != null)
            {
                return validationError;
            }

            var status = NormalizeStatus(dto.Status);

            application.CvId = dto.CvId;
            application.JobTitle = dto.JobTitle.Trim();
            application.Company = dto.Company.Trim();
            application.Location = dto.Location?.Trim() ?? string.Empty;
            application.JobUrl = dto.JobUrl?.Trim() ?? string.Empty;
            application.Source = dto.Source?.Trim() ?? string.Empty;
            application.Status = status;
            application.AppliedAt = ResolveAppliedAt(status, dto.AppliedAt ?? application.AppliedAt);
            application.InterviewAt = dto.InterviewAt;
            application.Notes = dto.Notes?.Trim() ?? string.Empty;
            application.InterviewNotes = dto.InterviewNotes?.Trim() ?? string.Empty;
            application.SalaryRange = dto.SalaryRange?.Trim() ?? string.Empty;
            application.ContactPerson = dto.ContactPerson?.Trim() ?? string.Empty;

            await _context.SaveChangesAsync();

            return Ok(ToResponse(application));
        }

        [HttpPatch("{applicationId:int}/status")]
        public async Task<IActionResult> UpdateApplicationStatus(int applicationId, UpdateJobApplicationDto dto)
        {
            var application = await _context.JobApplications.FindAsync(applicationId);

            if (application == null)
            {
                return NotFound("Application was not found.");
            }

            var status = NormalizeStatus(dto.Status);

            application.Status = status;
            application.AppliedAt = ResolveAppliedAt(status, dto.AppliedAt ?? application.AppliedAt);
            application.InterviewAt = dto.InterviewAt ?? application.InterviewAt;
            application.Notes = dto.Notes?.Trim() ?? application.Notes;
            application.InterviewNotes = dto.InterviewNotes?.Trim() ?? application.InterviewNotes;

            await _context.SaveChangesAsync();

            return Ok(ToResponse(application));
        }

        [HttpDelete("{applicationId:int}")]
        public async Task<IActionResult> DeleteApplication(int applicationId)
        {
            var application = await _context.JobApplications.FindAsync(applicationId);

            if (application == null)
            {
                return NotFound("Application was not found.");
            }

            _context.JobApplications.Remove(application);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Application deleted successfully." });
        }

        private async Task<IActionResult?> ValidateApplicationInput(int userId, int? cvId, string? jobTitle, string? company)
        {
            var userExists = await _context.Users.AnyAsync(x => x.Id == userId);

            if (!userExists)
            {
                return NotFound("User was not found.");
            }

            if (cvId.HasValue)
            {
                var cvExists = await _context.Cvs.AnyAsync(x => x.Id == cvId.Value && x.UserId == userId);

                if (!cvExists)
                {
                    return BadRequest("The selected CV was not found for this user.");
                }
            }

            if (string.IsNullOrWhiteSpace(jobTitle))
            {
                return BadRequest("Job title is required.");
            }

            if (string.IsNullOrWhiteSpace(company))
            {
                return BadRequest("Company is required.");
            }

            return null;
        }

        private static string NormalizeStatus(string? status)
        {
            var value = string.IsNullOrWhiteSpace(status)
                ? "Saved"
                : status.Trim();

            value = value switch
            {
                "InterviewScheduled" => "Interview Scheduled",
                "InterviewCompleted" => "Interview Done",
                "OfferReceived" => "Offer",
                _ => value
            };

            return AllowedStatuses.Contains(value) ? value : "Saved";
        }

        private static DateTime? ResolveAppliedAt(string status, DateTime? appliedAt)
        {
            if (status == "Saved")
            {
                return appliedAt;
            }

            return appliedAt ?? DateTime.UtcNow;
        }

        private static object ToResponse(JobApplication application)
        {
            return new
            {
                application.Id,
                application.UserId,
                application.CvId,
                CvTitle = application.Cv == null ? "" : application.Cv.Title,
                application.JobExternalId,
                application.JobTitle,
                application.Company,
                application.Location,
                application.JobUrl,
                application.Source,
                application.Status,
                application.AppliedAt,
                application.InterviewAt,
                application.Notes,
                application.InterviewNotes,
                application.SalaryRange,
                application.ContactPerson,
                application.CreatedAt
            };
        }
    }
}
