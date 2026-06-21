using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using JobCv.Api.Dtos;

namespace JobCv.Api.Services
{
    public class GeminiAiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly MockAiService _mockAiService;

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public GeminiAiService(
            HttpClient httpClient,
            IConfiguration configuration,
            MockAiService mockAiService)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _mockAiService = mockAiService;
        }

        public async Task<InterviewPrepResponseDto> GenerateInterviewPrepAsync(InterviewPrepRequestDto request)
        {
            var result = await GenerateJsonAsync<InterviewPrepResponseDto>(BuildInterviewPrepPrompt(request));

            if (result == null)
                return await _mockAiService.GenerateInterviewPrepAsync(request);

            result.JobTitle = string.IsNullOrWhiteSpace(result.JobTitle) ? request.JobTitle : result.JobTitle;
            result.Company = string.IsNullOrWhiteSpace(result.Company) ? request.Company : result.Company;
            result.Summary = string.IsNullOrWhiteSpace(result.Summary)
                ? $"Interview preparation for {request.JobTitle} at {request.Company}."
                : result.Summary;
            result.KeySkills ??= new List<string>();
            result.Questions ??= new List<InterviewQuestionDto>();
            result.BeforeInterviewTips ??= new List<string>();
            result.IsMock = false;

            return result;
        }

        public async Task<CvTailoringResponseDto> GenerateCvTailoringAsync(CvTailoringRequestDto request)
        {
            var result = await GenerateJsonAsync<CvTailoringResponseDto>(BuildCvTailoringPrompt(request));

            if (result == null)
                return BuildMockCvTailoring(request);

            result.JobTitle = string.IsNullOrWhiteSpace(result.JobTitle) ? request.JobTitle : result.JobTitle;
            result.Company = string.IsNullOrWhiteSpace(result.Company) ? request.Company : result.Company;
            result.TailoredProfileSummary ??= string.Empty;
            result.ImportantKeywords ??= new List<string>();
            result.SkillsToHighlight ??= new List<string>();
            result.ExperienceToEmphasize ??= new List<string>();
            result.Suggestions ??= new List<CvTailoringSuggestionDto>();
            result.CvQualityWarning ??= string.Empty;
            result.IsMock = false;

            return result;
        }

        public async Task<CvJobMatchResponseDto?> GenerateCvJobMatchAsync(CvJobMatchRequestDto request)
        {
            var result = await GenerateJsonAsync<CvJobMatchResponseDto>(BuildCvJobMatchPrompt(request));

            if (result == null)
                return null;

            result.JobTitle = string.IsNullOrWhiteSpace(result.JobTitle) ? request.JobTitle : result.JobTitle;
            result.Company = string.IsNullOrWhiteSpace(result.Company) ? request.Company : result.Company;
            result.MatchScore = Math.Clamp(result.MatchScore, 0, 100);
            result.Recommendation ??= string.Empty;
            result.Summary ??= string.Empty;
            result.Strengths ??= new List<string>();
            result.MissingSkills ??= new List<string>();
            result.Improvements ??= new List<string>();
            result.IsMock = false;

            return result;
        }

        private async Task<T?> GenerateJsonAsync<T>(string prompt)
        {
            var apiKey = _configuration["Gemini:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
                return default;

            var model = _configuration["Gemini:Model"] ?? "gemini-2.5-flash-lite";
            var url = $"v1beta/models/{WebUtility.UrlEncode(model)}:generateContent?key={WebUtility.UrlEncode(apiKey)}";

            var geminiRequest = new GeminiGenerateContentRequest
            {
                Contents = new List<GeminiContent>
                {
                    new GeminiContent
                    {
                        Role = "user",
                        Parts = new List<GeminiPart>
                        {
                            new GeminiPart
                            {
                                Text =
    "Return only one valid JSON object. No markdown. No explanations outside JSON.\n\n" +
    prompt
                            }
                        }
                    }
                },
                GenerationConfig = new GeminiGenerationConfig
                {
                    Temperature = 0.2,
                    MaxOutputTokens = 1600,
                    ResponseMimeType = "application/json"
                }
            };

            try
            {
                using var response = await _httpClient.PostAsJsonAsync(url, geminiRequest, _jsonOptions);

                if (!response.IsSuccessStatusCode)
                    return default;

                var geminiResponse = await response.Content.ReadFromJsonAsync<GeminiGenerateContentResponse>(_jsonOptions);
                var text = geminiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

                if (string.IsNullOrWhiteSpace(text))
                    return default;

                return JsonSerializer.Deserialize<T>(CleanJson(text), _jsonOptions);
            }
            catch
            {
                return default;
            }
        }

        private static string BuildInterviewPrepPrompt(InterviewPrepRequestDto request)
        {
            var jobTitle = DefaultText(request.JobTitle, AiLanguageHelper.DefaultJobTitle(request.Language));
            var company = DefaultText(request.Company, AiLanguageHelper.DefaultCompany(request.Language));
            var location = DefaultText(request.Location, AiLanguageHelper.DefaultLocation(request.Language));
            var description = Limit(DefaultText(request.Description, AiLanguageHelper.DefaultJobDescription(request.Language)), 1800);
            var languageInstruction = AiLanguageHelper.GetLanguageInstruction(request.Language);

            return $$"""
You are an assistant for a career preparation mobile app.
{{languageInstruction}}
Generate interview preparation for this job.

Job title: {{jobTitle}}
Company: {{company}}
Location: {{location}}
Description: {{description}}

Return exactly one valid JSON object matching this C# DTO shape:
{
  "jobTitle": "{{EscapeForPrompt(jobTitle)}}",
  "company": "{{EscapeForPrompt(company)}}",
  "summary": "short preparation summary",
  "keySkills": ["skill 1", "skill 2", "skill 3", "skill 4", "skill 5"],
  "questions": [
    { "category": "General", "question": "question text", "suggestedAnswer": "short suggested answer" },
    { "category": "Technical", "question": "question text", "suggestedAnswer": "short suggested answer" },
    { "category": "Behavioural", "question": "question text", "suggestedAnswer": "short suggested answer" },
    { "category": "Closing", "question": "question text", "suggestedAnswer": "short suggested answer" }
  ],
  "beforeInterviewTips": ["tip 1", "tip 2", "tip 3"],
  "isMock": false
}

Rules:
- Return JSON only.
- Keep answers practical and concise.
- All user-facing values must respect the requested language.
""";
        }

        private static string BuildCvTailoringPrompt(CvTailoringRequestDto request)
        {
            var jobTitle = DefaultText(request.JobTitle, AiLanguageHelper.DefaultJobTitle(request.Language));
            var company = DefaultText(request.Company, AiLanguageHelper.DefaultCompany(request.Language));
            var location = DefaultText(request.Location, AiLanguageHelper.DefaultLocation(request.Language));
            var jobDescription = Limit(DefaultText(request.JobDescription, AiLanguageHelper.DefaultJobDescription(request.Language)), 1800);
            var cvText = Limit(DefaultText(request.CurrentCvText, AiLanguageHelper.DefaultCvText(request.Language)), 2500);
            var languageInstruction = AiLanguageHelper.GetLanguageInstruction(request.Language);

            return $$"""
You are an assistant for a CV and career preparation mobile app.
{{languageInstruction}}
Generate CV tailoring advice for this job.

Job title: {{jobTitle}}
Company: {{company}}
Location: {{location}}
Job description: {{jobDescription}}

Current CV text:
{{cvText}}

Return exactly one valid JSON object matching this C# DTO shape:
{
  "jobTitle": "{{EscapeForPrompt(jobTitle)}}",
  "company": "{{EscapeForPrompt(company)}}",
  "tailoredProfileSummary": "short rewritten CV profile summary for this job",
  "importantKeywords": ["keyword 1", "keyword 2", "keyword 3", "keyword 4", "keyword 5"],
  "skillsToHighlight": ["skill 1", "skill 2", "skill 3", "skill 4"],
  "experienceToEmphasize": ["experience or project type 1", "experience or project type 2", "experience or project type 3"],
  "suggestions": [
    { "section": "Profile", "suggestion": "specific improvement", "reason": "why this helps" },
    { "section": "Skills", "suggestion": "specific improvement", "reason": "why this helps" },
    { "section": "Experience", "suggestion": "specific improvement", "reason": "why this helps" }
  ],
  "cvQualityWarning": "warning only if CV text is missing, too short or not meaningful; otherwise empty string",
  "isMock": false
}

Rules:
- Return JSON only.
- Do not invent degrees, companies, certifications or work experience that are not in the CV.
- If the CV is weak or missing, say that tailoring cannot be done properly yet and focus on improving content first.
- All user-facing values must respect the requested language.
""";
        }

        private static string BuildCvJobMatchPrompt(CvJobMatchRequestDto request)
        {
            var jobTitle = DefaultText(request.JobTitle, AiLanguageHelper.DefaultJobTitle(request.Language));
            var company = DefaultText(request.Company, AiLanguageHelper.DefaultCompany(request.Language));
            var location = DefaultText(request.Location, AiLanguageHelper.DefaultLocation(request.Language));
            var jobDescription = Limit(DefaultText(request.JobDescription, AiLanguageHelper.DefaultJobDescription(request.Language)), 1800);
            var cvText = Limit(DefaultText(request.CurrentCvText, AiLanguageHelper.DefaultCvText(request.Language)), 2500);
            var languageInstruction = AiLanguageHelper.GetLanguageInstruction(request.Language);

            return $$"""
You are an assistant for a CV and career preparation mobile app.
{{languageInstruction}}
Evaluate how well the CV matches the job.

Job title: {{jobTitle}}
Company: {{company}}
Location: {{location}}
Job description: {{jobDescription}}

Current CV text:
{{cvText}}

Return exactly one valid JSON object matching this C# DTO shape:
{
  "jobTitle": "{{EscapeForPrompt(jobTitle)}}",
  "company": "{{EscapeForPrompt(company)}}",
  "matchScore": 0,
  "recommendation": "Strong match - apply with this CV / Good match - improve the CV before applying / Partial match - tailor the CV first / Weak match - improve the CV significantly first",
  "summary": "brief explanation of the score",
  "strengths": ["strength 1", "strength 2", "strength 3"],
  "missingSkills": ["missing skill or weak area 1", "missing skill or weak area 2"],
  "improvements": ["improvement 1", "improvement 2", "improvement 3"],
  "isMock": false
}

Rules:
- Return JSON only.
- matchScore must be an integer from 0 to 100.
- Do not invent experience. Base the score only on the job and CV text provided.
- All user-facing values must respect the requested language.
""";
        }

        private static CvTailoringResponseDto BuildMockCvTailoring(CvTailoringRequestDto request)
        {
            var title = DefaultText(request.JobTitle, AiLanguageHelper.DefaultJobTitle(request.Language));
            var company = DefaultText(request.Company, AiLanguageHelper.DefaultCompany(request.Language));

            if (AiLanguageHelper.IsRomanian(request.Language))
            {
                return new CvTailoringResponseDto
                {
                    JobTitle = title,
                    Company = company,
                    TailoredProfileSummary = $"Candidat motivat pentru rolul {title} la {company}, cu competențe relevante, adaptabilitate și abilități bune de comunicare.",
                    ImportantKeywords = new List<string> { "comunicare", "rezolvare de probleme", "lucru în echipă", "adaptabilitate", "atenție la detalii" },
                    SkillsToHighlight = new List<string> { "Competențe tehnice relevante", "Comunicare", "Rezolvare de probleme", "Colaborare în echipă" },
                    ExperienceToEmphasize = new List<string>
                    {
                        "Menționează proiecte sau sarcini care se potrivesc responsabilităților din descrierea jobului.",
                        "Folosește exemple măsurabile acolo unde este posibil.",
                        "Evidențiază experiența care arată că te poți adapta rapid la rol."
                    },
                    Suggestions = new List<CvTailoringSuggestionDto>
                    {
                        new CvTailoringSuggestionDto
                        {
                            Section = "Profil",
                            Suggestion = "Rescrie secțiunea de profil astfel încât să menționeze rolul vizat și cele mai relevante puncte forte.",
                            Reason = "Recrutorii citesc de obicei profilul primul, deci acesta trebuie să se potrivească rapid cu jobul."
                        },
                        new CvTailoringSuggestionDto
                        {
                            Section = "Competențe",
                            Suggestion = "Mută competențele cele mai relevante pentru acest job aproape de începutul secțiunii de competențe.",
                            Reason = "CV-ul devine mai ușor de scanat și mai relevant pentru rol."
                        }
                    },
                    CvQualityWarning = string.IsNullOrWhiteSpace(request.CurrentCvText) || request.CurrentCvText.Trim().Length < 120
                        ? "CV-ul selectat are prea puțin text, deci sugestiile sunt în mare parte generale. Completează CV-ul înainte de a-l adapta serios pentru job."
                        : string.Empty,
                    IsMock = true
                };
            }

            return new CvTailoringResponseDto
            {
                JobTitle = title,
                Company = company,
                TailoredProfileSummary = $"Motivated candidate interested in the {title} role at {company}, with relevant skills, adaptability and strong communication abilities.",
                ImportantKeywords = new List<string> { "communication", "problem solving", "teamwork", "adaptability", "attention to detail" },
                SkillsToHighlight = new List<string> { "Relevant technical skills", "Communication", "Problem solving", "Team collaboration" },
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
                    }
                },
                CvQualityWarning = string.IsNullOrWhiteSpace(request.CurrentCvText) || request.CurrentCvText.Trim().Length < 120
                    ? "The selected CV has too little text, so the suggestions are mostly general. Complete the CV before tailoring it seriously."
                    : string.Empty,
                IsMock = true
            };
        }

        private static string DefaultText(string? value, string fallback) =>
            string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();

        private static string Limit(string value, int maxLength) =>
            value.Length <= maxLength ? value : value[..maxLength];

        private static string EscapeForPrompt(string value) =>
            value.Replace("\\", "\\\\").Replace("\"", "\\\"");

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

        private class GeminiGenerateContentRequest
        {
            public List<GeminiContent> Contents { get; set; } = new();
            public GeminiGenerationConfig GenerationConfig { get; set; } = new();
        }

        private class GeminiContent
        {
            public string Role { get; set; } = string.Empty;
            public List<GeminiPart> Parts { get; set; } = new();
        }

        private class GeminiPart
        {
            public string Text { get; set; } = string.Empty;
        }

        private class GeminiGenerationConfig
        {
            public double Temperature { get; set; }
            public int MaxOutputTokens { get; set; }
            public string ResponseMimeType { get; set; } = "application/json";
        }

        private class GeminiGenerateContentResponse
        {
            public List<GeminiCandidate>? Candidates { get; set; }
        }

        private class GeminiCandidate
        {
            public GeminiContent? Content { get; set; }
        }
        public async Task<CoverLetterResponseDto> GenerateCoverLetterAsync(CoverLetterRequestDto request)
        {
            var result = await GenerateJsonAsync<CoverLetterResponseDto>(BuildCoverLetterPrompt(request));

            if (result == null)
                return await _mockAiService.GenerateCoverLetterAsync(request);

            result.JobTitle = string.IsNullOrWhiteSpace(result.JobTitle)
                ? request.JobTitle
                : result.JobTitle;

            result.Company = string.IsNullOrWhiteSpace(result.Company)
                ? request.Company
                : result.Company;

            result.Subject ??= string.Empty;
            result.Letter ??= string.Empty;
            result.IsMock = false;

            return result;
        }
        private static string BuildCoverLetterPrompt(CoverLetterRequestDto request)
        {
            var jobTitle = DefaultText(request.JobTitle, AiLanguageHelper.DefaultJobTitle(request.Language));
            var company = DefaultText(request.Company, AiLanguageHelper.DefaultCompany(request.Language));
            var location = DefaultText(request.Location, AiLanguageHelper.DefaultLocation(request.Language));
            var jobDescription = Limit(DefaultText(request.JobDescription, AiLanguageHelper.DefaultJobDescription(request.Language)), 1600);
            var cvText = Limit(DefaultText(request.CvText, AiLanguageHelper.DefaultCvText(request.Language)), 2200);
            var languageInstruction = AiLanguageHelper.GetLanguageInstruction(request.Language);

            return $$"""
You are an assistant for a CV and career preparation mobile app.
{{languageInstruction}}
Generate a professional cover letter for this job application.

Job title: {{jobTitle}}
Company: {{company}}
Location: {{location}}

Job description or application notes:
{{jobDescription}}

Candidate CV information:
{{cvText}}

Return exactly one valid JSON object matching this C# DTO shape:
{
  "jobTitle": "{{EscapeForPrompt(jobTitle)}}",
  "company": "{{EscapeForPrompt(company)}}",
  "subject": "short email subject",
  "letter": "cover letter text",
  "isMock": false
}

Rules:
- Return JSON only.
- Do not include markdown.
- Keep the cover letter between 180 and 280 words.
- Make it professional, clear and not exaggerated.
- Do not invent degrees, companies, technologies or experience that are not present in the CV text.
- If the CV text is weak or missing, keep the letter general but still useful.
- All user-facing values must respect the requested language.
""";
        }
    }
}
