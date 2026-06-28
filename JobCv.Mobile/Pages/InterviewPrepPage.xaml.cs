using JobCv.Mobile.Models;
using JobCv.Mobile.Services;


namespace JobCv.Mobile.Pages
{
    public partial class InterviewPrepPage : ContentPage
    {
        private readonly UserDto _user;
        private readonly ApiService _apiService;
        private readonly JobSearchResultDto _job;

        private static string Txt(string en, string ro) => LanguageService.IsRomanian ? ro : en;

        public InterviewPrepPage(
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

        private async void OnGenerateClicked(object sender, EventArgs e)
        {
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            MessageLabel.TextColor = Colors.Gray;
            MessageLabel.Text = UiTranslationService.TranslateText("Generating interview preparation...");

            ResultsLayout.IsVisible = false;

            try
            {
                var request = new InterviewPrepRequest
                {
                    JobTitle = _job.Title ?? string.Empty,
                    Company = _job.Company ?? string.Empty,
                    Location = _job.Location ?? string.Empty,
                    Description = _job.Description ?? string.Empty,
                    Language = LanguageService.CurrentLanguage
                };

                var result = await _apiService.GenerateInterviewPrepAsync(request);

                if (result == null)
                {
                    MessageLabel.TextColor = Colors.Red;
                    MessageLabel.Text = UiTranslationService.TranslateText("Interview preparation could not be generated.");
                    return;
                }

                SummaryLabel.Text = result.Summary;

                SkillsCollectionView.ItemsSource = result.KeySkills;
                QuestionsCollectionView.ItemsSource = result.Questions;
                TipsCollectionView.ItemsSource = result.BeforeInterviewTips;

                ResultsLayout.IsVisible = true;
                AiSourceBanner.BackgroundColor = Color.FromArgb("#ECFDF5");
                AiSourceBanner.Stroke = Color.FromArgb("#BBF7D0");
                AiSourceLabel.TextColor = Color.FromArgb("#166534");
                AiSourceLabel.Text = Txt(
                    "Generated successfully.",
                    "Generat cu succes.");
                MessageLabel.TextColor = Colors.Green;
                MessageLabel.Text = Txt("Interview preparation generated successfully.", "Pregătirea pentru interviu a fost generată cu succes.");
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