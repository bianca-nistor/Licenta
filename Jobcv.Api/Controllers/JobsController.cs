using JobCv.Api.Data;
using JobCv.Api.Dtos;
using JobCv.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobCv.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public JobsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateJob(CreateJobDto dto)
        {
            var job = new Job
            {
                Title = dto.Title,
                Company = dto.Company,
                Location = dto.Location,
                Description = dto.Description,
                Requirements = dto.Requirements,
                WorkMode = dto.WorkMode,
                EmploymentType = dto.EmploymentType,
                Source = dto.Source,
                ExternalUrl = dto.ExternalUrl,
                PublishedAt = dto.PublishedAt
            };

            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();

            return Ok(job);
        }

        [HttpGet]
        public async Task<IActionResult> GetJobs()
        {
            var jobs = await _context.Jobs
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return Ok(jobs);
        }

        [HttpGet("{jobId}")]
        public async Task<IActionResult> GetJobById(int jobId)
        {
            var job = await _context.Jobs.FindAsync(jobId);

            if (job == null)
            {
                return NotFound("Jobul nu a fost găsit.");
            }

            return Ok(job);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchJobs(
            string? keyword,
            string? location,
            string? workMode)
        {
            var query = _context.Jobs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(x =>
                    x.Title.Contains(keyword) ||
                    x.Company.Contains(keyword) ||
                    x.Description.Contains(keyword) ||
                    x.Requirements.Contains(keyword));
            }

            if (!string.IsNullOrWhiteSpace(location))
            {
                query = query.Where(x => x.Location.Contains(location));
            }

            if (!string.IsNullOrWhiteSpace(workMode))
            {
                query = query.Where(x => x.WorkMode == workMode);
            }

            var jobs = await query
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return Ok(jobs);
        }

        [HttpPut("{jobId}")]
        public async Task<IActionResult> UpdateJob(int jobId, UpdateJobDto dto)
        {
            var job = await _context.Jobs.FindAsync(jobId);

            if (job == null)
            {
                return NotFound("Jobul nu a fost găsit.");
            }

            job.Title = dto.Title;
            job.Company = dto.Company;
            job.Location = dto.Location;
            job.Description = dto.Description;
            job.Requirements = dto.Requirements;
            job.WorkMode = dto.WorkMode;
            job.EmploymentType = dto.EmploymentType;
            job.Source = dto.Source;
            job.ExternalUrl = dto.ExternalUrl;
            job.PublishedAt = dto.PublishedAt;

            await _context.SaveChangesAsync();

            return Ok(job);
        }

        [HttpDelete("{jobId}")]
        public async Task<IActionResult> DeleteJob(int jobId)
        {
            var job = await _context.Jobs.FindAsync(jobId);

            if (job == null)
            {
                return NotFound("Jobul nu a fost găsit.");
            }

            _context.Jobs.Remove(job);
            await _context.SaveChangesAsync();

            return Ok("Jobul a fost șters cu succes.");
        }
    }
}