using System.Text.RegularExpressions;
using JobCv.Api.Dtos;

namespace JobCv.Api.Services
{
    public class MockAiService
    {
        public Task<InterviewPrepResponseDto> GenerateInterviewPrepAsync(InterviewPrepRequestDto request)
        {
            if (AiLanguageHelper.IsRomanian(request.Language))
                return Task.FromResult(BuildRomanianInterviewPrep(request));

            var text = $"{request.JobTitle} {request.Description}".ToLowerInvariant();
            var skills = DetectSkills(text);

            var response = new InterviewPrepResponseDto
            {
                JobTitle = string.IsNullOrWhiteSpace(request.JobTitle)
                    ? "Selected job"
                    : request.JobTitle.Trim(),

                Company = string.IsNullOrWhiteSpace(request.Company)
                    ? "Unknown company"
                    : request.Company.Trim(),

                Summary = BuildInterviewSummary(request, skills),

                KeySkills = skills,

                Questions = BuildInterviewQuestions(request, text, skills),

                BeforeInterviewTips = new List<string>
                {
                    "Review the job description and identify the most important requirements.",
                    "Prepare one or two examples from your CV that match this role.",
                    "Research the company before the interview.",
                    "Practice explaining your previous projects clearly and briefly.",
                    "Prepare questions to ask the interviewer about the team, technologies and expectations."
                },

                IsMock = true
            };

            return Task.FromResult(response);
        }

        public Task<CvTailoringResponseDto> GenerateCvTailoringAsync(CvTailoringRequestDto request)
        {
            CvTailoringResponseDto result;

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

            return Task.FromResult(result);
        }

        public CvQualityCheckResponseDto CheckCvQuality(CvQualityCheckRequestDto request)
        {
            var result = BuildCvQualityCheck(request);

            return AiLanguageHelper.LocalizeCvQualityResponse(result, request.Language);
        }

        public Task<CvJobMatchResponseDto> GenerateCvJobMatchAsync(CvJobMatchRequestDto request)
        {
            var result = BuildExplainableCvJobMatch(request);

            var localizedResult =
                AiLanguageHelper.LocalizeCvJobMatchResponse(result, request.Language);

            return Task.FromResult(localizedResult);
        }

        public Task<CoverLetterResponseDto> GenerateCoverLetterAsync(CoverLetterRequestDto request)
        {
            var romanian = AiLanguageHelper.IsRomanian(request.Language);

            var jobTitle = string.IsNullOrWhiteSpace(request.JobTitle)
                ? romanian ? "rolul selectat" : "the selected role"
                : request.JobTitle.Trim();

            var company = string.IsNullOrWhiteSpace(request.Company)
                ? romanian ? "compania dumneavoastră" : "your company"
                : request.Company.Trim();

            if (romanian)
            {
                return Task.FromResult(new CoverLetterResponseDto
                {
                    JobTitle = jobTitle,
                    Company = company,
                    Subject = $"Aplicare pentru {jobTitle}",
                    Letter =
$@"Bună ziua,

Vă scriu pentru a-mi exprima interesul pentru poziția {jobTitle} în cadrul {company}. Consider că acest rol se potrivește cu profilul meu, motivația mea și interesul meu pentru dezvoltarea de soluții practice.

Experiența și competențele mele m-au ajutat să îmi formez o bază solidă în rezolvarea problemelor, comunicare și lucrul cu proiecte orientate spre tehnologie. Sunt interesat(ă) în special de oportunități în care pot continua să învăț, să contribui în cadrul unei echipe și să aplic cunoștințele în contexte reale.

Mi-ar plăcea să discutăm despre modul în care profilul meu se poate potrivi nevoilor echipei dumneavoastră. Vă mulțumesc pentru timpul acordat și pentru oportunitate.

Cu stimă,",
                    IsMock = true
                });
            }

            return Task.FromResult(new CoverLetterResponseDto
            {
                JobTitle = jobTitle,
                Company = company,
                Subject = $"Application for {jobTitle}",
                Letter =
$@"Dear Hiring Team,

I am writing to express my interest in the {jobTitle} position at {company}. I believe this role is a strong match for my background, motivation, and interest in developing practical solutions.

My experience and skills have helped me build a solid foundation in problem solving, communication, and working with technology-focused projects. I am especially interested in opportunities where I can continue learning, contribute to a team, and apply my knowledge to real-world challenges.

I would appreciate the opportunity to discuss how my profile could fit the needs of your team. Thank you for your time and consideration.

Kind regards,",
                IsMock = true
            });
        }

        private static InterviewPrepResponseDto BuildRomanianInterviewPrep(InterviewPrepRequestDto request)
        {
            var text = $"{request.JobTitle} {request.Description}".ToLowerInvariant();
            var skills = DetectSkills(text);

            var jobTitle = string.IsNullOrWhiteSpace(request.JobTitle)
                ? "jobul selectat"
                : request.JobTitle.Trim();

            var company = string.IsNullOrWhiteSpace(request.Company)
                ? "compania selectată"
                : request.Company.Trim();

            var skillText = skills.Count == 0
                ? "cerințele principale din descrierea jobului"
                : string.Join(", ", skills.Take(4));

            var questions = new List<InterviewQuestionDto>
            {
                new InterviewQuestionDto
                {
                    Category = "General",
                    Question = "Îmi poți spune pe scurt despre tine și de ce te interesează acest rol?",
                    SuggestedAnswer = "Prezintă pe scurt experiența ta, menționează competențele relevante și explică de ce rolul se potrivește cu obiectivele tale profesionale."
                },
                new InterviewQuestionDto
                {
                    Category = "General",
                    Question = "De ce vrei să lucrezi pentru această companie?",
                    SuggestedAnswer = "Menționează ce știi despre companie, ce te atrage la rol și cum poți contribui prin competențele tale."
                },
                new InterviewQuestionDto
                {
                    Category = "CV",
                    Question = "Care proiect din CV-ul tău este cel mai relevant pentru această poziție?",
                    SuggestedAnswer = "Alege un proiect puternic, explică problema, contribuția ta, tehnologiile folosite și rezultatul obținut."
                }
            };

            if (skills.Contains("C#") || skills.Contains(".NET"))
            {
                questions.Add(new InterviewQuestionDto
                {
                    Category = "Tehnic",
                    Question = "Ce este dependency injection și de ce este util în aplicațiile .NET?",
                    SuggestedAnswer = "Explică faptul că dependency injection ajută la gestionarea dependențelor, crește testabilitatea și menține codul mai modular."
                });
            }

            if (skills.Contains("REST APIs"))
            {
                questions.Add(new InterviewQuestionDto
                {
                    Category = "Tehnic",
                    Question = "Cum ai proiecta un endpoint REST pentru crearea unei resurse noi?",
                    SuggestedAnswer = "Menționează folosirea HTTP POST, validarea datelor, coduri de status potrivite și returnarea resursei create sau a identificatorului ei."
                });
            }

            if (skills.Contains("SQL"))
            {
                questions.Add(new InterviewQuestionDto
                {
                    Category = "Tehnic",
                    Question = "Cum ai investiga o interogare SQL lentă?",
                    SuggestedAnswer = "Menționează verificarea indexurilor, structura query-ului, planul de execuție, condițiile de filtrare și reducerea datelor încărcate inutil."
                });
            }

            questions.Add(new InterviewQuestionDto
            {
                Category = "Comportamental",
                Question = "Povestește despre un moment în care a trebuit să înveți rapid ceva nou.",
                SuggestedAnswer = "Folosește un exemplu concret. Explică situația, ce ai învățat, cum ai aplicat informația și care a fost rezultatul."
            });

            questions.Add(new InterviewQuestionDto
            {
                Category = "Final",
                Question = "Ai întrebări pentru noi?",
                SuggestedAnswer = "Întreabă despre echipă, tehnologii, procesul de onboarding, așteptările pentru primele luni și oportunitățile de dezvoltare."
            });

            return new InterviewPrepResponseDto
            {
                JobTitle = jobTitle,
                Company = company,
                Summary = $"Această pregătire pentru interviu este orientată către rolul {jobTitle} la {company}. Candidatul ar trebui să fie pregătit să discute despre {skillText}, experiența relevantă și modul în care profilul său se potrivește responsabilităților rolului.",
                KeySkills = skills,
                Questions = questions.Take(10).ToList(),
                BeforeInterviewTips = new List<string>
                {
                    "Recitește descrierea jobului și identifică cerințele cele mai importante.",
                    "Pregătește unul sau două exemple din CV care se potrivesc acestui rol.",
                    "Informează-te despre companie înainte de interviu.",
                    "Exersează explicarea proiectelor tale clar și pe scurt.",
                    "Pregătește întrebări despre echipă, tehnologii și așteptări."
                },
                IsMock = true
            };
        }

        private static List<string> DetectSkills(string text)
        {
            var skills = new List<string>();

            AddIfContains(skills, text, "C#", "c#", "csharp");
            AddIfContains(skills, text, ".NET", ".net", "asp.net", "dotnet");
            AddIfContains(skills, text, "REST APIs", "rest", "api", "apis");
            AddIfContains(skills, text, "SQL", "sql", "database", "databases");
            AddIfContains(skills, text, "Entity Framework", "entity framework", "ef core");
            AddIfContains(skills, text, "JavaScript", "javascript", "js");
            AddIfContains(skills, text, "React", "react");
            AddIfContains(skills, text, "Mobile Development", "mobile", "android", "ios", "maui");
            AddIfContains(skills, text, "Testing", "test", "testing", "qa");
            AddIfContains(skills, text, "Communication", "communication", "collaborate", "team");

            if (skills.Count == 0)
            {
                skills.AddRange(new[]
                {
                    "Problem solving",
                    "Communication",
                    "Attention to detail",
                    "Team collaboration"
                });
            }

            return skills.Take(8).ToList();
        }

        private static void AddIfContains(
            List<string> skills,
            string text,
            string skillName,
            params string[] keywords)
        {
            if (skills.Contains(skillName))
                return;

            if (keywords.Any(text.Contains))
                skills.Add(skillName);
        }

        private static string BuildInterviewSummary(InterviewPrepRequestDto request, List<string> skills)
        {
            var company = string.IsNullOrWhiteSpace(request.Company)
                ? "this company"
                : request.Company.Trim();

            var title = string.IsNullOrWhiteSpace(request.JobTitle)
                ? "this role"
                : request.JobTitle.Trim();

            var skillText = skills.Count == 0
                ? "the main requirements from the job description"
                : string.Join(", ", skills.Take(4));

            return $"This interview preparation is focused on the {title} role at {company}. " +
                   $"The candidate should be ready to discuss {skillText}, previous relevant experience, " +
                   $"and how their background matches the responsibilities of the role.";
        }

        private static List<InterviewQuestionDto> BuildInterviewQuestions(
            InterviewPrepRequestDto request,
            string text,
            List<string> skills)
        {
            var questions = new List<InterviewQuestionDto>
            {
                new InterviewQuestionDto
                {
                    Category = "General",
                    Question = "Can you tell me about yourself and why you are interested in this role?",
                    SuggestedAnswer = "Give a short summary of your background, mention relevant skills, and explain why this role matches your career goals."
                },
                new InterviewQuestionDto
                {
                    Category = "General",
                    Question = "Why do you want to work for this company?",
                    SuggestedAnswer = "Mention what you know about the company, the role, and how your skills can contribute to their work."
                },
                new InterviewQuestionDto
                {
                    Category = "CV",
                    Question = "Which project from your CV is most relevant for this position?",
                    SuggestedAnswer = "Choose one strong project, explain the problem, your contribution, technologies used, and the result."
                }
            };

            if (skills.Contains("C#") || skills.Contains(".NET"))
            {
                questions.Add(new InterviewQuestionDto
                {
                    Category = "Technical",
                    Question = "What is dependency injection and why is it useful in .NET applications?",
                    SuggestedAnswer = "Dependency injection helps manage dependencies between classes, improves testability, and keeps the code more modular."
                });
            }

            if (skills.Contains("REST APIs"))
            {
                questions.Add(new InterviewQuestionDto
                {
                    Category = "Technical",
                    Question = "How would you design a REST API endpoint for creating a new resource?",
                    SuggestedAnswer = "Explain the use of HTTP POST, request body validation, appropriate status codes, and returning the created resource or its identifier."
                });
            }

            if (skills.Contains("SQL"))
            {
                questions.Add(new InterviewQuestionDto
                {
                    Category = "Technical",
                    Question = "How would you investigate a slow database query?",
                    SuggestedAnswer = "Mention checking indexes, query structure, execution plans, filtering conditions, and reducing unnecessary data loading."
                });
            }

            if (skills.Contains("JavaScript") || skills.Contains("React"))
            {
                questions.Add(new InterviewQuestionDto
                {
                    Category = "Technical",
                    Question = "How do you make a user interface responsive and easy to use?",
                    SuggestedAnswer = "Mention layout structure, reusable components, responsive design, accessibility, and testing on different screen sizes."
                });
            }

            if (skills.Contains("Testing"))
            {
                questions.Add(new InterviewQuestionDto
                {
                    Category = "Testing",
                    Question = "How do you approach testing a new feature?",
                    SuggestedAnswer = "Explain that you verify requirements, test common and edge cases, report issues clearly, and retest after fixes."
                });
            }

            questions.Add(new InterviewQuestionDto
            {
                Category = "Behavioural",
                Question = "Tell me about a time when you had to learn something quickly.",
                SuggestedAnswer = "Use a concrete example. Explain the situation, what you learned, how you applied it, and the final result."
            });

            questions.Add(new InterviewQuestionDto
            {
                Category = "Behavioural",
                Question = "How do you handle feedback or code review comments?",
                SuggestedAnswer = "Explain that you see feedback as a way to improve, ask clarifying questions when needed, and apply changes carefully."
            });

            questions.Add(new InterviewQuestionDto
            {
                Category = "Closing",
                Question = "Do you have any questions for us?",
                SuggestedAnswer = "Ask about the team, technologies, onboarding process, expectations for the first months, and opportunities to grow."
            });

            return questions.Take(10).ToList();
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

            CheckEmailFormat(issues, ref score, request.Email);

            CheckTextSection(
                issues,
                ref score,
                "Phone",
                request.Phone,
                5,
                6,
                "The CV does not contain a phone number.",
                "Add a phone number if you want recruiters to contact you faster.");

            CheckPhoneFormat(issues, ref score, request.Phone);

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
            CheckDuplicateSkills(issues, ref score, request.Skills);
            CheckGenericSummary(issues, ref score, request.Summary);
            CheckGenericSkills(issues, ref score, request.Skills);

            CheckShortListItems(
                issues,
                ref score,
                "Experience",
                request.Experiences,
                80,
                "Some experience entries appear too short.",
                "Add responsibilities, technologies used and results obtained for each experience.");

            CheckShortListItems(
                issues,
                ref score,
                "Projects",
                request.Projects,
                70,
                "Some project entries appear too short.",
                "Add a clearer description, technologies used and the purpose of each project.");

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
                "asdf",
                "example",
                "your name",
                "untitled",
                "placeholder"
            };

            var containsPlaceholder = placeholders.Any(placeholder =>
                Regex.IsMatch(
                    allText,
                    $@"(^|\W){Regex.Escape(placeholder)}($|\W)",
                    RegexOptions.IgnoreCase));

            var containsTestWord = Regex.IsMatch(allText, @"(^|\W)test($|\W)", RegexOptions.IgnoreCase);

            if (containsPlaceholder || containsTestWord)
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

        private static void CheckEmailFormat(
            List<CvQualityIssueDto> issues,
            ref int score,
            string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return;

            var trimmedEmail = email.Trim();

            var isValid = Regex.IsMatch(
                trimmedEmail,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$");

            if (!isValid)
            {
                issues.Add(new CvQualityIssueDto
                {
                    Section = "Email",
                    Problem = "The email address does not appear to have a valid format.",
                    Suggestion = "Check that the email contains a username, @ symbol and domain, for example name@example.com.",
                    Severity = "High"
                });

                score -= 10;
            }
        }

        private static void CheckPhoneFormat(
            List<CvQualityIssueDto> issues,
            ref int score,
            string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return;

            var digitCount = phone.Count(char.IsDigit);

            if (digitCount < 7)
            {
                issues.Add(new CvQualityIssueDto
                {
                    Section = "Phone",
                    Problem = "The phone number seems incomplete.",
                    Suggestion = "Add a phone number with enough digits so recruiters can contact you correctly.",
                    Severity = "Medium"
                });

                score -= 6;
            }
        }

        private static void CheckDuplicateSkills(
            List<CvQualityIssueDto> issues,
            ref int score,
            List<string>? skills)
        {
            if (skills == null || skills.Count == 0)
                return;

            var duplicateSkills = skills
                .Where(skill => !string.IsNullOrWhiteSpace(skill))
                .Select(skill => skill.Trim())
                .GroupBy(skill => skill.ToLowerInvariant())
                .Where(group => group.Count() > 1)
                .Select(group => group.First())
                .Take(3)
                .ToList();

            if (duplicateSkills.Count == 0)
                return;

            issues.Add(new CvQualityIssueDto
            {
                Section = "Skills",
                Problem = "Some skills appear more than once.",
                Suggestion = $"Remove duplicate skills such as: {string.Join(", ", duplicateSkills)}.",
                Severity = "Low"
            });

            score -= 4;
        }

        private static void CheckGenericSummary(
            List<CvQualityIssueDto> issues,
            ref int score,
            string? summary)
        {
            if (string.IsNullOrWhiteSpace(summary))
                return;

            var normalizedSummary = summary.ToLowerInvariant();

            var genericPhrases = new[]
            {
                "i want a job",
                "looking for a job",
                "looking for opportunities",
                "hardworking person",
                "motivated person",
                "serious person",
                "punctual person",
                "i am hardworking",
                "i am motivated"
            };

            if (!genericPhrases.Any(normalizedSummary.Contains))
                return;

            issues.Add(new CvQualityIssueDto
            {
                Section = "Profile summary",
                Problem = "The profile summary contains very generic wording.",
                Suggestion = "Add specific details about your role, experience level, technologies and career objective.",
                Severity = "Medium"
            });

            score -= 8;
        }

        private static void CheckGenericSkills(
            List<CvQualityIssueDto> issues,
            ref int score,
            List<string>? skills)
        {
            if (skills == null || skills.Count == 0)
                return;

            var genericSkills = new[]
            {
                "computer",
                "internet",
                "office",
                "communication",
                "teamwork",
                "hardworking",
                "serious",
                "punctual"
            };

            var normalizedSkills = skills
                .Where(skill => !string.IsNullOrWhiteSpace(skill))
                .Select(skill => skill.Trim().ToLowerInvariant())
                .ToList();

            var genericCount = normalizedSkills.Count(skill =>
                genericSkills.Any(genericSkill => skill.Contains(genericSkill)));

            if (genericCount < 3)
                return;

            issues.Add(new CvQualityIssueDto
            {
                Section = "Skills",
                Problem = "Several skills appear to be too general.",
                Suggestion = "Add more specific technical or role-related skills that match the jobs you want to apply for.",
                Severity = "Medium"
            });

            score -= 8;
        }

        private static void CheckShortListItems(
            List<CvQualityIssueDto> issues,
            ref int score,
            string section,
            List<string>? values,
            int minimumLength,
            string problem,
            string suggestion)
        {
            if (values == null || values.Count == 0)
                return;

            var hasShortItems = values
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Any(value => value.Trim().Length < minimumLength);

            if (!hasShortItems)
                return;

            issues.Add(new CvQualityIssueDto
            {
                Section = section,
                Problem = problem,
                Suggestion = suggestion,
                Severity = "Medium"
            });

            score -= 8;
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
                : matchedKeywords
                    .Select(keyword => $"The CV mentions {keyword}, which appears relevant for this role.")
                    .ToList();

            var missing = missingKeywords.Count == 0
                ? new List<string>
                {
                    "No major missing keywords were detected from the available job description."
                }
                : missingKeywords
                    .Select(keyword => $"The job description mentions {keyword}, but it is not clear in the CV.")
                    .ToList();

            var improvements = new List<string>
            {
                "Rewrite the profile summary so it directly targets this job title and company.",
                "Move the most relevant skills near the top of the CV.",
                "Add project or experience descriptions that prove the required skills.",
                "Use keywords from the job description naturally, without copying the entire text."
            };

            if (cvText.Trim().Length < 120)
            {
                improvements.Insert(
                    0,
                    "The selected CV contains very little text. Add summary, skills, experience, projects and education before applying.");
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

            if (text.Contains("summary") || text.Contains("profile"))
                bonus += 4;

            if (text.Contains("skills"))
                bonus += 4;

            if (text.Contains("experience"))
                bonus += 4;

            if (text.Contains("project"))
                bonus += 4;

            if (text.Contains("education"))
                bonus += 4;

            if (cvText.Length < 120)
                bonus -= 20;
            else if (cvText.Length < 400)
                bonus -= 8;
            else if (cvText.Length > 900)
                bonus += 5;

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
    }
}