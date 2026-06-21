using JobCv.Api.Dtos;
using JobCv.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace JobCv.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobsController : ControllerBase
    {
        private readonly AdzunaJobSearchService _adzunaJobSearchService;

        public JobsController(AdzunaJobSearchService adzunaJobSearchService)
        {
            _adzunaJobSearchService = adzunaJobSearchService;
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchJobs(
            [FromQuery] string? query,
            [FromQuery] string? location,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? country = null)
        {
            try
            {
                if (page < 1)
                    page = 1;

                if (pageSize < 1)
                    pageSize = 20;

                if (pageSize > 50)
                    pageSize = 50;

                var jobs = await _adzunaJobSearchService.SearchJobsAsync(
                    query,
                    location,
                    page,
                    pageSize,
                    country);

                return Ok(jobs);
            }
            catch (Exception ex)
            {
                return StatusCode(502, new
                {
                    message = "Could not load jobs from Adzuna.",
                    details = ex.Message
                });
            }
        }
    }
}