using JobCv.Api.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace JobCv.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobsController : ControllerBase
    {
        private static readonly List<JobSearchResultDto> MockJobs = new()
        {
            new JobSearchResultDto
            {
                ExternalId = "mock-1",
                Title = "Junior .NET Developer",
                Company = "Tech Solutions",
                Location = "Bucharest / Remote",
                Description = "Work on web and mobile applications using C#, .NET, REST APIs and SQL. Suitable for junior candidates with basic backend knowledge.",
                ApplyUrl = "https://www.linkedin.com/jobs/",
                Source = "Mock",
                Salary = "Not specified",
                PostedAt = DateTime.UtcNow.AddDays(-2)
            },
            new JobSearchResultDto
            {
                ExternalId = "mock-2",
                Title = "Frontend Developer",
                Company = "Digital Studio",
                Location = "Cluj-Napoca",
                Description = "Build responsive user interfaces using HTML, CSS, JavaScript and modern frontend frameworks.",
                ApplyUrl = "https://www.linkedin.com/jobs/",
                Source = "Mock",
                Salary = "Not specified",
                PostedAt = DateTime.UtcNow.AddDays(-4)
            },
            new JobSearchResultDto
            {
                ExternalId = "mock-3",
                Title = "Mobile App Developer",
                Company = "AppWorks",
                Location = "Remote",
                Description = "Develop mobile applications and collaborate with backend developers, designers and QA engineers.",
                ApplyUrl = "https://www.linkedin.com/jobs/",
                Source = "Mock",
                Salary = "Not specified",
                PostedAt = DateTime.UtcNow.AddDays(-1)
            },
            new JobSearchResultDto
            {
                ExternalId = "mock-4",
                Title = "Data Analyst",
                Company = "Insight Analytics",
                Location = "Bucharest",
                Description = "Analyze business data, create reports and dashboards, and support decision making with clear insights.",
                ApplyUrl = "https://www.linkedin.com/jobs/",
                Source = "Mock",
                Salary = "Not specified",
                PostedAt = DateTime.UtcNow.AddDays(-7)
            },
            new JobSearchResultDto
            {
                ExternalId = "mock-5",
                Title = "QA Tester",
                Company = "QualitySoft",
                Location = "Iasi / Remote",
                Description = "Test web and mobile applications, write test cases, report bugs and collaborate with developers.",
                ApplyUrl = "https://www.linkedin.com/jobs/",
                Source = "Mock",
                Salary = "Not specified",
                PostedAt = DateTime.UtcNow.AddDays(-3)
            },
            new JobSearchResultDto
            {
                ExternalId = "mock-6",
                Title = "Backend Developer",
                Company = "Cloud Systems",
                Location = "Timisoara",
                Description = "Develop REST APIs, work with databases, authentication and application architecture using .NET.",
                ApplyUrl = "https://www.linkedin.com/jobs/",
                Source = "Mock",
                Salary = "Not specified",
                PostedAt = DateTime.UtcNow.AddDays(-5)
            }
        };

        [HttpGet("search")]
        public IActionResult SearchJobs(
            [FromQuery] string? query,
            [FromQuery] string? location,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            if (page < 1)
                page = 1;

            if (pageSize < 1)
                pageSize = 20;

            var results = MockJobs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                var normalizedQuery = query.Trim().ToLower();

                results = results.Where(job =>
                    job.Title.ToLower().Contains(normalizedQuery) ||
                    job.Company.ToLower().Contains(normalizedQuery) ||
                    job.Description.ToLower().Contains(normalizedQuery));
            }

            if (!string.IsNullOrWhiteSpace(location))
            {
                var normalizedLocation = location.Trim().ToLower();

                results = results.Where(job =>
                    job.Location.ToLower().Contains(normalizedLocation));
            }

            var finalResults = results
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Ok(finalResults);
        }
    }
}