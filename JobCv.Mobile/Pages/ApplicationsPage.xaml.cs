using JobCv.Mobile.Models;
using JobCv.Mobile.Services;

namespace JobCv.Mobile.Pages
{
    public partial class ApplicationsPage : ContentPage
    {
        private readonly UserDto _user;
        private readonly ApiService _apiService;

        public ApplicationsPage(UserDto user, ApiService apiService)
        {
            InitializeComponent();

            _user = user;
            _apiService = apiService;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadApplicationsAsync();
        }

        private async Task LoadApplicationsAsync()
        {
            MessageLabel.TextColor = Colors.Gray;
            MessageLabel.Text = "Loading applications...";

            try
            {
                var applications = await _apiService.GetUserApplicationsAsync(_user.Id);

                ApplicationsCollectionView.ItemsSource = applications;

                MessageLabel.Text = applications.Count == 0
                    ? "No applications saved yet. Go to Find jobs and track your first application."
                    : $"{applications.Count} application(s) saved.";
            }
            catch (Exception ex)
            {
                MessageLabel.TextColor = Colors.Red;
                MessageLabel.Text = ex.Message;
            }
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnFindJobsClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new JobsPage(_user, _apiService));
        }

        private async void OnOpenJobClicked(object sender, EventArgs e)
        {
            if ((sender as Button)?.CommandParameter is not JobApplicationDto application)
                return;

            if (string.IsNullOrWhiteSpace(application.JobUrl))
            {
                await DisplayAlert("Missing link", "This application does not have a job link.", "OK");
                return;
            }

            try
            {
                await Launcher.OpenAsync(application.JobUrl);
            }
            catch
            {
                await DisplayAlert("Error", "The job link could not be opened.", "OK");
            }
        }

        private async void OnDeleteApplicationClicked(object sender, EventArgs e)
        {
            if ((sender as Button)?.CommandParameter is not JobApplicationDto application)
                return;

            var confirm = await DisplayAlert(
                "Delete application",
                $"Are you sure you want to delete this application?\n\n{application.JobTitle}\n{application.Company}",
                "Yes",
                "No");

            if (!confirm)
                return;

            try
            {
                var deleted = await _apiService.DeleteApplicationAsync(application.Id);

                if (!deleted)
                {
                    await DisplayAlert("Error", "The application could not be deleted.", "OK");
                    return;
                }

                await LoadApplicationsAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }
}