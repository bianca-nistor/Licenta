using JobCv.Mobile.Models;
using JobCv.Mobile.Services;

namespace JobCv.Mobile.Pages
{
    public partial class CvQualityCheckPage : ContentPage
    {
        private readonly int _cvId;
        private readonly ApiService _apiService;
        private CvDto? _cv;
        private bool _isChecking;

        public CvQualityCheckPage(int cvId, ApiService apiService)
        {
            InitializeComponent();

            _cvId = cvId;
            _apiService = apiService;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await LoadCvAsync();
        }

        private async Task LoadCvAsync()
        {
            try
            {
                _cv = await _apiService.GetCvByIdAsync(_cvId);

                if (_cv == null)
                {
                    MessageLabel.Text = "The selected CV could not be loaded.";
                    CheckButton.IsEnabled = false;
                    return;
                }

                SubtitleLabel.Text = $"Quality analysis for: {_cv.Title}";
            }
            catch (Exception ex)
            {
                MessageLabel.Text = ex.Message;
                CheckButton.IsEnabled = false;
            }
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnCheckCvQualityClicked(object sender, EventArgs e)
        {
            if (_isChecking || _cv == null)
                return;

            _isChecking = true;
            CheckButton.IsEnabled = false;
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;
            MessageLabel.Text = string.Empty;

            try
            {
                var request = BuildRequest(_cv);
                var result = await _apiService.CheckCvQualityAsync(request);

                if (result == null)
                {
                    MessageLabel.Text = "The CV quality check could not be generated.";
                    return;
                }

                ApplyResult(result);
            }
            catch (Exception ex)
            {
                MessageLabel.Text = ex.Message;
            }
            finally
            {
                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;
                CheckButton.IsEnabled = true;
                _isChecking = false;
            }
        }

        private static CvQualityCheckRequest BuildRequest(CvDto cv)
        {
            return new CvQualityCheckRequest
            {
                CvId = cv.Id,
                Title = cv.Title,
                FullName = cv.FullName,
                Email = cv.Email,
                Phone = cv.Phone,
                Location = cv.Location,
                Summary = cv.Summary,
                Skills = cv.Skills.Select(skill => skill.Name).ToList(),
                Educations = cv.Educations
                    .Select(education => $"{education.Degree} {education.Institution} {education.Description}")
                    .ToList(),
                Experiences = cv.Experiences
                    .Select(experience => $"{experience.JobTitle} {experience.Company} {experience.Description}")
                    .ToList(),
                Projects = cv.Projects
                    .Select(project => $"{project.Title} {project.Description} {project.Technologies}")
                    .ToList(),
                Certifications = cv.Certifications
                    .Select(certification => $"{certification.Name} {certification.Issuer}")
                    .ToList(),
                Languages = cv.Languages
                    .Select(language => $"{language.Name} {language.Level}")
                    .ToList()
            };
        }

        private void ApplyResult(CvQualityCheckResponse result)
        {
            ScoreLabel.Text = $"{result.Score}%";
            LevelLabel.Text = string.IsNullOrWhiteSpace(result.CompletenessLevel)
                ? "CV quality"
                : result.CompletenessLevel;
            SummaryLabel.Text = result.Summary;

            StrengthsCollectionView.ItemsSource = result.Strengths;
            IssuesCollectionView.ItemsSource = result.Issues;
            SuggestionsCollectionView.ItemsSource = result.Suggestions;
        }
    }
}
