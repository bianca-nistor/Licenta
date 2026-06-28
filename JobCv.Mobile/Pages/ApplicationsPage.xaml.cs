using JobCv.Mobile.Models;
using JobCv.Mobile.Services;

namespace JobCv.Mobile.Pages
{
    public partial class ApplicationsPage : ContentPage
    {
        private readonly UserDto _user;
        private readonly ApiService _apiService;

        private List<JobApplicationDto> _allApplications = new();
        private string _currentFilter = "All";
        private string _searchText = string.Empty;

        private sealed class ApplicationListItem
        {
            public ApplicationListItem(JobApplicationDto application)
            {
                Application = application;
            }

            public JobApplicationDto Application { get; }
            public int Id => Application.Id;
            public string JobTitle => Application.JobTitle;
            public string Company => Application.Company;
            public string Location => Application.Location;
            public string Status => UiTranslationService.TranslateText(Application.Status);
            public DateTime? AppliedAt => Application.AppliedAt;
            public string Source => Application.Source;
        }

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

        private Border GetProfileMenu()
        {
            return GetControl<Border>("ProfileMenu");
        }

        private async Task LoadApplicationsAsync()
        {
            try
            {
                _allApplications = await _apiService.GetUserApplicationsAsync(_user.Id);

                ApplyCurrentFilter();
            }
            catch (Exception ex)
            {
                await UiTranslationService.DisplayAlertAsync(this, "Error", ex.Message);
            }
        }

        private void OnFilterClicked(object sender, EventArgs e)
        {
            if ((sender as Button)?.CommandParameter is not string selectedFilter)
                return;

            _currentFilter = selectedFilter;
            ApplyCurrentFilter();
        }

        private void ApplyCurrentFilter()
        {
            var filteredApplications = GetFilteredApplications();

            ApplicationsCollectionView.ItemsSource = filteredApplications.Select(application => new ApplicationListItem(application)).ToList();

            EmptyApplicationsState.IsVisible = filteredApplications.Count == 0;
            ApplicationsCollectionView.IsVisible = filteredApplications.Count > 0;

            ApplicationsCountLabel.Text = filteredApplications.Count.ToString();

            FilterSummaryLabel.Text = UiTranslationService.TranslateText(_currentFilter == "All"
                ? $"Showing all applications ({filteredApplications.Count})."
                : $"Showing {_currentFilter.ToLower()} applications ({filteredApplications.Count}).");

            EmptyApplicationsMessageLabel.Text = UiTranslationService.TranslateText(_currentFilter == "All"
                ? "You have not saved or tracked any applications yet."
                : $"You do not have applications with the status {_currentFilter} yet.");

            UpdateFilterButtons();
            UiTranslationService.ApplyToPage(this);
        }


        private void UpdateFilterButtons()
        {
            SetFilterButtonStyle(AllFilterButton, _currentFilter == "All");
            SetFilterButtonStyle(SavedFilterButton, _currentFilter == "Saved");
            SetFilterButtonStyle(AppliedFilterButton, _currentFilter == "Applied");
            SetFilterButtonStyle(InterviewFilterButton, _currentFilter == "Interview");
            SetFilterButtonStyle(OfferFilterButton, _currentFilter == "Offer");
            SetFilterButtonStyle(RejectedFilterButton, _currentFilter == "Rejected");
        }

        private static void SetFilterButtonStyle(Button button, bool isSelected)
        {
            if (isSelected)
            {
                button.BackgroundColor = Color.FromArgb("#1D4ED8");
                button.TextColor = Colors.White;
                return;
            }

            button.BackgroundColor = Color.FromArgb("#EFF6FF");
            button.TextColor = Color.FromArgb("#1D4ED8");
        }

        private void OnMenuClicked(object sender, EventArgs e)
        {
            var mainMenu = GetMainMenu();
            var profileMenu = GetProfileMenu();

            mainMenu.IsVisible = !mainMenu.IsVisible;

            if (profileMenu.IsVisible)
                profileMenu.IsVisible = false;
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
        }

        private async void OnFindJobsClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new JobsPage(_user, _apiService));
        }

        private async void OnViewApplicationDetailsClicked(object sender, EventArgs e)
        {
            var commandParameter = (sender as Button)?.CommandParameter;

            var application = commandParameter switch
            {
                ApplicationListItem item => item.Application,
                JobApplicationDto dto => dto,
                _ => null
            };

            if (application == null)
                return;

            await Navigation.PushAsync(new ApplicationDetailsPage(_user, _apiService, application));
        }

        private async void OnDeleteApplicationClicked(object sender, EventArgs e)
        {
            var commandParameter = (sender as Button)?.CommandParameter;

            var applicationId = commandParameter switch
            {
                int id => id,
                ApplicationListItem item => item.Id,
                JobApplicationDto dto => dto.Id,
                _ => 0
            };

            if (applicationId <= 0)
                return;

            var confirm = await UiTranslationService.DisplayConfirmAsync(
                this,
                "Delete application",
                "Are you sure you want to delete this application?");

            if (!confirm)
                return;

            var deleted = await _apiService.DeleteApplicationAsync(applicationId);

            if (!deleted)
            {
                await UiTranslationService.DisplayAlertAsync(this, "Error", "The application could not be deleted.");
                return;
            }

            await LoadApplicationsAsync();
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

            var confirm = await UiTranslationService.DisplayConfirmAsync(
                this,
                "Logout",
                "Are you sure you want to log out?");

            if (!confirm)
                return;

            Application.Current!.Windows[0].Page = new LocalizedNavigationPage(new MainPage());
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
        private void OnApplicationSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            _searchText = e.NewTextValue ?? string.Empty;
            ApplyCurrentFilter();
        }
        private List<JobApplicationDto> GetFilteredApplications()
        {
            IEnumerable<JobApplicationDto> applications = _allApplications;

            if (_currentFilter == "Interview")
            {
                applications = applications
                    .Where(application =>
                        (
                            !string.IsNullOrWhiteSpace(application.Status) &&
                            application.Status.Contains("Interview", StringComparison.OrdinalIgnoreCase)
                        )
                        ||
                        application.InterviewAt != null);
            }
            else if (_currentFilter == "Applied")
            {
                applications = applications
                    .Where(application =>
                        string.Equals(
                            application.Status?.Trim(),
                            "Applied",
                            StringComparison.OrdinalIgnoreCase)
                        ||
                        application.AppliedAt != null);
            }
            else if (_currentFilter != "All")
            {
                applications = applications
                    .Where(application =>
                        string.Equals(
                            application.Status?.Trim(),
                            _currentFilter,
                            StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(_searchText))
            {
                var normalizedSearch = _searchText.Trim();

                applications = applications
                    .Where(application =>
                        !string.IsNullOrWhiteSpace(application.JobTitle) &&
                        application.JobTitle.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase));
            }

            return applications.ToList();
        }
    }
}