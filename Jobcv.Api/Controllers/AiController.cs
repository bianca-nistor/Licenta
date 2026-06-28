using JobCv.Api.Dtos;
using JobCv.Api.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace JobCv.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AiController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly MockAiService _mockAiService;
        private readonly OllamaAiService _ollamaAiService;
        private readonly GeminiAiService _geminiAiService;

        public AiController(
            IConfiguration configuration,
            MockAiService mockAiService,
            OllamaAiService ollamaAiService,
            GeminiAiService geminiAiService)
        {
            _configuration = configuration;
            _mockAiService = mockAiService;
            _ollamaAiService = ollamaAiService;
            _geminiAiService = geminiAiService;
        }

        [HttpPost("interview-prep")]
        public async Task<IActionResult> GenerateInterviewPrep([FromBody] InterviewPrepRequestDto request)
        {
            if (request == null)
                return BadRequest("Invalid request.");

            if (string.IsNullOrWhiteSpace(request.JobTitle) &&
                string.IsNullOrWhiteSpace(request.Description))
            {
                return BadRequest("Job title or description is required.");
            }

            var provider = GetAiProvider();

            InterviewPrepResponseDto result;

            if (provider.Equals("Gemini", StringComparison.OrdinalIgnoreCase))
            {
                result = await _geminiAiService.GenerateInterviewPrepAsync(request);
            }
            else if (provider.Equals("Ollama", StringComparison.OrdinalIgnoreCase))
            {
                result = await _ollamaAiService.GenerateInterviewPrepAsync(request);
            }
            else
            {
                result = await _mockAiService.GenerateInterviewPrepAsync(request);
            }

            return Ok(result);
        }

        [HttpPost("cv-tailoring")]
        public async Task<IActionResult> GenerateCvTailoring([FromBody] CvTailoringRequestDto request)
        {
            if (request == null)
                return BadRequest("Invalid request.");

            if (string.IsNullOrWhiteSpace(request.JobTitle) &&
                string.IsNullOrWhiteSpace(request.JobDescription))
            {
                return BadRequest("Job title or job description is required.");
            }

            var provider = GetAiProvider();

            CvTailoringResponseDto result;

            if (provider.Equals("Gemini", StringComparison.OrdinalIgnoreCase))
            {
                result = await _geminiAiService.GenerateCvTailoringAsync(request);
            }
            else if (provider.Equals("Ollama", StringComparison.OrdinalIgnoreCase))
            {
                result = await _ollamaAiService.GenerateCvTailoringAsync(request);
            }
            else
            {
                result = await _mockAiService.GenerateCvTailoringAsync(request);
            }

            return Ok(result);
        }

        [HttpPost("cv-quality-check")]
        public IActionResult CheckCvQuality([FromBody] CvQualityCheckRequestDto request)
        {
            if (request == null)
                return BadRequest("Invalid request.");

            var result = _mockAiService.CheckCvQuality(request);

            return Ok(result);
        }

        [HttpPost("cv-job-match")]
        public async Task<IActionResult> GenerateCvJobMatch([FromBody] CvJobMatchRequestDto request)
        {
            if (request == null)
                return BadRequest("Invalid request.");

            if (string.IsNullOrWhiteSpace(request.JobTitle) &&
                string.IsNullOrWhiteSpace(request.JobDescription))
            {
                return BadRequest("Job title or job description is required.");
            }

            var provider = GetAiProvider();

            if (provider.Equals("Gemini", StringComparison.OrdinalIgnoreCase))
            {
                var geminiResult = await _geminiAiService.GenerateCvJobMatchAsync(request);

                if (geminiResult != null)
                {
                    var localizedGeminiResult =
                        AiLanguageHelper.LocalizeCvJobMatchResponse(geminiResult, request.Language);

                    return Ok(localizedGeminiResult);
                }
            }

            var result = await _mockAiService.GenerateCvJobMatchAsync(request);

            return Ok(result);
        }

        [HttpPost("cover-letter")]
        public async Task<IActionResult> GenerateCoverLetter([FromBody] CoverLetterRequestDto request)
        {
            if (request == null)
                return BadRequest("Invalid request.");

            if (string.IsNullOrWhiteSpace(request.JobTitle) &&
                string.IsNullOrWhiteSpace(request.Company))
            {
                return BadRequest("Job title or company is required.");
            }

            var provider = GetAiProvider();

            CoverLetterResponseDto result;

            if (provider.Equals("Gemini", StringComparison.OrdinalIgnoreCase))
            {
                result = await _geminiAiService.GenerateCoverLetterAsync(request);
            }
            else
            {
                result = await _mockAiService.GenerateCoverLetterAsync(request);
            }

            return Ok(result);
        }

        [HttpGet("gemini-test")]
        public async Task<IActionResult> TestGemini()
        {
            var provider = _configuration["Ai:Provider"];
            var apiKey = _configuration["Gemini:ApiKey"];
            var model = _configuration["Gemini:Model"] ?? "gemini-2.5-flash-lite";

            if (string.IsNullOrWhiteSpace(provider))
            {
                return BadRequest(new
                {
                    ok = false,
                    step = "configuration",
                    problem = "Ai:Provider is missing.",
                    provider,
                    model,
                    apiKeyConfigured = !string.IsNullOrWhiteSpace(apiKey)
                });
            }

            if (!provider.Equals("Gemini", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    ok = false,
                    step = "configuration",
                    problem = "Ai:Provider is not Gemini.",
                    provider,
                    model,
                    apiKeyConfigured = !string.IsNullOrWhiteSpace(apiKey)
                });
            }

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return BadRequest(new
                {
                    ok = false,
                    step = "configuration",
                    problem = "Gemini:ApiKey is missing.",
                    provider,
                    model,
                    apiKeyConfigured = false
                });
            }

            var url =
                $"https://generativelanguage.googleapis.com/v1beta/models/{WebUtility.UrlEncode(model)}:generateContent";

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[]
                        {
                            new
                            {
                                text = "Return only this JSON: {\"ok\":true,\"message\":\"gemini works\"}"
                            }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0,
                    maxOutputTokens = 100,
                    responseMimeType = "application/json"
                }
            };

            try
            {
                using var httpClient = new HttpClient
                {
                    Timeout = TimeSpan.FromSeconds(30)
                };

                using var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
                requestMessage.Headers.TryAddWithoutValidation("x-goog-api-key", apiKey);
                requestMessage.Content = JsonContent.Create(requestBody);

                using var response = await httpClient.SendAsync(requestMessage);
                var rawResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode(500, new
                    {
                        ok = false,
                        step = "gemini-http-call",
                        problem = "Gemini returned an error status.",
                        statusCode = (int)response.StatusCode,
                        status = response.StatusCode.ToString(),
                        provider,
                        model,
                        apiKeyConfigured = true,
                        rawResponse = rawResponse.Length > 1200
                            ? rawResponse[..1200]
                            : rawResponse
                    });
                }

                using var document = JsonDocument.Parse(rawResponse);

                var text = document.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                if (string.IsNullOrWhiteSpace(text))
                {
                    return StatusCode(500, new
                    {
                        ok = false,
                        step = "gemini-response-parse",
                        problem = "Gemini response did not contain text.",
                        provider,
                        model,
                        apiKeyConfigured = true,
                        rawResponse = rawResponse.Length > 1200
                            ? rawResponse[..1200]
                            : rawResponse
                    });
                }

                return Ok(new
                {
                    ok = true,
                    step = "success",
                    provider,
                    model,
                    apiKeyConfigured = true,
                    geminiText = text
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    ok = false,
                    step = "exception",
                    problem = ex.Message,
                    provider,
                    model,
                    apiKeyConfigured = true
                });
            }
        }

        private string GetAiProvider()
        {
            return _configuration["Ai:Provider"] ?? "Mock";
        }
    }
}