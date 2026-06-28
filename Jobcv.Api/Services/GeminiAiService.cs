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
        private readonly ILogger<GeminiAiService> _logger;

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public GeminiAiService(
            HttpClient httpClient,
            IConfiguration configuration,
            MockAiService mockAiService,
            ILogger<GeminiAiService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _mockAiService = mockAiService;
            _logger = logger;
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
            {
                _logger.LogWarning("Gemini request skipped because Gemini:ApiKey is missing.");
                return default;
            }

            var model = _configuration["Gemini:Model"] ?? "gemini-2.5-flash-lite";
            var url = $"v1beta/models/{WebUtility.UrlEncode(model)}:generateContent";

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
                    MaxOutputTokens = 3000,
                    ResponseMimeType = "application/json"
                }
            };

            try
            {
                using var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
                requestMessage.Headers.TryAddWithoutValidation("x-goog-api-key", apiKey);
                requestMessage.Content = JsonContent.Create(geminiRequest, options: _jsonOptions);

                using var response = await _httpClient.SendAsync(requestMessage);
                var rawResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "Gemini returned an error. StatusCode: {StatusCode}. Body: {Body}",
                        response.StatusCode,
                        Truncate(rawResponse, 2000));

                    return default;
                }

                GeminiGenerateContentResponse? geminiResponse;

                try
                {
                    geminiResponse = JsonSerializer.Deserialize<GeminiGenerateContentResponse>(rawResponse, _jsonOptions);
                }
                catch (JsonException ex)
                {
                    _logger.LogError(
                        ex,
                        "Could not deserialize Gemini HTTP response. Body: {Body}",
                        Truncate(rawResponse, 2000));

                    return default;
                }

                var text = geminiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

                if (string.IsNullOrWhiteSpace(text))
                {
                    _logger.LogWarning(
                        "Gemini response did not contain candidate text. Body: {Body}",
                        Truncate(rawResponse, 2000));

                    return default;
                }

                var cleanedJson = CleanJson(text);

                try
                {
                    return JsonSerializer.Deserialize<T>(cleanedJson, _jsonOptions);
                }
                catch (JsonException ex)
                {
                    _logger.LogError(
                        ex,
                        "Gemini returned text, but it was not valid JSON for {TargetType}. Text: {Text}",
                        typeof(T).Name,
                        Truncate(cleanedJson, 2000));

                    return default;
                }
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Gemini request timed out.");
                return default;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gemini request failed unexpectedly.");
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

        private static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            return value.Length <= maxLength ? value : value[..maxLength];
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
        private static string BuildCareerRecommendationsPrompt(CareerAiRecommendationsRequestDto request)
        {
            var languageInstruction = AiLanguageHelper.GetLanguageInstruction(request.Language);

            var scoresText = string.Join("\n", request.AreaScores.Select(score =>
                $"- {score.AreaCode} / {score.Area}: raw score {score.RawScore}/{score.MaxScore}, percentage {score.Score}%, description: {score.Description}"));

            var profileTitle = DefaultText(request.ProfileTitle, "RIASEC career interest profile");
            var profileCode = DefaultText(request.ProfileCode, "unknown");
            var summary = Limit(DefaultText(request.Summary, "No summary provided."), 1200);

            return $$"""
You are a career guidance assistant inside a mobile app.
{{languageInstruction}}

The user completed a career interest test based on the RIASEC model.
Use the profile and scores below to generate practical career orientation recommendations.

Important:
- Do not claim this is an official psychological diagnosis.
- Do not claim the recommendations are official O*NET career matches.
- Explain that recommendations are orientation suggestions based on the RIASEC profile.
- Keep the tone practical, supportive and realistic.
- Do not invent personal information about the user.
- Recommend broad career directions and example roles.

Profile title: {{profileTitle}}
RIASEC profile code: {{profileCode}}

Summary:
{{summary}}

Scores:
{{scoresText}}

Return exactly one valid JSON object matching this C# DTO shape:
{
  "summary": "short personalized explanation of the RIASEC profile",
  "careerDirections": [
    "career direction 1",
    "career direction 2",
    "career direction 3"
  ],
  "recommendedRoles": [
    {
      "title": "example role title",
      "reason": "why this role may fit the profile"
    },
    {
      "title": "example role title",
      "reason": "why this role may fit the profile"
    }
  ],
  "skillsToDevelop": [
    "skill 1",
    "skill 2",
    "skill 3"
  ],
  "nextSteps": [
    "step 1",
    "step 2",
    "step 3"
  ],
  "disclaimer": "short disclaimer that the result is orientation only",
  "isMock": false
}

Rules:
- Return JSON only.
- recommendedRoles should contain 5 to 8 roles.
- careerDirections should contain 3 to 5 items.
- skillsToDevelop should contain 4 to 6 items.
- nextSteps should contain 4 to 6 items.
- All user-facing values must respect the requested language.
""";
        }

        private static CareerAiRecommendationsResponseDto BuildFallbackCareerRecommendations(
            CareerAiRecommendationsRequestDto request)
        {
            var topAreas = request.AreaScores
                .OrderByDescending(x => x.RawScore)
                .ThenByDescending(x => x.Score)
                .Take(3)
                .ToList();

            var topCodes = topAreas.Select(x => x.AreaCode).ToHashSet();

            if (AiLanguageHelper.IsRomanian(request.Language))
            {
                var responseRo = new CareerAiRecommendationsResponseDto
                {
                    Summary = $"Profilul {request.ProfileCode} indică interese dominante în ariile {string.Join(", ", topAreas.Select(x => $"{x.AreaCode} - {x.Area}"))}. Recomandările sunt orientative și trebuie comparate cu educația, experiența și obiectivele personale.",
                    Disclaimer = "Recomandările sunt generate pe baza profilului RIASEC și au caracter orientativ. Ele nu reprezintă o evaluare psihologică sau vocațională oficială.",
                    IsMock = true
                };

                if (topCodes.Contains("R"))
                {
                    responseRo.CareerDirections.Add("activități practice și tehnice");
                    responseRo.RecommendedRoles.Add(new CareerAiRecommendedRoleDto
                    {
                        Title = "Tehnician / rol practic aplicat",
                        Reason = "Profilul include interes pentru activități concrete, instrumente, echipamente sau lucru practic."
                    });
                }

                if (topCodes.Contains("I"))
                {
                    responseRo.CareerDirections.Add("analiză, cercetare și rezolvare de probleme");
                    responseRo.RecommendedRoles.Add(new CareerAiRecommendedRoleDto
                    {
                        Title = "Analist / rol de cercetare",
                        Reason = "Profilul include interes pentru investigare, analiză și înțelegerea cauzelor."
                    });
                }

                if (topCodes.Contains("A"))
                {
                    responseRo.CareerDirections.Add("creație, design și comunicare vizuală");
                    responseRo.RecommendedRoles.Add(new CareerAiRecommendedRoleDto
                    {
                        Title = "Designer / creator de conținut",
                        Reason = "Profilul include interes pentru exprimare creativă și activități imaginative."
                    });
                }

                if (topCodes.Contains("S"))
                {
                    responseRo.CareerDirections.Add("educație, sprijin și lucru cu oamenii");
                    responseRo.RecommendedRoles.Add(new CareerAiRecommendedRoleDto
                    {
                        Title = "Trainer / specialist suport",
                        Reason = "Profilul include interes pentru ajutor, predare, consiliere sau lucru direct cu persoane."
                    });
                }

                if (topCodes.Contains("E"))
                {
                    responseRo.CareerDirections.Add("business, vânzări și coordonare");
                    responseRo.RecommendedRoles.Add(new CareerAiRecommendedRoleDto
                    {
                        Title = "Coordonator proiect / business development",
                        Reason = "Profilul include interes pentru persuasiune, organizare, negociere și luarea deciziilor."
                    });
                }

                if (topCodes.Contains("C"))
                {
                    responseRo.CareerDirections.Add("administrativ, organizare și lucrul cu date");
                    responseRo.RecommendedRoles.Add(new CareerAiRecommendedRoleDto
                    {
                        Title = "Specialist administrativ / raportare",
                        Reason = "Profilul include interes pentru structură, proceduri, evidențe și organizare."
                    });
                }

                responseRo.SkillsToDevelop = new List<string>
        {
            "comunicare profesională",
            "organizare personală",
            "analiză a descrierilor de job",
            "adaptarea CV-ului pentru roluri diferite"
        };

                responseRo.NextSteps = new List<string>
        {
            "Alege 2-3 roluri care par interesante.",
            "Caută anunțuri reale pentru aceste roluri.",
            "Compară cerințele joburilor cu educația și competențele tale actuale.",
            "Actualizează CV-ul pentru direcția profesională pe care vrei să o explorezi."
        };

                return responseRo;
            }

            var responseEn = new CareerAiRecommendationsResponseDto
            {
                Summary = $"The {request.ProfileCode} profile suggests stronger interests in {string.Join(", ", topAreas.Select(x => $"{x.AreaCode} - {x.Area}"))}. These recommendations are orientation suggestions and should be compared with your education, experience and goals.",
                Disclaimer = "These recommendations are generated based on the RIASEC profile and are intended for orientation only. They are not an official psychological or vocational assessment.",
                IsMock = true
            };

            if (topCodes.Contains("R"))
            {
                responseEn.CareerDirections.Add("hands-on technical and practical work");
                responseEn.RecommendedRoles.Add(new CareerAiRecommendedRoleDto
                {
                    Title = "Technician / applied practical role",
                    Reason = "The profile includes interest in concrete tasks, tools, equipment or hands-on work."
                });
            }

            if (topCodes.Contains("I"))
            {
                responseEn.CareerDirections.Add("analysis, research and problem solving");
                responseEn.RecommendedRoles.Add(new CareerAiRecommendedRoleDto
                {
                    Title = "Analyst / research-oriented role",
                    Reason = "The profile includes interest in investigation, analysis and understanding causes."
                });
            }

            if (topCodes.Contains("A"))
            {
                responseEn.CareerDirections.Add("creative work, design and visual communication");
                responseEn.RecommendedRoles.Add(new CareerAiRecommendedRoleDto
                {
                    Title = "Designer / content creator",
                    Reason = "The profile includes interest in creative expression and imaginative activities."
                });
            }

            if (topCodes.Contains("S"))
            {
                responseEn.CareerDirections.Add("education, support and people-focused work");
                responseEn.RecommendedRoles.Add(new CareerAiRecommendedRoleDto
                {
                    Title = "Trainer / support specialist",
                    Reason = "The profile includes interest in helping, teaching, counseling or working directly with people."
                });
            }

            if (topCodes.Contains("E"))
            {
                responseEn.CareerDirections.Add("business, sales and coordination");
                responseEn.RecommendedRoles.Add(new CareerAiRecommendedRoleDto
                {
                    Title = "Project coordinator / business development role",
                    Reason = "The profile includes interest in persuasion, organization, negotiation and decision making."
                });
            }

            if (topCodes.Contains("C"))
            {
                responseEn.CareerDirections.Add("administration, organization and working with data");
                responseEn.RecommendedRoles.Add(new CareerAiRecommendedRoleDto
                {
                    Title = "Administrative / reporting specialist",
                    Reason = "The profile includes interest in structure, procedures, records and organization."
                });
            }

            responseEn.SkillsToDevelop = new List<string>
    {
        "professional communication",
        "personal organization",
        "job description analysis",
        "CV tailoring for different roles"
    };

            responseEn.NextSteps = new List<string>
    {
        "Choose 2-3 roles that seem interesting.",
        "Search for real job descriptions for those roles.",
        "Compare the job requirements with your current education and skills.",
        "Update your CV for the career direction you want to explore."
    };

            return responseEn;
        }
        public async Task<CareerAiRecommendationsResponseDto> GenerateCareerRecommendationsAsync(
    CareerAiRecommendationsRequestDto request)
        {
            var result = await GenerateJsonAsync<CareerAiRecommendationsResponseDto>(
                BuildCareerRecommendationsPrompt(request));

            if (result == null)
                return BuildFallbackCareerRecommendations(request);

            result.Summary ??= string.Empty;
            result.CareerDirections ??= new List<string>();
            result.RecommendedRoles ??= new List<CareerAiRecommendedRoleDto>();
            result.SkillsToDevelop ??= new List<string>();
            result.NextSteps ??= new List<string>();

            if (string.IsNullOrWhiteSpace(result.Disclaimer))
            {
                result.Disclaimer = AiLanguageHelper.IsRomanian(request.Language)
                    ? "Aceste recomandări sunt generate cu AI pe baza scorurilor RIASEC și au caracter orientativ. Ele nu reprezintă o evaluare psihologică sau vocațională oficială."
                    : "These recommendations are AI-generated based on the RIASEC scores and are intended for orientation only. They are not an official psychological or vocational assessment.";
            }

            result.IsMock = false;

            return result;
        }
    }

}
