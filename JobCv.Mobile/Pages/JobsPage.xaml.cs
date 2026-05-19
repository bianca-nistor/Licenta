using JobCv.Mobile.Models;
using JobCv.Mobile.Services;

namespace JobCv.Mobile.Pages
{
    public partial class JobsPage : ContentPage
    {
        private readonly UserDto _user;
        private readonly ApiService _apiService;

        public JobsPage(UserDto user, ApiService apiService)
        {
            InitializeComponent();

            _user = user;
            _apiService = apiService;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (JobsCollectionView.ItemsSource == null)
            {
                QueryEntry.Text = "developer";
                LocationEntry.Text = "remote";

                await SearchJobsAsync();
            }
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnSearchClicked(object sender, EventArgs e)
        {
            await SearchJobsAsync();
        }

        private async Task SearchJobsAsync()
        {
            MessageLabel.Text = "Searching jobs...";

            try
            {
                var query = QueryEntry.Text?.Trim() ?? "";
                var location = LocationEntry.Text?.Trim() ?? "";

                var jobs = await _apiService.SearchJobsAsync(query, location);

                JobsCollectionView.ItemsSource = jobs;

                MessageLabel.Text = jobs.Count == 0
                    ? "No jobs found. Try another keyword or location."
                    : $"{jobs.Count} job(s) found.";
            }
            catch (Exception ex)
            {
                MessageLabel.TextColor = Colors.Red;
                MessageLabel.Text = ex.Message;
            }
        }

        private async void OnApplyClicked(object sender, EventArgs e)
        {
            if ((sender as Button)?.CommandParameter is not JobSearchResultDto job)
                return;

            if (string.IsNullOrWhiteSpace(job.ApplyUrl))
            {
                await DisplayAlert("Missing link", "This job does not have an application link.", "OK");
                return;
            }

            try
            {
                await Launcher.OpenAsync(job.ApplyUrl);
            }
            catch
            {
                await DisplayAlert("Error", "The job link could not be opened.", "OK");
            }
        }

        private async void OnCreateCvForJobClicked(object sender, EventArgs e)
        {
            if ((sender as Button)?.CommandParameter is not JobSearchResultDto job)
                return;

            var confirm = await DisplayAlert(
                "Create CV",
                $"Create a CV for this job?\n\n{job.Title}\n{job.Company}",
                "Yes",
                "No");

            if (!confirm)
                return;

            await Navigation.PushAsync(new CreateCvPage(_user, _apiService));
        }

        private async void OnTrackApplicationClicked(object sender, EventArgs e)
        {
            if ((sender as Button)?.CommandParameter is not JobSearchResultDto job)
                return;

            await Navigation.PushAsync(new ApplicationFormPage(_user, _apiService, job));
        }
    }
}