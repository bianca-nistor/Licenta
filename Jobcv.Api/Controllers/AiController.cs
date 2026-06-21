using JobCv.Api.Dtos;
using JobCv.Api.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net;
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
        public async Task<IActionResult> GenerateInterviewPrep(
            [FromBody] InterviewPrepRequestDto request)
        {
            if (request == null)
                return BadRequest("Invalid request.");

            if (string.IsNullOrWhiteSpace(request.JobTitle) &&
                string.IsNullOrWhiteSpace(request.Description))
            {
                return BadRequest("Job title or description is required.");
            }

            var provider = _configuration["Ai:Provider"] ?? "Mock";

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
        public async Task<IActionResult> GenerateCvTailoring(
    [FromBody] CvTailoringRequestDto request)
        {
            if (request == null)
                return BadRequest("Invalid request.");

            if (string.IsNullOrWhiteSpace(request.JobTitle) &&
                string.IsNullOrWhiteSpace(request.JobDescription))
            {
                return BadRequest("Job title or job description is required.");
            }

            var provider = _configuration["Ai:Provider"] ?? "Mock";

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
                if (AiLanguageHelper.IsRomanian(request.Language))
                {
                    result = new CvTailoringResponseDto
                    {
                        JobTitle = request.JobTitle,
                        Company = request.Company,
                        TailoredProfileSummary =
                            $"Profilul candidatului este adaptat pentru rolul {request.JobTitle} la {request.Company}.",
                        ImportantKeywords = new List<string>
                        {
                            "comunicare",
                            "rezolvare de probleme",
                            "lucru în echipă"
                        },
                        SkillsToHighlight = new List<string>
                        {
                            "Competențe relevante",
                            "Adaptabilitate",
                            "Atenție la detalii"
                        },
                        ExperienceToEmphasize = new List<string>
                        {
                            "Menționează proiecte sau responsabilități care se potrivesc acestui job."
                        },
                        Suggestions = new List<CvTailoringSuggestionDto>
                        {
                            new CvTailoringSuggestionDto
                            {
                                Section = "Profil",
                                Suggestion = "Adaptează secțiunea de profil pentru jobul selectat.",
                                Reason = "Acest lucru face CV-ul mai relevant pentru recrutor."
                            }
                        },
                        IsMock = true
                    };
                }
                else
                {
                    result = new CvTailoringResponseDto
                    {
                        JobTitle = request.JobTitle,
                        Company = request.Company,
                        TailoredProfileSummary =
                            $"Candidate profile adapted for the {request.JobTitle} role at {request.Company}.",
                        ImportantKeywords = new List<string>
                        {
                            "communication",
                            "problem solving",
                            "teamwork"
                        },
                        SkillsToHighlight = new List<string>
                        {
                            "Relevant skills",
                            "Adaptability",
                            "Attention to detail"
                        },
                        ExperienceToEmphasize = new List<string>
                        {
                            "Mention projects or responsibilities that match this job."
                        },
                        Suggestions = new List<CvTailoringSuggestionDto>
                        {
                            new CvTailoringSuggestionDto
                            {
                                Section = "Profile",
                                Suggestion = "Adapt the profile section to the selected job.",
                                Reason = "This makes the CV more relevant for the recruiter."
                            }
                        },
                        IsMock = true
                    };
                }
            }

            return Ok(result);
        }
        [HttpPost("cv-quality-check")]
        public IActionResult CheckCvQuality([FromBody] CvQualityCheckRequestDto request)
        {
            if (request == null)
                return BadRequest("Invalid request.");

            var result = AiLanguageHelper.LocalizeCvQualityResponse(BuildCvQualityCheck(request), request.Language);

            return Ok(result);
        }

        private static CvQualityCheckResponseDto BuildCvQualityCheck(CvQualityCheckRequestDto request)
        {
            var issues = new List<CvQualityIssueDto>();
            var strengths = new List<string>();
            var suggestions = new List<string>();
            var score = 100;

            CheckTextSection(
                issues,
                ref score,
                "Profile summary",
                request.Summary,
                220,
                18,
                "The profile summary is missing or too short.",
                "Add 3-5 lines about your role, experience, technologies and career objective.");

            CheckTextSection(
                issues,
                ref score,
                "Full name",
                request.FullName,
                2,
                10,
                "The CV does not contain a full name.",
                "Add your real full name in the personal information section.");

            CheckTextSection(
                issues,
                ref score,
                "Email",
                request.Email,
                5,
                10,
                "The CV does not contain a valid email address.",
                "Add a professional email address so recruiters can contact you.");

            CheckTextSection(
                issues,
                ref score,
                "Phone",
                request.Phone,
                5,
                6,
                "The CV does not contain a phone number.",
                "Add a phone number if you want recruiters to contact you faster.");

            CheckListSection(
                issues,
                ref score,
                "Skills",
                request.Skills,
                4,
                16,
                "The CV has too few skills.",
                "Add at least 4-8 relevant technical and soft skills.");

            CheckListSection(
                issues,
                ref score,
                "Experience",
                request.Experiences,
                1,
                18,
                "The CV does not contain professional experience.",
                "Add work experience, internship experience or relevant responsibilities.");

            CheckListSection(
                issues,
                ref score,
                "Education",
                request.Educations,
                1,
                12,
                "The CV does not contain education details.",
                "Add your degree, university or relevant education program.");

            CheckListSection(
                issues,
                ref score,
                "Projects",
                request.Projects,
                1,
                12,
                "The CV does not contain projects.",
                "Add at least one project with technologies, your contribution and result.");

            if (request.Certifications == null || request.Certifications.Count == 0)
            {
                issues.Add(new CvQualityIssueDto
                {
                    Section = "Certifications",
                    Problem = "No certifications are listed.",
                    Suggestion = "Add certifications only if they are relevant. This is optional, but useful for junior candidates.",
                    Severity = "Low"
                });

                score -= 4;
            }
            else
            {
                strengths.Add("The CV includes certifications, which can support the candidate profile.");
            }

            if (request.Languages == null || request.Languages.Count == 0)
            {
                issues.Add(new CvQualityIssueDto
                {
                    Section = "Languages",
                    Problem = "No languages are listed.",
                    Suggestion = "Add languages and proficiency levels, especially English if relevant for the job market.",
                    Severity = "Medium"
                });

                score -= 6;
            }
            else
            {
                strengths.Add("The CV includes language information.");
            }

            AddPlaceholderWarnings(issues, ref score, request);
            AddStrengths(strengths, request);
            AddSuggestions(suggestions, issues, request);

            score = Math.Clamp(score, 0, 100);

            return new CvQualityCheckResponseDto
            {
                Score = score,
                CompletenessLevel = GetCompletenessLevel(score),
                Summary = BuildQualitySummary(score, issues.Count),
                Strengths = strengths.Distinct().Take(6).ToList(),
                Issues = issues.Take(12).ToList(),
                Suggestions = suggestions.Distinct().Take(8).ToList(),
                IsMock = true
            };
        }

        private static void CheckTextSection(
            List<CvQualityIssueDto> issues,
            ref int score,
            string section,
            string? value,
            int recommendedLength,
            int penalty,
            string problem,
            string suggestion)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Trim().Length < recommendedLength)
            {
                issues.Add(new CvQualityIssueDto
                {
                    Section = section,
                    Problem = problem,
                    Suggestion = suggestion,
                    Severity = penalty >= 15 ? "High" : "Medium"
                });

                score -= penalty;
            }
        }

        private static void CheckListSection(
            List<CvQualityIssueDto> issues,
            ref int score,
            string section,
            List<string>? values,
            int minimumCount,
            int penalty,
            string problem,
            string suggestion)
        {
            var validItems = values?
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Where(value => value.Trim().Length > 2)
                .ToList() ?? new List<string>();

            if (validItems.Count < minimumCount)
            {
                issues.Add(new CvQualityIssueDto
                {
                    Section = section,
                    Problem = problem,
                    Suggestion = suggestion,
                    Severity = penalty >= 15 ? "High" : "Medium"
                });

                score -= penalty;
            }
        }

        private static void AddPlaceholderWarnings(
            List<CvQualityIssueDto> issues,
            ref int score,
            CvQualityCheckRequestDto request)
        {
            var allText = string.Join(" ", new[]
            {
        request.Title,
        request.FullName,
        request.Email,
        request.Phone,
        request.Location,
        request.Summary,
        string.Join(" ", request.Skills ?? new List<string>()),
        string.Join(" ", request.Educations ?? new List<string>()),
        string.Join(" ", request.Experiences ?? new List<string>()),
        string.Join(" ", request.Projects ?? new List<string>())
    }).ToLowerInvariant();

            var placeholders = new[]
            {
        "lorem ipsum",
        "test",
        "asdf",
        "example",
        "your name",
        "untitled",
        "placeholder"
    };

            if (placeholders.Any(allText.Contains))
            {
                issues.Add(new CvQualityIssueDto
                {
                    Section = "General",
                    Problem = "The CV appears to contain placeholder or test text.",
                    Suggestion = "Replace generic or test values with real professional information before applying.",
                    Severity = "High"
                });

                score -= 15;
            }
        }

        private static void AddStrengths(List<string> strengths, CvQualityCheckRequestDto request)
        {
            if (!string.IsNullOrWhiteSpace(request.Summary) && request.Summary.Trim().Length >= 220)
                strengths.Add("The profile summary is detailed enough to introduce the candidate.");

            if (request.Skills != null && request.Skills.Count >= 4)
                strengths.Add("The CV includes a useful skills section.");

            if (request.Experiences != null && request.Experiences.Count > 0)
                strengths.Add("The CV includes experience information.");

            if (request.Projects != null && request.Projects.Count > 0)
                strengths.Add("The CV includes projects, which is useful especially for junior candidates.");

            if (!string.IsNullOrWhiteSpace(request.Email) && !string.IsNullOrWhiteSpace(request.Phone))
                strengths.Add("The CV contains multiple contact methods.");

            if (strengths.Count == 0)
                strengths.Add("The CV has a basic structure that can be improved by completing the missing sections.");
        }

        private static void AddSuggestions(
            List<string> suggestions,
            List<CvQualityIssueDto> issues,
            CvQualityCheckRequestDto request)
        {
            suggestions.Add("Use concrete achievements instead of general statements wherever possible.");
            suggestions.Add("Keep descriptions short, clear and focused on impact, technologies and responsibilities.");

            if (issues.Any(issue => issue.Section == "Skills"))
                suggestions.Add("Add skills that match the jobs you want to apply for, such as C#, .NET, SQL, REST APIs or communication.");

            if (issues.Any(issue => issue.Section == "Projects"))
                suggestions.Add("Add at least one relevant project with problem, solution, technologies and your role.");

            if (issues.Any(issue => issue.Section == "Experience"))
                suggestions.Add("If you do not have work experience, add internships, volunteering, university projects or freelance work.");

            if (request.Projects != null && request.Projects.Count > 0)
                suggestions.Add("For each project, mention the technologies used and the result of the project.");
        }

        private static string GetCompletenessLevel(int score)
        {
            if (score >= 85)
                return "High completeness";

            if (score >= 65)
                return "Medium completeness";

            if (score >= 40)
                return "Low completeness";

            return "Very low completeness";
        }

        private static string BuildQualitySummary(int score, int issueCount)
        {
            if (score >= 85)
                return "This CV is well structured and contains most of the information expected by recruiters.";

            if (score >= 65)
                return $"This CV is usable, but it still has {issueCount} area(s) that should be improved before applying.";

            if (score >= 40)
                return $"This CV is incomplete. It has {issueCount} important issue(s), so it should be improved before being used for job applications.";

            return "This CV contains too little useful information. Complete the main sections before using it for applications.";
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

            var provider = _configuration["Ai:Provider"] ?? "Mock";

            if (provider.Equals("Gemini", StringComparison.OrdinalIgnoreCase))
            {
                var geminiResult = await _geminiAiService.GenerateCvJobMatchAsync(request);
                if (geminiResult != null)
                    return Ok(AiLanguageHelper.LocalizeCvJobMatchResponse(geminiResult, request.Language));
            }

            var result = AiLanguageHelper.LocalizeCvJobMatchResponse(BuildExplainableCvJobMatch(request), request.Language);
            return Ok(result);
        }

        private static CvJobMatchResponseDto BuildExplainableCvJobMatch(CvJobMatchRequestDto request)
        {
            var jobText = $"{request.JobTitle} {request.Company} {request.Location} {request.JobDescription}";
            var cvText = request.CurrentCvText ?? string.Empty;

            var jobKeywords = ExtractImportantKeywords(jobText);
            var cvKeywords = ExtractImportantKeywords(cvText);

            var matchedKeywords = jobKeywords
                .Where(keyword => cvKeywords.Contains(keyword))
                .Distinct()
                .Take(8)
                .ToList();

            var missingKeywords = jobKeywords
                .Where(keyword => !cvKeywords.Contains(keyword))
                .Distinct()
                .Take(8)
                .ToList();

            var baseScore = jobKeywords.Count == 0
                ? 35
                : (int)Math.Round((double)matchedKeywords.Count / jobKeywords.Count * 100);

            var cvCompletenessBonus = GetCvCompletenessBonus(cvText);
            var score = Math.Clamp(baseScore + cvCompletenessBonus, 5, 95);

            var recommendation = score switch
            {
                >= 80 => "Strong match - apply with this CV",
                >= 60 => "Good match - improve the CV before applying",
                >= 40 => "Partial match - tailor the CV first",
                _ => "Weak match - improve the CV significantly first"
            };

            var strengths = matchedKeywords.Count == 0
                ? new List<string>
                {
            "The CV was found, but it does not clearly match the job keywords yet.",
            "You can still improve the match by tailoring the summary and skills sections."
                }
                : matchedKeywords.Select(keyword => $"The CV mentions {keyword}, which appears relevant for this role.").ToList();

            var missing = missingKeywords.Count == 0
                ? new List<string> { "No major missing keywords were detected from the available job description." }
                : missingKeywords.Select(keyword => $"The job description mentions {keyword}, but it is not clear in the CV.").ToList();

            var improvements = new List<string>
    {
        "Rewrite the profile summary so it directly targets this job title and company.",
        "Move the most relevant skills near the top of the CV.",
        "Add project or experience descriptions that prove the required skills.",
        "Use keywords from the job description naturally, without copying the entire text."
    };

            if (cvText.Trim().Length < 120)
            {
                improvements.Insert(0, "The selected CV contains very little text. Add summary, skills, experience, projects and education before applying.");
            }

            return new CvJobMatchResponseDto
            {
                JobTitle = request.JobTitle,
                Company = request.Company,
                MatchScore = score,
                Recommendation = recommendation,
                Summary = $"The score is based on overlap between important job keywords and the readable CV content. Matched terms: {matchedKeywords.Count}. Missing or weak terms: {missingKeywords.Count}.",
                Strengths = strengths,
                MissingSkills = missing,
                Improvements = improvements,
                IsMock = true
            };
        }

        private static int GetCvCompletenessBonus(string cvText)
        {
            if (string.IsNullOrWhiteSpace(cvText))
                return -15;

            var text = cvText.ToLowerInvariant();
            var bonus = 0;

            if (text.Contains("summary") || text.Contains("profile")) bonus += 4;
            if (text.Contains("skills")) bonus += 4;
            if (text.Contains("experience")) bonus += 4;
            if (text.Contains("project")) bonus += 4;
            if (text.Contains("education")) bonus += 4;

            if (cvText.Length < 120) bonus -= 20;
            else if (cvText.Length < 400) bonus -= 8;
            else if (cvText.Length > 900) bonus += 5;

            return bonus;
        }

        private static List<string> ExtractImportantKeywords(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new List<string>();

            var separators = new[]
            {
        ' ', '\n', '\r', '\t', ',', '.', ';', ':', '/', '\\', '|', '-', '_', '(', ')', '[', ']', '{', '}',
        '!', '?', '"', '\'', '<', '>', '+', '*', '&'
    };

            var stopWords = new HashSet<string>
    {
        "the", "and", "for", "with", "from", "that", "this", "you", "your", "are", "our", "will",
        "job", "role", "work", "team", "candidate", "company", "have", "has", "was", "were", "been",
        "about", "into", "their", "they", "them", "then", "than", "also", "can", "all", "any", "not",
        "but", "or", "in", "on", "at", "to", "of", "a", "an", "is", "as", "be", "by", "it", "we"
    };

            return text
                .ToLowerInvariant()
                .Split(separators, StringSplitOptions.RemoveEmptyEntries)
                .Select(word => word.Trim())
                .Where(word => word.Length >= 3)
                .Where(word => !stopWords.Contains(word))
                .Where(word => word.Any(char.IsLetter))
                .GroupBy(word => word)
                .OrderByDescending(group => group.Count())
                .ThenBy(group => group.Key)
                .Take(30)
                .Select(group => group.Key)
                .ToList();
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
                $"https://generativelanguage.googleapis.com/v1beta/models/{WebUtility.UrlEncode(model)}:generateContent?key={WebUtility.UrlEncode(apiKey)}";

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

                using var response = await httpClient.PostAsJsonAsync(url, requestBody);
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

            var provider = _configuration["Ai:Provider"] ?? "Mock";

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

    }
}