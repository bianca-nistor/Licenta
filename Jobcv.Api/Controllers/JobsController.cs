using JobCv.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace JobCv.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobsController : ControllerBase
    {
        private readonly AdzunaJobSearchService _jobSearchService;

        public JobsController(AdzunaJobSearchService jobSearchService)
        {
            _jobSearchService = jobSearchService;
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchJobs(
            [FromQuery] string? query,
            [FromQuery] string? location,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var jobs = await _jobSearchService.SearchJobsAsync(
                query,
                location,
                page,
                pageSize);

            return Ok(jobs);
        }
    }
}