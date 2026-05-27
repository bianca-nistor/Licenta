using JobCv.Mobile.Models;
using JobCv.Mobile.Services;


namespace JobCv.Mobile.Pages
{
    public partial class InterviewPrepPage : ContentPage
    {
        private readonly UserDto _user;
        private readonly ApiService _apiService;
        private readonly JobSearchResultDto _job;

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
            MessageLabel.Text = "Generating interview preparation...";

            ResultsLayout.IsVisible = false;

            try
            {
                var request = new InterviewPrepRequest
                {
                    JobTitle = _job.Title ?? string.Empty,
                    Company = _job.Company ?? string.Empty,
                    Location = _job.Location ?? string.Empty,
                    Description = _job.Description ?? string.Empty
                };

                var result = await _apiService.GenerateInterviewPrepAsync(request);

                if (result == null)
                {
                    MessageLabel.TextColor = Colors.Red;
                    MessageLabel.Text = "Interview preparation could not be generated.";
                    return;
                }

                SummaryLabel.Text = result.Summary;

                SkillsCollectionView.ItemsSource = result.KeySkills;
                QuestionsCollectionView.ItemsSource = result.Questions;
                TipsCollectionView.ItemsSource = result.BeforeInterviewTips;

                ResultsLayout.IsVisible = true;
                if (result.IsMock)
                {
                    AiSourceBanner.BackgroundColor = Color.FromArgb("#FEF3C7");
                    AiSourceBanner.Stroke = Color.FromArgb("#FDE68A");
                    AiSourceLabel.TextColor = Color.FromArgb("#92400E");
                    AiSourceLabel.Text = "Demo AI result: this is a mock response. Later it can be connected to OpenAI or a local LLM.";
                }
                else
                {
                    AiSourceBanner.BackgroundColor = Color.FromArgb("#ECFDF5");
                    AiSourceBanner.Stroke = Color.FromArgb("#BBF7D0");
                    AiSourceLabel.TextColor = Color.FromArgb("#166534");
                    AiSourceLabel.Text = "Generated with local AI using Ollama. Your job data was processed locally on this laptop.";
                }

                MessageLabel.TextColor = Colors.Green;
                MessageLabel.Text = "Interview preparation generated successfully.";
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