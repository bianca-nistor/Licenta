using JobCv.Mobile.Models;
using JobCv.Mobile.Services;

namespace JobCv.Mobile.Pages
{
    public partial class CareerTestPage : ContentPage
    {
        private readonly UserDto _user;
        private readonly ApiService _apiService;

        private readonly List<CareerTestQuestion> _questions;
        private readonly Dictionary<int, int> _answers = new();

        private int _currentIndex = 0;

        public CareerTestPage(UserDto user, ApiService apiService)
        {
            InitializeComponent();

            _user = user;
            _apiService = apiService;
            _questions = BuildQuestions();

            ShowQuestion();
        }

        private static List<CareerTestQuestion> BuildQuestions()
        {
            return new List<CareerTestQuestion>
            {
                new() { Id = 1, Area = "Technical / Problem solving", Text = "I enjoy understanding how systems, apps or tools work." },
                new() { Id = 2, Area = "Technical / Problem solving", Text = "I like solving problems step by step until I find a working solution." },
                new() { Id = 3, Area = "Technical / Problem solving", Text = "I feel motivated when I can build or improve something using technology." },
                new() { Id = 4, Area = "Technical / Problem solving", Text = "I am comfortable learning new digital tools or technical concepts." },

                new() { Id = 5, Area = "Analytical / Data thinking", Text = "I like working with numbers, facts or structured information." },
                new() { Id = 6, Area = "Analytical / Data thinking", Text = "Before making a decision, I prefer to compare options and look at evidence." },
                new() { Id = 7, Area = "Analytical / Data thinking", Text = "I enjoy finding patterns, causes or explanations behind a situation." },
                new() { Id = 8, Area = "Analytical / Data thinking", Text = "I prefer tasks where logic and accuracy matter." },

                new() { Id = 9, Area = "Creative / Design thinking", Text = "I enjoy creating visual, written or original ideas." },
                new() { Id = 10, Area = "Creative / Design thinking", Text = "I like improving how something looks, feels or is presented." },
                new() { Id = 11, Area = "Creative / Design thinking", Text = "I feel energized when I can express my own ideas in a project." },
                new() { Id = 12, Area = "Creative / Design thinking", Text = "I prefer tasks that allow flexibility and imagination." },

                new() { Id = 13, Area = "Social / Helping people", Text = "I enjoy helping people understand something or solve a problem." },
                new() { Id = 14, Area = "Social / Helping people", Text = "I am comfortable communicating with different types of people." },
                new() { Id = 15, Area = "Social / Helping people", Text = "I feel satisfied when my work supports or improves someone else's experience." },
                new() { Id = 16, Area = "Social / Helping people", Text = "I prefer work where empathy and communication are important." },

                new() { Id = 17, Area = "Business / Leadership", Text = "I like organizing people, tasks or plans to reach a goal." },
                new() { Id = 18, Area = "Business / Leadership", Text = "I am interested in how companies, products or markets work." },
                new() { Id = 19, Area = "Business / Leadership", Text = "I feel comfortable taking responsibility for decisions." },
                new() { Id = 20, Area = "Business / Leadership", Text = "I like setting goals and tracking progress." },

                new() { Id = 21, Area = "Practical / Organized work", Text = "I prefer clear tasks, structure and practical outcomes." },
                new() { Id = 22, Area = "Practical / Organized work", Text = "I am good at following a process and finishing what I start." },
                new() { Id = 23, Area = "Practical / Organized work", Text = "I like work where responsibilities and expectations are clear." },
                new() { Id = 24, Area = "Practical / Organized work", Text = "I feel productive when I can organize information, documents or activities." }
            };
        }

        private void ShowQuestion()
        {
            var question = _questions[_currentIndex];

            ProgressLabel.Text = $"Question {_currentIndex + 1} of {_questions.Count}";
            QuestionProgressBar.Progress = (double)(_currentIndex + 1) / _questions.Count;
            AreaLabel.Text = question.Area;
            QuestionLabel.Text = question.Text;

            NextButton.Text = _currentIndex == _questions.Count - 1
                ? "See result"
                : "Next";

            MessageLabel.IsVisible = false;
            MessageLabel.Text = "";

            ResetAnswerButtonStyles();

            if (_answers.TryGetValue(question.Id, out var selectedValue))
                HighlightAnswer(selectedValue);
        }

        private void OnAnswerClicked(object sender, EventArgs e)
        {
            if ((sender as Button)?.CommandParameter is not string valueText)
                return;

            if (!int.TryParse(valueText, out var value))
                return;

            var question = _questions[_currentIndex];

            _answers[question.Id] = value;

            ResetAnswerButtonStyles();
            HighlightAnswer(value);

            MessageLabel.IsVisible = false;
        }

        private async void OnNextClicked(object sender, EventArgs e)
        {
            var question = _questions[_currentIndex];

            if (!_answers.ContainsKey(question.Id))
            {
                MessageLabel.Text = "Please select an answer before continuing.";
                MessageLabel.IsVisible = true;
                return;
            }

            if (_currentIndex < _questions.Count - 1)
            {
                _currentIndex++;
                ShowQuestion();
                return;
            }

            var result = BuildResult();

            await Navigation.PushAsync(new CareerTestResultPage(_user, _apiService, result));
        }

        private void OnPreviousClicked(object sender, EventArgs e)
        {
            if (_currentIndex == 0)
                return;

            _currentIndex--;
            ShowQuestion();
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            var confirm = await DisplayAlert(
                "Leave test?",
                "Your answers are not saved. If you leave now, you will need to start the test again.",
                "Leave",
                "Stay");

            if (!confirm)
                return;

            await Navigation.PopAsync();
        }

        private CareerTestResult BuildResult()
        {
            var groupedScores = _questions
                .GroupBy(q => q.Area)
                .Select(group =>
                {
                    var total = group.Sum(q => _answers.TryGetValue(q.Id, out var value) ? value : 0);
                    var max = group.Count() * 5;
                    var percent = max == 0 ? 0 : (int)Math.Round((double)total / max * 100);

                    return new CareerAreaScore
                    {
                        Area = group.Key,
                        Score = percent,
                        Description = GetAreaDescription(group.Key)
                    };
                })
                .OrderByDescending(x => x.Score)
                .ToList();

            var topAreas = groupedScores.Take(3).ToList();

            return new CareerTestResult
            {
                ProfileTitle = BuildProfileTitle(topAreas),
                Summary = BuildSummary(topAreas),
                GeneratedAt = DateTime.Now,
                AreaScores = groupedScores,
                RecommendedRoles = BuildRecommendedRoles(topAreas),
                NextSteps = BuildNextSteps(topAreas)
            };
        }

        private static string BuildProfileTitle(List<CareerAreaScore> topAreas)
        {
            var first = topAreas.ElementAtOrDefault(0)?.Area ?? "";
            var second = topAreas.ElementAtOrDefault(1)?.Area ?? "";

            if (first.Contains("Technical") && second.Contains("Analytical"))
                return "Technical and Analytical Problem Solver";

            if (first.Contains("Practical") && second.Contains("Technical"))
                return "Structured Technical Builder";

            if (first.Contains("Creative") && second.Contains("Social"))
                return "Creative Communicator";

            if (first.Contains("Business") && second.Contains("Social"))
                return "People-Oriented Organizer";

            if (first.Contains("Analytical") && second.Contains("Business"))
                return "Business Data Thinker";

            if (first.Contains("Practical"))
                return "Organized and Practical Planner";

            return "Balanced Career Explorer";
        }

        private static string BuildSummary(List<CareerAreaScore> topAreas)
        {
            var names = topAreas.Select(x => x.Area).ToList();

            return
                $"Your answers suggest that your strongest areas are {string.Join(", ", names)}. " +
                "This result can help you explore career directions that match your interests, preferred work style and strengths. " +
                "The result is not a psychological diagnosis, but a practical orientation tool for career exploration.";
        }

        private static List<string> BuildRecommendedRoles(List<CareerAreaScore> topAreas)
        {
            var areas = topAreas.Select(x => x.Area).ToList();
            var roles = new List<string>();

            if (areas.Any(x => x.Contains("Technical")))
                roles.AddRange(new[] { "Junior Software Developer", "QA Tester", "Technical Support Specialist" });

            if (areas.Any(x => x.Contains("Analytical")))
                roles.AddRange(new[] { "Data Analyst", "Business Analyst", "Reporting Specialist" });

            if (areas.Any(x => x.Contains("Creative")))
                roles.AddRange(new[] { "UX/UI Designer", "Content Creator", "Digital Marketing Assistant" });

            if (areas.Any(x => x.Contains("Social")))
                roles.AddRange(new[] { "HR Assistant", "Recruiter", "Customer Success Specialist" });

            if (areas.Any(x => x.Contains("Business")))
                roles.AddRange(new[] { "Project Coordinator", "Sales Assistant", "Product Assistant" });

            if (areas.Any(x => x.Contains("Practical")))
                roles.AddRange(new[] { "Operations Assistant", "Administrative Specialist", "Documentation Specialist" });

            return roles.Distinct().Take(6).ToList();
        }

        private static List<string> BuildNextSteps(List<CareerAreaScore> topAreas)
        {
            var steps = new List<string>
            {
                "Review the recommended roles and choose 2-3 that seem interesting.",
                "Search for entry-level job descriptions for those roles and compare the required skills.",
                "Create or update a CV version focused on the most relevant direction."
            };

            if (topAreas.Any(x => x.Area.Contains("Technical")))
                steps.Add("Build a small portfolio project to demonstrate practical technical skills.");

            if (topAreas.Any(x => x.Area.Contains("Analytical")))
                steps.Add("Practice working with spreadsheets, SQL basics or simple data analysis tasks.");

            if (topAreas.Any(x => x.Area.Contains("Creative")))
                steps.Add("Create a small portfolio with design, writing or presentation examples.");

            if (topAreas.Any(x => x.Area.Contains("Social")))
                steps.Add("Prepare examples that show communication, teamwork and helping others.");

            return steps.Take(6).ToList();
        }

        private static string GetAreaDescription(string area)
        {
            if (area.Contains("Technical"))
                return "Interest in systems, tools, technology and structured problem solving.";

            if (area.Contains("Analytical"))
                return "Preference for facts, logic, data and careful decision making.";

            if (area.Contains("Creative"))
                return "Interest in ideas, design, expression and flexible tasks.";

            if (area.Contains("Social"))
                return "Motivation to communicate, support and help other people.";

            if (area.Contains("Business"))
                return "Interest in planning, goals, organization and leadership.";

            if (area.Contains("Practical"))
                return "Preference for structure, clear tasks and reliable execution.";

            return string.Empty;
        }

        private void ResetAnswerButtonStyles()
        {
            var buttons = new[]
            {
                Answer1Button,
                Answer2Button,
                Answer3Button,
                Answer4Button,
                Answer5Button
            };

            foreach (var button in buttons)
            {
                button.BackgroundColor = Color.FromArgb("#F8FAFC");
                button.TextColor = Color.FromArgb("#0F172A");
                button.BorderColor = Color.FromArgb("#CBD5E1");
            }
        }

        private void HighlightAnswer(int value)
        {
            var button = value switch
            {
                1 => Answer1Button,
                2 => Answer2Button,
                3 => Answer3Button,
                4 => Answer4Button,
                5 => Answer5Button,
                _ => null
            };

            if (button == null)
                return;

            button.BackgroundColor = Color.FromArgb("#1D4ED8");
            button.TextColor = Colors.White;
            button.BorderColor = Color.FromArgb("#1D4ED8");
        }
    }
}