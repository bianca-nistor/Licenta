using JobCv.Api.Dtos;
using JobCv.Api.Pdf;
using JobCv.Api.Services;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;

namespace JobCv.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CareerTestController : ControllerBase
    {
        private readonly GeminiAiService _geminiAiService;

        public CareerTestController(GeminiAiService geminiAiService)
        {
            _geminiAiService = geminiAiService;
        }

        [HttpPost("ai-recommendations")]
        public async Task<IActionResult> GenerateAiRecommendations(
            [FromBody] CareerAiRecommendationsRequestDto request)
        {
            if (request == null)
                return BadRequest("Invalid career test result.");

            if (request.AreaScores == null || request.AreaScores.Count == 0)
                return BadRequest("Career test scores are required.");

            var result = await _geminiAiService.GenerateCareerRecommendationsAsync(request);

            return Ok(result);
        }

        [HttpPost("export-pdf")]
        public IActionResult ExportCareerTestPdf([FromBody] CareerTestPdfRequestDto request)
        {
            if (request == null)
                return BadRequest("Invalid career test result.");

            if (string.IsNullOrWhiteSpace(request.ProfileTitle))
                request.ProfileTitle = "Career orientation result";

            if (string.IsNullOrWhiteSpace(request.Summary))
                request.Summary = "Your result was generated based on your answers.";

            request.GeneratedAt = request.GeneratedAt == default
                ? DateTime.UtcNow
                : request.GeneratedAt;

            var document = new CareerTestPdfDocument(request);
            var pdfBytes = document.GeneratePdf();

            var safeTitle = string.Join("_", request.ProfileTitle.Split(Path.GetInvalidFileNameChars()));
            var fileName = $"career_orientation_test_{safeTitle}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }
    }
}