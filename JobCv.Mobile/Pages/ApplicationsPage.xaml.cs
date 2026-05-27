using JobCv.Mobile.Models;
using JobCv.Mobile.Services;
using Microsoft.Maui.ApplicationModel;

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

        private T GetControl<T>(string name) where T : Element
        {
            var control = this.FindByName<T>(name);

            if (control == null)
                throw new Exception($"Control '{name}' was not found in ApplicationsPage.xaml.");

            return control;
        }

        private Border GetMainMenu()
        {
            return GetControl<Border>("MainMenu");
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

        private void OnMenuClicked(object sender, EventArgs e)
        {
            var mainMenu = GetMainMenu();
            mainMenu.IsVisible = !mainMenu.IsVisible;
        }

        private async void OnDashboardClicked(object sender, EventArgs e)
        {
            GetMainMenu().IsVisible = false;
            await Navigation.PushAsync(new CvsPage(_user, _apiService));
        }

        private async void OnMyCvsClicked(object sender, EventArgs e)
        {
            GetMainMenu().IsVisible = false;
            await Navigation.PushAsync(new MyCvsPage(_user, _apiService));
        }

        private void OnMyApplicationsClicked(object sender, EventArgs e)
        {
            GetMainMenu().IsVisible = false;
            // Already on Applications.
        }

        private async void OnFindJobsClicked(object sender, EventArgs e)
        {
            GetMainMenu().IsVisible = false;
            await Navigation.PushAsync(new JobsPage(_user, _apiService));
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
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
        private Border GetProfileMenu()
        {
            return GetControl<Border>("ProfileMenu");
        }

        private void OnProfileClicked(object sender, EventArgs e)
        {
            var profileMenu = GetProfileMenu();
            var mainMenu = GetMainMenu();

            profileMenu.IsVisible = !profileMenu.IsVisible;

            if (mainMenu.IsVisible)
                mainMenu.IsVisible = false;
        }

        private async void OnViewProfileClicked(object sender, EventArgs e)
        {
            GetProfileMenu().IsVisible = false;

            await Navigation.PushAsync(new ProfilePage(_user, _apiService));
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            GetProfileMenu().IsVisible = false;

            var confirm = await DisplayAlert(
                "Logout",
                "Are you sure you want to log out?",
                "Yes",
                "No");

            if (!confirm)
                return;

            Application.Current!.Windows[0].Page = new NavigationPage(new MainPage());
        }
        private async void OnApplicationDetailsClicked(object sender, EventArgs e)
        {
            if (sender is not Button button || button.CommandParameter is not JobApplicationDto application)
                return;

            await Navigation.PushAsync(new ApplicationDetailsPage(_user, _apiService, application));
        }
        private async void OnViewApplicationDetailsClicked(object sender, EventArgs e)
        {
            if (sender is not Button button || button.CommandParameter is not JobApplicationDto application)
                return;

            await Navigation.PushAsync(new ApplicationDetailsPage(_user, _apiService, application));
        }
    }
}