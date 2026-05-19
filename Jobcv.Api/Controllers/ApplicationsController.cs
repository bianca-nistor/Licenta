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

            return Ok(new
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
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateApplication(CreateJobApplicationDto dto)
        {
            var userExists = await _context.Users.AnyAsync(x => x.Id == dto.UserId);

            if (!userExists)
            {
                return NotFound("User was not found.");
            }

            if (dto.CvId.HasValue)
            {
                var cvExists = await _context.Cvs.AnyAsync(x => x.Id == dto.CvId.Value && x.UserId == dto.UserId);

                if (!cvExists)
                {
                    return BadRequest("The selected CV was not found for this user.");
                }
            }

            if (string.IsNullOrWhiteSpace(dto.JobTitle))
            {
                return BadRequest("Job title is required.");
            }

            if (string.IsNullOrWhiteSpace(dto.Company))
            {
                return BadRequest("Company is required.");
            }

            var allowedStatuses = new[]
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

            var status = string.IsNullOrWhiteSpace(dto.Status)
                ? "Saved"
                : dto.Status.Trim();

            if (!allowedStatuses.Contains(status))
            {
                status = "Saved";
            }

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
                AppliedAt = dto.AppliedAt,
                InterviewAt = dto.InterviewAt,
                Notes = dto.Notes?.Trim() ?? string.Empty,
                InterviewNotes = dto.InterviewNotes?.Trim() ?? string.Empty,
                SalaryRange = dto.SalaryRange?.Trim() ?? string.Empty,
                ContactPerson = dto.ContactPerson?.Trim() ?? string.Empty,
                CreatedAt = DateTime.UtcNow
            };

            _context.JobApplications.Add(application);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                application.Id,
                application.UserId,
                application.CvId,
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
            });
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
    }
}