using JobCv.Mobile.Models;
using JobCv.Mobile.Services;
using Microsoft.Maui.ApplicationModel;

namespace JobCv.Mobile.Pages
{
    public partial class JobsPage : ContentPage
    {
        private readonly UserDto _user;
        private readonly ApiService _apiService;

        public JobsPage(UserDto user, ApiService apiService)
        {
            InitializeComponent();

            SizeChanged += OnPageSizeChanged;

            _user = user;
            _apiService = apiService;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (JobsCollectionView.ItemsSource == null)
            {
                QueryEntry.Text = "developer";
                LocationEntry.Text = "";

                await SearchJobsAsync();
            }
        }

        private T GetControl<T>(string name) where T : Element
        {
            var control = this.FindByName<T>(name);

            if (control == null)
                throw new Exception($"Control '{name}' was not found in JobsPage.xaml.");

            return control;
        }

        private Border GetMainMenu()
        {
            return GetControl<Border>("MainMenu");
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

        private async void OnMyApplicationsClicked(object sender, EventArgs e)
        {
            GetMainMenu().IsVisible = false;
            await Navigation.PushAsync(new ApplicationsPage(_user, _apiService));
        }

        private void OnFindJobsClicked(object sender, EventArgs e)
        {
            GetMainMenu().IsVisible = false;
            // Already on Jobs.
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
            MessageLabel.TextColor = Colors.Gray;
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

        private async void OnViewJobDetailsClicked(object sender, EventArgs e)
        {
            if ((sender as Button)?.CommandParameter is not JobSearchResultDto job)
                return;

            await Navigation.PushAsync(new JobDetailsPage(_user, _apiService, job));
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
        private async void OnPrepareInterviewClicked(object sender, EventArgs e)
        {
            if ((sender as Button)?.CommandParameter is not JobSearchResultDto job)
                return;

            await Navigation.PushAsync(
                new InterviewPrepPage(_user, _apiService, job));
        }
        private async void OnTailorCvClicked(object sender, EventArgs e)
        {
            if ((sender as Button)?.CommandParameter is not JobSearchResultDto job)
                return;

            await Navigation.PushAsync(
                new CvTailoringPage(_user, _apiService, job));
        }
        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            if (PageTitleLabel == null)
                return;

            if (width < 360)
            {
                PageTitleLabel.FontSize = 26;
            }
            else if (width < 430)
            {
                PageTitleLabel.FontSize = 30;
            }
            else
            {
                PageTitleLabel.FontSize = 34;
            }
        }
        private void OnPageSizeChanged(object? sender, EventArgs e)
        {
            UpdateResponsiveHeader(Width);
        }

        private void UpdateResponsiveHeader(double width)
        {
            if (PageTitleLabel == null)
                return;

            if (width <= 360)
            {
                PageTitleLabel.Text = "Dashboard";
                PageTitleLabel.FontSize = 26;
            }
            else if (width <= 430)
            {
                PageTitleLabel.Text = "Dashboard";
                PageTitleLabel.FontSize = 30;
            }
            else if (width <= 600)
            {
                PageTitleLabel.Text = "Career Dashboard";
                PageTitleLabel.FontSize = 30;
            }
            else
            {
                PageTitleLabel.Text = "Career Dashboard";
                PageTitleLabel.FontSize = 34;
            }
        }
    }
}