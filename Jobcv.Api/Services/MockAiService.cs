
using JobCv.Api.Dtos;

namespace JobCv.Api.Services
{
    public class MockAiService
    {
        public Task<InterviewPrepResponseDto> GenerateInterviewPrepAsync(InterviewPrepRequestDto request)
        {
            var text = $"{request.JobTitle} {request.Description}".ToLower();

            var skills = DetectSkills(text);

            var response = new InterviewPrepResponseDto
            {
                JobTitle = string.IsNullOrWhiteSpace(request.JobTitle)
                    ? "Selected job"
                    : request.JobTitle,

                Company = string.IsNullOrWhiteSpace(request.Company)
                    ? "Unknown company"
                    : request.Company,

                Summary = BuildSummary(request, skills),

                KeySkills = skills,

                Questions = BuildQuestions(request, text, skills),

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

        private static string BuildSummary(InterviewPrepRequestDto request, List<string> skills)
        {
            var company = string.IsNullOrWhiteSpace(request.Company)
                ? "this company"
                : request.Company;

            var title = string.IsNullOrWhiteSpace(request.JobTitle)
                ? "this role"
                : request.JobTitle;

            var skillText = skills.Count == 0
                ? "the main requirements from the job description"
                : string.Join(", ", skills.Take(4));

            return $"This interview preparation is focused on the {title} role at {company}. " +
                   $"The candidate should be ready to discuss {skillText}, previous relevant experience, " +
                   $"and how their background matches the responsibilities of the role.";
        }

        private static List<InterviewQuestionDto> BuildQuestions(
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
        public Task<CoverLetterResponseDto> GenerateCoverLetterAsync(CoverLetterRequestDto request)
        {
            var jobTitle = string.IsNullOrWhiteSpace(request.JobTitle)
                ? "the selected role"
                : request.JobTitle.Trim();

            var company = string.IsNullOrWhiteSpace(request.Company)
                ? "your company"
                : request.Company.Trim();

            var response = new CoverLetterResponseDto
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
            };

            return Task.FromResult(response);
        }
    }
}