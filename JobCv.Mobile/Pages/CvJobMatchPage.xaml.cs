using JobCv.Mobile.Models;
using JobCv.Mobile.Services;

namespace JobCv.Mobile.Pages
{
    public partial class CvJobMatchPage : ContentPage
    {
        private readonly UserDto _user;
        private readonly ApiService _apiService;
        private readonly JobSearchResultDto _job;
        private readonly List<CvSelectionOption> _cvOptions = new();

        public CvJobMatchPage(UserDto user, ApiService apiService, JobSearchResultDto job)
        {
            InitializeComponent();

            _user = user;
            _apiService = apiService;
            _job = job;

            JobTitleLabel.Text = string.IsNullOrWhiteSpace(_job.Title) ? "Selected job" : _job.Title;
            CompanyLabel.Text = string.IsNullOrWhiteSpace(_job.Company) ? "Unknown company" : _job.Company;
            LocationLabel.Text = string.IsNullOrWhiteSpace(_job.Location) ? "Location not specified" : _job.Location;
            DescriptionLabel.Text = string.IsNullOrWhiteSpace(_job.Description) ? "No job description available." : _job.Description;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadCvOptionsAsync();
        }

        private async Task LoadCvOptionsAsync()
        {
            try
            {
                _cvOptions.Clear();

                var createdCvs = await _apiService.GetUserCvsAsync(_user.Id);
                var uploadedCvs = await _apiService.GetUploadedCvFilesAsync(_user.Id);

                foreach (var cv in createdCvs)
                {
                    _cvOptions.Add(new CvSelectionOption
                    {
                        SourceType = "Created",
                        Title = string.IsNullOrWhiteSpace(cv.Title) ? "Created CV" : cv.Title,
                        Subtitle = $"Created CV • {cv.Language}",
                        CvText = BuildReadableCvText(cv)
                    });
                }

                foreach (var uploaded in uploadedCvs)
                {
                    var fileName = string.IsNullOrWhiteSpace(uploaded.OriginalFileName)
                        ? "Uploaded CV"
                        : uploaded.OriginalFileName;

                    _cvOptions.Add(new CvSelectionOption
                    {
                        SourceType = "Uploaded",
                        Title = fileName,
                        Subtitle = "Uploaded CV file. Full text extraction can be added later.",
                        CvText =
                            $"The user selected an uploaded CV file named {fileName}. " +
                            "The full file content is not extracted yet, so estimate the match mainly from the job description and give cautious advice."
                    });
                }

                _cvOptions.Add(new CvSelectionOption
                {
                    SourceType = "Manual",
                    Title = "Manual CV text",
                    Subtitle = "Write or paste CV text manually.",
                    CvText = string.Empty
                });

                CvPicker.ItemsSource = null;
                CvPicker.ItemsSource = _cvOptions;

                if (_cvOptions.Count > 0)
                    CvPicker.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageLabel.TextColor = Colors.Red;
                MessageLabel.Text = $"CV list could not be loaded: {ex.Message}";
            }
        }

        private static string BuildReadableCvText(CvDto cv)
        {
            var parts = new List<string>();

            Add(parts, "Title", cv.Title);
            Add(parts, "Language", cv.Language);
            Add(parts, "Summary", cv.Summary);
            Add(parts, "Template", cv.TemplateName);

            if (cv.Skills != null && cv.Skills.Count > 0)
                parts.Add("Skills: " + string.Join(", ", cv.Skills.Select(x => x.Name)));

            if (cv.Experiences != null && cv.Experiences.Count > 0)
            {
                foreach (var exp in cv.Experiences)
                    parts.Add($"Experience: {exp.JobTitle} at {exp.Company}. {exp.Description}");
            }

            if (cv.Projects != null && cv.Projects.Count > 0)
            {
                foreach (var project in cv.Projects)
                    parts.Add($"Project: {project.Title}. {project.Description}. {project.Technologies}");
            }

            if (cv.Educations != null && cv.Educations.Count > 0)
            {
                foreach (var education in cv.Educations)
                    parts.Add($"Education: {education.Degree} at {education.Institution}. {education.Description}");
            }

            if (cv.Certifications != null && cv.Certifications.Count > 0)
            {
                foreach (var certification in cv.Certifications)
                    parts.Add($"Certification: {certification.Name} from {certification.Issuer}");
            }

            if (cv.Languages != null && cv.Languages.Count > 0)
                parts.Add("Languages: " + string.Join(", ", cv.Languages.Select(x => $"{x.Name} {x.Level}")));

            return parts.Count == 0
                ? "The selected CV exists, but it does not contain enough readable information."
                : string.Join("\n", parts);
        }

        private static void Add(List<string> parts, string label, string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
                parts.Add($"{label}: {value}");
        }

        private void OnCvSelected(object sender, EventArgs e)
        {
            if (CvPicker.SelectedItem is not CvSelectionOption selected)
                return;

            SelectedCvInfoBorder.IsVisible = true;
            SelectedCvTitleLabel.Text = selected.Title;
            SelectedCvSubtitleLabel.Text = selected.Subtitle;
            ManualCvTextLayout.IsVisible = selected.SourceType == "Manual";
        }

        private void OnToggleManualTextClicked(object sender, EventArgs e)
        {
            ManualCvTextLayout.IsVisible = !ManualCvTextLayout.IsVisible;

            if (ManualCvTextLayout.IsVisible)
            {
                SelectedCvInfoBorder.IsVisible = false;
                CvPicker.SelectedIndex = -1;
            }
        }

        private async void OnGenerateClicked(object sender, EventArgs e)
        {
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;
            ResultsLayout.IsVisible = false;
            MessageLabel.TextColor = Colors.Gray;
            MessageLabel.Text = UiTranslationService.TranslateText("Generating match score...");

            try
            {
                var selectedCv = CvPicker.SelectedItem as CvSelectionOption;

                var cvText = ManualCvTextLayout.IsVisible
                    ? CvTextEditor.Text ?? string.Empty
                    : selectedCv?.CvText ?? string.Empty;

                if (string.IsNullOrWhiteSpace(cvText))
                {
                    MessageLabel.TextColor = Colors.Red;
                    MessageLabel.Text = UiTranslationService.TranslateText("Choose a CV or paste CV text first.");
                    return;
                }

                var request = new CvJobMatchRequest
                {
                    JobTitle = _job.Title ?? string.Empty,
                    Company = _job.Company ?? string.Empty,
                    Location = _job.Location ?? string.Empty,
                    JobDescription = _job.Description ?? string.Empty,
                    CurrentCvText = cvText,
                    Language = LanguageService.CurrentLanguage
                };

                var result = await _apiService.GenerateCvJobMatchAsync(request);

                if (result == null)
                {
                    MessageLabel.TextColor = Colors.Red;
                    MessageLabel.Text = UiTranslationService.TranslateText("The match score could not be generated.");
                    return;
                }

                ScoreLabel.Text = $"{result.MatchScore}%";
                RecommendationLabel.Text = UiTranslationService.TranslateText(result.Recommendation);
                SummaryLabel.Text = UiTranslationService.TranslateText(result.Summary);
                StrengthsCollectionView.ItemsSource = result.Strengths.Select(UiTranslationService.TranslateText).ToList();
                MissingCollectionView.ItemsSource = result.MissingSkills.Select(UiTranslationService.TranslateText).ToList();
                ImprovementsCollectionView.ItemsSource = result.Improvements.Select(UiTranslationService.TranslateText).ToList();

                ResultsLayout.IsVisible = true;
                UiTranslationService.ApplyToPage(this);
                MessageLabel.TextColor = Colors.Green;
                MessageLabel.Text = result.IsMock
                    ? UiTranslationService.TranslateText("Match score generated with explainable demo logic.")
                    : UiTranslationService.TranslateText("Match score generated successfully.");
            }
            catch (Exception ex)
            {
                MessageLabel.TextColor = Colors.Red;
                MessageLabel.Text = ex.Message;
            }
            finally
            {
                LoadingIndicator.IsVisible = false;
                LoadingIndicator.IsRunning = false;
            }
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
