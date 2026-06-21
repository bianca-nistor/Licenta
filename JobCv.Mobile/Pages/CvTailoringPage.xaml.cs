using System.Text.Json;
using JobCv.Mobile.Models;
using JobCv.Mobile.Services;

namespace JobCv.Mobile.Pages
{
    public partial class CvTailoringPage : ContentPage
    {
        private readonly UserDto _user;
        private readonly ApiService _apiService;
        private readonly JobSearchResultDto _job;

        private readonly List<CvSelectionOption> _cvOptions = new();

        public CvTailoringPage(
            UserDto user,
            ApiService apiService,
            JobSearchResultDto job)
        {
            InitializeComponent();

            _user = user;
            _apiService = apiService;
            _job = job;

            JobTitleLabel.Text = string.IsNullOrWhiteSpace(_job.Title)
                ? "Selected job"
                : _job.Title;

            CompanyLabel.Text = string.IsNullOrWhiteSpace(_job.Company)
                ? "Unknown company"
                : _job.Company;

            LocationLabel.Text = string.IsNullOrWhiteSpace(_job.Location)
                ? "Location not specified"
                : _job.Location;

            DescriptionLabel.Text = string.IsNullOrWhiteSpace(_job.Description)
                ? "No job description available."
                : _job.Description;
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
                    var title = GetPropertyValue(cv, "Title")
                        ?? GetPropertyValue(cv, "Name")
                        ?? "Created CV";

                    var language = GetPropertyValue(cv, "Language") ?? "Created inside the app";

                    _cvOptions.Add(new CvSelectionOption
                    {
                        SourceType = "Created",
                        Title = title,
                        Subtitle = $"Created CV • {language}",
                        CvText = BuildReadableCvText(cv)
                    });
                }

                foreach (var uploaded in uploadedCvs)
                {
                    var fileName = GetPropertyValue(uploaded, "FileName")
                        ?? GetPropertyValue(uploaded, "OriginalFileName")
                        ?? GetPropertyValue(uploaded, "Name")
                        ?? "Uploaded CV";

                    _cvOptions.Add(new CvSelectionOption
                    {
                        SourceType = "Uploaded",
                        Title = fileName,
                        Subtitle = "Uploaded CV file. Full file text extraction can be added later.",
                        CvText =
                            $"The user selected an uploaded CV file named {fileName}. " +
                            "The full file content is not extracted yet, so generate general tailoring advice based on the job description."
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
        private static string BuildReadableCvText(object cv)
        {
            var ignoredProperties = new HashSet<string>
    {
        "Id",
        "UserId",
        "CreatedAt",
        "UpdatedAt"
    };

            var parts = new List<string>();

            foreach (var property in cv.GetType().GetProperties())
            {
                if (ignoredProperties.Contains(property.Name))
                    continue;

                var value = property.GetValue(cv);

                if (value == null)
                    continue;

                var text = value.ToString();

                if (string.IsNullOrWhiteSpace(text))
                    continue;

                parts.Add($"{property.Name}: {text}");
            }

            if (parts.Count == 0)
                return "The selected CV exists, but it does not contain enough readable information.";

            return string.Join("\n", parts);
        }

        private static string? GetPropertyValue(object obj, string propertyName)
        {
            var property = obj.GetType().GetProperty(propertyName);

            if (property == null)
                return null;

            var value = property.GetValue(obj);

            return value?.ToString();
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

            MessageLabel.TextColor = Colors.Gray;
            MessageLabel.Text = UiTranslationService.TranslateText("Generating CV suggestions...");

            ResultsLayout.IsVisible = false;

            try
            {
                var selectedCv = CvPicker.SelectedItem as CvSelectionOption;

                var cvText = ManualCvTextLayout.IsVisible
                    ? CvTextEditor.Text ?? string.Empty
                    : selectedCv?.CvText ?? string.Empty;

                if (string.IsNullOrWhiteSpace(cvText))
                {
                    cvText = "No CV text was provided. Generate general tailoring advice based only on the selected job.";
                }

                var request = new CvTailoringRequest
                {
                    JobTitle = _job.Title ?? string.Empty,
                    Company = _job.Company ?? string.Empty,
                    Location = _job.Location ?? string.Empty,
                    JobDescription = _job.Description ?? string.Empty,
                    CurrentCvText = cvText,
                    Language = LanguageService.CurrentLanguage
                };

                var result = await _apiService.GenerateCvTailoringAsync(request);

                if (result == null)
                {
                    MessageLabel.TextColor = Colors.Red;
                    MessageLabel.Text = UiTranslationService.TranslateText("CV suggestions could not be generated.");
                    return;
                }

                TailoredSummaryLabel.Text = result.TailoredProfileSummary;

                KeywordsCollectionView.ItemsSource = result.ImportantKeywords;
                SkillsCollectionView.ItemsSource = result.SkillsToHighlight;
                ExperienceCollectionView.ItemsSource = result.ExperienceToEmphasize;
                SuggestionsCollectionView.ItemsSource = result.Suggestions;

                ResultsLayout.IsVisible = true;

                if (result.IsMock)
                {
                    AiSourceBanner.BackgroundColor = Color.FromArgb("#FEF3C7");
                    AiSourceBanner.Stroke = Color.FromArgb("#FDE68A");
                    AiSourceLabel.TextColor = Color.FromArgb("#92400E");
                    AiSourceLabel.Text = UiTranslationService.TranslateText("Demo AI result: this is a mock response. Later it can be connected to OpenAI or a local LLM.");
                }
                else
                {
                    AiSourceBanner.BackgroundColor = Color.FromArgb("#ECFDF5");
                    AiSourceBanner.Stroke = Color.FromArgb("#BBF7D0");
                    AiSourceLabel.TextColor = Color.FromArgb("#166534");
                    AiSourceLabel.Text = UiTranslationService.TranslateText("Generated with AI.");
                }

                if (string.IsNullOrWhiteSpace(result.CvQualityWarning))
                {
                    CvQualityWarningBorder.IsVisible = false;
                    CvQualityWarningLabel.Text = string.Empty;
                }
                else
                {
                    CvQualityWarningBorder.IsVisible = true;
                    CvQualityWarningLabel.Text = UiTranslationService.TranslateText(result.CvQualityWarning);
                }

                MessageLabel.TextColor = Colors.Green;
                MessageLabel.Text = UiTranslationService.TranslateText("CV suggestions generated successfully.");
                UiTranslationService.ApplyToPage(this);
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