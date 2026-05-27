using JobCv.Mobile.Models;
using JobCv.Mobile.Services;

namespace JobCv.Mobile.Pages
{
    public partial class CvsPage : ContentPage
    {
        private readonly UserDto _user;
        private readonly ApiService _apiService;

        public CvsPage(UserDto user, ApiService apiService)
        {
            InitializeComponent();

            _user = user;
            _apiService = apiService;

            var displayName = string.IsNullOrWhiteSpace(_user.FullName)
                ? _user.Email
                : _user.FullName;

            SetLabelText(
                "WelcomeLabel",
                $"Welcome, {displayName}. Manage your CVs, jobs and applications in one place.");
        }

        private void OnPageSizeChanged(object? sender, EventArgs e)
        {
            UpdateResponsiveHeader(Width);
        }

        private void UpdateResponsiveHeader(double width)
        {
            if (PageTitleLabel == null)
                return;

            if (width <= 700)
            {
                PageTitleLabel.Text = "Dashboard";
                PageTitleLabel.FontSize = 34;
            }
            else
            {
                PageTitleLabel.Text = "Career Dashboard";
                PageTitleLabel.FontSize = 34;
            }
        }
        private T GetControl<T>(string name) where T : Element
        {
            var control = this.FindByName<T>(name);

            if (control == null)
                throw new Exception($"Control '{name}' was not found in CvsPage.xaml.");

            return control;
        }

        private Border GetMainMenu()
        {
            return GetControl<Border>("MainMenu");
        }

        private Border GetProfileMenu()
        {
            return GetControl<Border>("ProfileMenu");
        }

        private void SetLabelText(string labelName, string text)
        {
            var label = this.FindByName<Label>(labelName);

            if (label != null)
                label.Text = text;
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await LoadDashboardAsync();
        }

        private async Task LoadDashboardAsync()
        {
            try
            {
                var createdCvs = await _apiService.GetUserCvsAsync(_user.Id);
                var uploadedCvs = await _apiService.GetUploadedCvFilesAsync(_user.Id);
                var applications = await _apiService.GetUserApplicationsAsync(_user.Id);

                SetLabelText("CreatedCvCountLabel", createdCvs.Count.ToString());
                SetLabelText("UploadedCvCountLabel", uploadedCvs.Count.ToString());
                SetLabelText("ApplicationsCountLabel", applications.Count.ToString());

                var interviewsCount = applications.Count(x =>
                    x.Status == "Interview Scheduled" ||
                    x.Status == "Interview Done");

                SetLabelText("InterviewsCountLabel", interviewsCount.ToString());

                var recentApplications = applications
                    .OrderByDescending(x => x.CreatedAt)
                    .Take(3)
                    .ToList();

                var recentApplicationsCollection =
                    this.FindByName<CollectionView>("RecentApplicationsCollectionView");

                var emptyLabel =
                    this.FindByName<Label>("RecentApplicationsEmptyLabel");

                if (recentApplicationsCollection != null)
                    recentApplicationsCollection.ItemsSource = recentApplications;

                if (emptyLabel != null)
                    emptyLabel.IsVisible = recentApplications.Count == 0;
            }
            catch (Exception ex)
            {
                var emptyLabel =
                    this.FindByName<Label>("RecentApplicationsEmptyLabel");

                if (emptyLabel != null)
                {
                    emptyLabel.IsVisible = true;
                    emptyLabel.Text = $"Dashboard could not be loaded: {ex.Message}";
                }

                SetLabelText("CreatedCvCountLabel", "0");
                SetLabelText("UploadedCvCountLabel", "0");
                SetLabelText("ApplicationsCountLabel", "0");
                SetLabelText("InterviewsCountLabel", "0");
            }
        }

        private void OnMenuClicked(object sender, EventArgs e)
        {
            var mainMenu = GetMainMenu();
            var profileMenu = GetProfileMenu();

            mainMenu.IsVisible = !mainMenu.IsVisible;

            if (profileMenu.IsVisible)
                profileMenu.IsVisible = false;
        }

        private void OnDashboardClicked(object sender, EventArgs e)
        {
            GetMainMenu().IsVisible = false;
        }

        private async void OnMyCvsClicked(object sender, EventArgs e)
        {
            GetMainMenu().IsVisible = false;
            await Navigation.PushAsync(new MyCvsPage(_user, _apiService));
        }

        private async void OnFindJobsClicked(object sender, EventArgs e)
        {
            GetMainMenu().IsVisible = false;
            await Navigation.PushAsync(new JobsPage(_user, _apiService));
        }

        private async void OnMyApplicationsClicked(object sender, EventArgs e)
        {
            GetMainMenu().IsVisible = false;
            await Navigation.PushAsync(new ApplicationsPage(_user, _apiService));
        }

        private async void OnCreateCvClicked(object sender, EventArgs e)
        {
            GetMainMenu().IsVisible = false;
            await Navigation.PushAsync(new CreateCvPage(_user, _apiService));
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
    }
}