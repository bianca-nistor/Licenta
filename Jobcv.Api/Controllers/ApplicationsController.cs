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

        [HttpPost]
        public async Task<IActionResult> CreateApplication(CreateApplicationDto dto)
        {
            var userExists = await _context.Users.AnyAsync(x => x.Id == dto.UserId);
            if (!userExists)
            {
                return NotFound("Utilizatorul nu există.");
            }

            var jobExists = await _context.Jobs.AnyAsync(x => x.Id == dto.JobId);
            if (!jobExists)
            {
                return NotFound("Jobul nu există.");
            }

            if (dto.CvId.HasValue)
            {
                var cvExists = await _context.Cvs.AnyAsync(x => x.Id == dto.CvId.Value && x.UserId == dto.UserId);
                if (!cvExists)
                {
                    return BadRequest("CV-ul nu există sau nu aparține utilizatorului.");
                }
            }

            var existingApplication = await _context.Applications
                .FirstOrDefaultAsync(x => x.UserId == dto.UserId && x.JobId == dto.JobId);

            if (existingApplication != null)
            {
                return BadRequest("Acest job este deja salvat sau aplicat de utilizator.");
            }

            var application = new Application
            {
                UserId = dto.UserId,
                JobId = dto.JobId,
                CvId = dto.CvId,
                Status = dto.Status,
                AppliedAt = dto.AppliedAt,
                Notes = dto.Notes
            };

            _context.Applications.Add(application);
            await _context.SaveChangesAsync();

            return Ok(application);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserApplications(int userId)
        {
            var applications = await _context.Applications
                .Include(x => x.Job)
                .Include(x => x.Cv)
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return Ok(applications);
        }

        [HttpGet("{applicationId}")]
        public async Task<IActionResult> GetApplicationById(int applicationId)
        {
            var application = await _context.Applications
                .Include(x => x.Job)
                .Include(x => x.Cv)
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == applicationId);

            if (application == null)
            {
                return NotFound("Aplicația nu a fost găsită.");
            }

            return Ok(application);
        }

        [HttpPut("{applicationId}/status")]
        public async Task<IActionResult> UpdateStatus(int applicationId, UpdateApplicationStatusDto dto)
        {
            var application = await _context.Applications.FindAsync(applicationId);

            if (application == null)
            {
                return NotFound("Aplicația nu a fost găsită.");
            }

            application.Status = dto.Status;
            application.AppliedAt = dto.AppliedAt;
            application.InterviewDate = dto.InterviewDate;
            application.Notes = dto.Notes;
            application.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(application);
        }

        [HttpPut("{applicationId}/interview-feedback")]
        public async Task<IActionResult> UpdateInterviewFeedback(int applicationId, UpdateInterviewFeedbackDto dto)
        {
            var application = await _context.Applications.FindAsync(applicationId);

            if (application == null)
            {
                return NotFound("Aplicația nu a fost găsită.");
            }

            application.InterviewDate = dto.InterviewDate;
            application.InterviewNotes = dto.InterviewNotes;
            application.QuestionsAsked = dto.QuestionsAsked;
            application.InterviewRating = dto.InterviewRating;
            application.InterviewDifficulty = dto.InterviewDifficulty;
            application.WouldApplyAgain = dto.WouldApplyAgain;
            application.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(application);
        }

        [HttpDelete("{applicationId}")]
        public async Task<IActionResult> DeleteApplication(int applicationId)
        {
            var application = await _context.Applications.FindAsync(applicationId);

            if (application == null)
            {
                return NotFound("Aplicația nu a fost găsită.");
            }

            _context.Applications.Remove(application);
            await _context.SaveChangesAsync();

            return Ok("Aplicația a fost ștearsă cu succes.");
        }
    }
}