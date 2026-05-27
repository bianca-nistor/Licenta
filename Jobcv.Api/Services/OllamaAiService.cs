using System.Net.Http.Json;
using System.Text.Json;
using JobCv.Api.Dtos;
using System.Text.RegularExpressions;

namespace JobCv.Api.Services
{
    public class OllamaAiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly MockAiService _mockAiService;

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public OllamaAiService(
            HttpClient httpClient,
            IConfiguration configuration,
            MockAiService mockAiService)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _mockAiService = mockAiService;
        }

        public async Task<InterviewPrepResponseDto> GenerateInterviewPrepAsync(
            InterviewPrepRequestDto request)
        {
            var model = _configuration["Ollama:Model"] ?? "llama3.2:1b";

            var ollamaRequest = new OllamaChatRequest
            {
                Model = model,
                Stream = false,
                Format = "json",
                Messages = new List<OllamaMessage>
                {
                    new OllamaMessage
                    {
                        Role = "system",
                        Content =
                            "You are an assistant for a career preparation app. " +
                            "Return only one valid JSON object. No markdown. No explanations outside JSON."
                    },
                    new OllamaMessage
                    {
                        Role = "user",
                        Content = BuildPrompt(request)
                    }
                },
                Options = new OllamaOptions
                {
                    Temperature = 0.1
                }
            };

            try
            {
                var response = await _httpClient.PostAsJsonAsync(
                    "/api/chat",
                    ollamaRequest,
                    _jsonOptions);

                if (!response.IsSuccessStatusCode)
                {
                    return await _mockAiService.GenerateInterviewPrepAsync(request);
                }

                var ollamaResponse =
                    await response.Content.ReadFromJsonAsync<OllamaChatResponse>(_jsonOptions);

                var content = ollamaResponse?.Message?.Content;

                if (string.IsNullOrWhiteSpace(content))
                {
                    return await _mockAiService.GenerateInterviewPrepAsync(request);
                }

                var cleanedJson = CleanJson(content);

                var result = JsonSerializer.Deserialize<InterviewPrepResponseDto>(
                    cleanedJson,
                    _jsonOptions);

                if (result == null)
                {
                    return await _mockAiService.GenerateInterviewPrepAsync(request);
                }

                result.JobTitle = string.IsNullOrWhiteSpace(result.JobTitle)
                    ? request.JobTitle
                    : result.JobTitle;

                result.Company = string.IsNullOrWhiteSpace(result.Company)
                    ? request.Company
                    : result.Company;

                result.Summary = string.IsNullOrWhiteSpace(result.Summary)
                    ? $"Interview preparation for {request.JobTitle} at {request.Company}."
                    : result.Summary;

                result.KeySkills ??= new List<string>();
                result.Questions ??= new List<InterviewQuestionDto>();
                result.BeforeInterviewTips ??= new List<string>();

                if (result.KeySkills.Count == 0)
                {
                    result.KeySkills = new List<string>
                    {
                        "Communication",
                        "Problem solving",
                        "Role understanding"
                    };
                }

                if (result.Questions.Count == 0)
                {
                    result.Questions = new List<InterviewQuestionDto>
                    {
                        new InterviewQuestionDto
                        {
                            Category = "General",
                            Question = "Can you tell me why you are interested in this role?",
                            SuggestedAnswer = "Connect your experience, skills and career goals with the responsibilities of the role."
                        }
                    };
                }

                if (result.BeforeInterviewTips.Count == 0)
                {
                    result.BeforeInterviewTips = new List<string>
                    {
                        "Review the job description carefully.",
                        "Prepare examples from your previous experience.",
                        "Research the company before the interview."
                    };
                }

                result.IsMock = false;

                return result;
            }
            catch
            {
                return await _mockAiService.GenerateInterviewPrepAsync(request);
            }
        }

        private static string BuildPrompt(InterviewPrepRequestDto request)
        {
            var jobTitle = string.IsNullOrWhiteSpace(request.JobTitle)
                ? "Selected job"
                : request.JobTitle;

            var company = string.IsNullOrWhiteSpace(request.Company)
                ? "Unknown company"
                : request.Company;

            var location = string.IsNullOrWhiteSpace(request.Location)
                ? "Not specified"
                : request.Location;

            var description = string.IsNullOrWhiteSpace(request.Description)
                ? "No job description available."
                : request.Description;

            if (description.Length > 1800)
                description = description[..1800];

            return $@"
Generate interview preparation for this job.

Job title: {jobTitle}
Company: {company}
Location: {location}
Description: {description}

Return exactly one JSON object with this structure:

{{
  ""jobTitle"": ""{EscapeForPrompt(jobTitle)}"",
  ""company"": ""{EscapeForPrompt(company)}"",
  ""summary"": ""short preparation summary"",
  ""keySkills"": [
    ""skill 1"",
    ""skill 2"",
    ""skill 3"",
    ""skill 4"",
    ""skill 5""
  ],
  ""questions"": [
    {{
      ""category"": ""General"",
      ""question"": ""question text"",
      ""suggestedAnswer"": ""short suggested answer""
    }},
    {{
      ""category"": ""Technical"",
      ""question"": ""question text"",
      ""suggestedAnswer"": ""short suggested answer""
    }},
    {{
      ""category"": ""Behavioural"",
      ""question"": ""question text"",
      ""suggestedAnswer"": ""short suggested answer""
    }},
    {{
      ""category"": ""Closing"",
      ""question"": ""question text"",
      ""suggestedAnswer"": ""short suggested answer""
    }}
  ],
  ""beforeInterviewTips"": [
    ""tip 1"",
    ""tip 2"",
    ""tip 3""
  ],
  ""isMock"": false
}}

Rules:
- Return JSON only.
- Do not include markdown.
- Do not include text before or after the JSON.
- Keep answers concise.
";
        }
        public async Task<CvTailoringResponseDto> GenerateCvTailoringAsync(
    CvTailoringRequestDto request)
        {
            var model = _configuration["Ollama:Model"] ?? "llama3.2:1b";
            var cvQualityWarning = GetCvQualityWarning(request.CurrentCvText);

            var ollamaRequest = new OllamaChatRequest
            {
                Model = model,
                Stream = false,
                Format = "json",
                Messages = new List<OllamaMessage>
        {
            new OllamaMessage
            {
                Role = "system",
                Content =
                    "You are an assistant for a CV and career preparation app. " +
                    "Return only one valid JSON object. No markdown. No explanations outside JSON."
            },
            new OllamaMessage
            {
                Role = "user",
                Content = BuildCvTailoringPrompt(request)
            }
        },
                Options = new OllamaOptions
                {
                    Temperature = 0.1
                }
            };

            try
            {
                var response = await _httpClient.PostAsJsonAsync(
                    "/api/chat",
                    ollamaRequest,
                    _jsonOptions);

                if (!response.IsSuccessStatusCode)
                    return BuildMockCvTailoring(request);

                var ollamaResponse =
                    await response.Content.ReadFromJsonAsync<OllamaChatResponse>(_jsonOptions);

                var content = ollamaResponse?.Message?.Content;

                if (string.IsNullOrWhiteSpace(content))
                    return BuildMockCvTailoring(request);

                var cleanedJson = CleanJson(content);

                var result = JsonSerializer.Deserialize<CvTailoringResponseDto>(
                    cleanedJson,
                    _jsonOptions);

                if (result == null)
                    return BuildMockCvTailoring(request);

                result.JobTitle = string.IsNullOrWhiteSpace(result.JobTitle)
                    ? request.JobTitle
                    : result.JobTitle;

                result.Company = string.IsNullOrWhiteSpace(result.Company)
                    ? request.Company
                    : result.Company;

                result.ImportantKeywords ??= new List<string>();
                result.SkillsToHighlight ??= new List<string>();
                result.ExperienceToEmphasize ??= new List<string>();
                result.Suggestions ??= new List<CvTailoringSuggestionDto>();

                if (string.IsNullOrWhiteSpace(result.TailoredProfileSummary))
                {
                    result.TailoredProfileSummary =
                        $"Candidate profile adapted for the {request.JobTitle} role at {request.Company}.";
                }

                if (result.ImportantKeywords.Count == 0)
                {
                    result.ImportantKeywords = new List<string>
            {
                "communication",
                "problem solving",
                "teamwork"
            };
                }

                if (result.SkillsToHighlight.Count == 0)
                {
                    result.SkillsToHighlight = new List<string>
            {
                "Relevant technical skills",
                "Communication",
                "Adaptability"
            };
                }

                if (result.ExperienceToEmphasize.Count == 0)
                {
                    result.ExperienceToEmphasize = new List<string>
            {
                "Mention projects or responsibilities that match the job description."
            };
                }

                if (result.Suggestions.Count == 0)
                {
                    result.Suggestions = new List<CvTailoringSuggestionDto>
            {
                new CvTailoringSuggestionDto
                {
                    Section = "Profile",
                    Suggestion = "Rewrite the profile section to match the role and company.",
                    Reason = "The first section of the CV should quickly show relevance to the job."
                }
            };
                }

                result.IsMock = false;

                result.CvQualityWarning = cvQualityWarning;

                if (!string.IsNullOrWhiteSpace(cvQualityWarning))
                {
                    result.TailoredProfileSummary =
                        "The selected CV does not contain enough reliable information to create a truly personalized version. The suggestions below are based mostly on the job description.";

                    result.ExperienceToEmphasize = new List<string>
    {
        "Complete the CV with real projects, responsibilities or work experience.",
        "Add technologies, tools, achievements and measurable results.",
        "Replace placeholder or random text with clear professional information."
    };

                    result.Suggestions.Insert(0, new CvTailoringSuggestionDto
                    {
                        Section = "CV quality",
                        Suggestion = "Update the selected CV before using AI tailoring.",
                        Reason = cvQualityWarning
                    });
                }

                return result;
            }
            catch
            {
                return BuildMockCvTailoring(request);
            }
        }
        private static string GetCvQualityWarning(string cvText)
        {
            if (string.IsNullOrWhiteSpace(cvText))
            {
                return "No CV content was provided. The AI can only generate general advice based on the job description.";
            }

            var cleaned = Regex.Replace(cvText, @"\s+", " ").Trim();

            if (cleaned.Length < 80)
            {
                return "The selected CV contains very little text. Add a profile summary, skills, education, projects or experience before tailoring it.";
            }

            var letters = cleaned.Count(char.IsLetter);
            var digits = cleaned.Count(char.IsDigit);
            var spaces = cleaned.Count(char.IsWhiteSpace);
            var total = cleaned.Length;

            var letterRatio = total == 0 ? 0 : (double)letters / total;

            if (letterRatio < 0.45)
            {
                return "The selected CV does not look like normal CV text. It may contain placeholders, random characters or incomplete data.";
            }

            var words = cleaned
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim().ToLowerInvariant())
                .Where(x => x.Length > 1)
                .ToList();

            if (words.Count < 15)
            {
                return "The selected CV does not contain enough meaningful words to create personalized tailoring suggestions.";
            }

            var uniqueWords = words.Distinct().Count();

            if (uniqueWords < 8)
            {
                return "The selected CV seems repetitive or incomplete. Add more real information before tailoring it.";
            }

            var suspiciousShortWords = words.Count(w => w.Length <= 2);

            if (suspiciousShortWords > words.Count * 0.55)
            {
                return "The selected CV appears to contain many short or random words, so the tailoring may not be reliable.";
            }

            return string.Empty;
        }
        private static string BuildCvTailoringPrompt(CvTailoringRequestDto request)
        {
            var jobTitle = string.IsNullOrWhiteSpace(request.JobTitle)
                ? "Selected job"
                : request.JobTitle;

            var company = string.IsNullOrWhiteSpace(request.Company)
                ? "Unknown company"
                : request.Company;

            var location = string.IsNullOrWhiteSpace(request.Location)
                ? "Not specified"
                : request.Location;

            var jobDescription = string.IsNullOrWhiteSpace(request.JobDescription)
                ? "No job description available."
                : request.JobDescription;

            var cvText = string.IsNullOrWhiteSpace(request.CurrentCvText)
                ? "No current CV text was provided. Give general tailoring advice based only on the job description."
                : request.CurrentCvText;

            if (jobDescription.Length > 1800)
                jobDescription = jobDescription[..1800];

            if (cvText.Length > 2500)
                cvText = cvText[..2500];

            return $@"
Generate CV tailoring advice for this job.

Job title: {jobTitle}
Company: {company}
Location: {location}
Job description: {jobDescription}

Current CV text:
{cvText}

Return exactly one JSON object with this structure:

{{
  ""jobTitle"": ""{EscapeForPrompt(jobTitle)}"",
  ""company"": ""{EscapeForPrompt(company)}"",
  ""tailoredProfileSummary"": ""short rewritten CV profile summary for this job"",
  ""importantKeywords"": [
    ""keyword 1"",
    ""keyword 2"",
    ""keyword 3"",
    ""keyword 4"",
    ""keyword 5""
  ],
  ""skillsToHighlight"": [
    ""skill 1"",
    ""skill 2"",
    ""skill 3"",
    ""skill 4""
  ],
  ""experienceToEmphasize"": [
    ""experience or project type 1"",
    ""experience or project type 2"",
    ""experience or project type 3""
  ],
  ""suggestions"": [
    {{
      ""section"": ""Profile"",
      ""suggestion"": ""specific improvement"",
      ""reason"": ""why this helps""
    }},
    {{
      ""section"": ""Skills"",
      ""suggestion"": ""specific improvement"",
      ""reason"": ""why this helps""
    }},
    {{
      ""section"": ""Experience"",
      ""suggestion"": ""specific improvement"",
      ""reason"": ""why this helps""
    }}
  ],
  ""isMock"": false
}}

Rules:
- Return JSON only.
- Do not include markdown.
- Do not include text before or after JSON.
- Keep the advice practical and concise.
- Do not invent degrees, companies, certifications or experience that are not in the CV.
- If the CV text is missing, give general suggestions based on the job description.
";
            return $@"
Generate CV tailoring advice for this job.

Job title: {jobTitle}
Company: {company}
Location: {location}
Job description: {jobDescription}

Current CV text:
{cvText}

Return exactly one JSON object with this structure:

{{
  ""jobTitle"": ""{EscapeForPrompt(jobTitle)}"",
  ""company"": ""{EscapeForPrompt(company)}"",
  ""tailoredProfileSummary"": ""short rewritten CV profile summary for this job"",
  ""importantKeywords"": [
    ""keyword 1"",
    ""keyword 2"",
    ""keyword 3"",
    ""keyword 4"",
    ""keyword 5""
  ],
  ""skillsToHighlight"": [
    ""skill 1"",
    ""skill 2"",
    ""skill 3"",
    ""skill 4""
  ],
  ""experienceToEmphasize"": [
    ""experience or project type 1"",
    ""experience or project type 2"",
    ""experience or project type 3""
  ],
  ""suggestions"": [
    {{
      ""section"": ""Profile"",
      ""suggestion"": ""specific improvement"",
      ""reason"": ""why this helps""
    }},
    {{
      ""section"": ""Skills"",
      ""suggestion"": ""specific improvement"",
      ""reason"": ""why this helps""
    }},
    {{
      ""section"": ""Experience"",
      ""suggestion"": ""specific improvement"",
      ""reason"": ""why this helps""
    }}
  ],
  ""cvQualityWarning"": """",
  ""isMock"": false
}}

Rules:
- Return JSON only.
- Do not include markdown.
- Do not include text before or after JSON.
- Keep the advice practical and concise.
- Do not invent degrees, companies, certifications or experience that are not in the CV.
- If the CV text is missing, too short, random, or not meaningful, clearly say that the CV cannot be tailored properly yet.
- If the CV text is weak, focus suggestions on improving the CV content first.
";
        }

        private static CvTailoringResponseDto BuildMockCvTailoring(CvTailoringRequestDto request)
        {
            var title = string.IsNullOrWhiteSpace(request.JobTitle)
                ? "Selected job"
                : request.JobTitle;

            var company = string.IsNullOrWhiteSpace(request.Company)
                ? "Unknown company"
                : request.Company;

            return new CvTailoringResponseDto
            {
                JobTitle = title,
                Company = company,
                TailoredProfileSummary =
                    $"Motivated candidate interested in the {title} role at {company}, with relevant skills, adaptability and strong communication abilities.",

                ImportantKeywords = new List<string>
        {
            "communication",
            "problem solving",
            "teamwork",
            "adaptability",
            "attention to detail"
        },

                SkillsToHighlight = new List<string>
        {
            "Relevant technical skills",
            "Communication",
            "Problem solving",
            "Team collaboration"
        },

                ExperienceToEmphasize = new List<string>
        {
            "Mention projects or tasks that match the responsibilities in the job description.",
            "Use measurable examples where possible.",
            "Highlight experience that proves you can adapt to the role quickly."
        },

                Suggestions = new List<CvTailoringSuggestionDto>
        {
            new CvTailoringSuggestionDto
            {
                Section = "Profile",
                Suggestion = "Rewrite the profile section so it mentions the target role and the most relevant strengths.",
                Reason = "Recruiters usually read the profile first, so it should quickly match the job."
            },
            new CvTailoringSuggestionDto
            {
                Section = "Skills",
                Suggestion = "Move the most relevant skills for this job near the top of the skills section.",
                Reason = "This makes the CV easier to scan and improves relevance."
            },
            new CvTailoringSuggestionDto
            {
                Section = "Experience",
                Suggestion = "Add short bullet points that connect your previous work or projects to the job requirements.",
                Reason = "Specific examples are stronger than general statements."
            }
        },
                CvQualityWarning = GetCvQualityWarning(request.CurrentCvText),

                IsMock = true
            };
        }
        private static string EscapeForPrompt(string value)
        {
            return value
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"");
        }

        private static string CleanJson(string content)
        {
            var cleaned = content.Trim();

            cleaned = cleaned.Replace("```json", "", StringComparison.OrdinalIgnoreCase);
            cleaned = cleaned.Replace("```", "");

            var firstBrace = cleaned.IndexOf('{');
            var lastBrace = cleaned.LastIndexOf('}');

            if (firstBrace >= 0 && lastBrace > firstBrace)
                cleaned = cleaned[firstBrace..(lastBrace + 1)];

            return cleaned.Trim();
        }

        private class OllamaChatRequest
        {
            public string Model { get; set; } = string.Empty;
            public bool Stream { get; set; }
            public string Format { get; set; } = "json";
            public List<OllamaMessage> Messages { get; set; } = new();
            public OllamaOptions Options { get; set; } = new();
        }

        private class OllamaMessage
        {
            public string Role { get; set; } = string.Empty;
            public string Content { get; set; } = string.Empty;
        }

        private class OllamaOptions
        {
            public double Temperature { get; set; }
        }

        private class OllamaChatResponse
        {
            public OllamaMessage? Message { get; set; }
        }
    }
}