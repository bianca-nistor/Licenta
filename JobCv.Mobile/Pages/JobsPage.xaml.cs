using JobCv.Mobile.Models;
using JobCv.Mobile.Services;


namespace JobCv.Mobile.Pages
{
    public partial class JobsPage : ContentPage
    {
        private readonly UserDto _user;
        private readonly ApiService _apiService;

        private readonly List<JobSearchResultDto> _allJobs = new();
        private List<JobSearchResultDto> _filteredJobs = new();

        private int _currentPage = 1;
        private bool _isSearching;
        private string _lastQuery = string.Empty;
        private string _lastLocation = string.Empty;

        public JobsPage(UserDto user, ApiService apiService)
        {
            InitializeComponent();

            _user = user;
            _apiService = apiService;

            SetDefaultFilters();
        }

        private void SetDefaultFilters()
        {
            WorkModePicker.SelectedIndex = 0;
            SalaryFilterPicker.SelectedIndex = 0;
            SourceFilterPicker.SelectedIndex = 0;
            SortPicker.SelectedIndex = 0;
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

        private Border GetProfileMenu()
        {
            return GetControl<Border>("ProfileMenu");
        }

        private async void OnSearchCompleted(object sender, EventArgs e)
        {
            await SearchJobsAsync(resetResults: true);
        }

        private async void OnSearchClicked(object sender, EventArgs e)
        {
            await SearchJobsAsync(resetResults: true);
        }

        private async void OnLoadMoreClicked(object sender, EventArgs e)
        {
            await SearchJobsAsync(resetResults: false);
        }

        private async Task SearchJobsAsync(bool resetResults)
        {
            if (_isSearching)
                return;

            var query = QueryEntry.Text?.Trim() ?? string.Empty;
            var location = LocationEntry.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(query))
            {
                SearchMessageLabel.TextColor = Colors.Red;
                SearchMessageLabel.Text = UiTranslationService.TranslateText("Please enter a job keyword before searching.");
                return;
            }

            _isSearching = true;
            SearchMessageLabel.TextColor = Color.FromArgb("#64748B");
            SearchMessageLabel.Text = UiTranslationService.TranslateText(resetResults ? "Searching jobs..." : "Loading more jobs...");

            try
            {
                if (resetResults)
                {
                    _currentPage = 1;
                    _allJobs.Clear();
                    _lastQuery = query;
                    _lastLocation = location;
                }
                else
                {
                    _currentPage++;
                    query = _lastQuery;
                    location = _lastLocation;
                }

                var jobs = await _apiService.SearchJobsAsync(query, location, _currentPage);

                if (jobs.Count == 0 && resetResults)
                {
                    _allJobs.Clear();
                    ApplyFilters();
                    EmptyJobsMessageLabel.Text = UiTranslationService.TranslateText("No jobs were found for this search.");
                    SearchMessageLabel.Text = "";
                    return;
                }

                _allJobs.AddRange(jobs);

                ApplyFilters();

                SearchMessageLabel.TextColor = Color.FromArgb("#166534");
                SearchMessageLabel.Text = UiTranslationService.TranslateText(resetResults
                    ? $"Search completed. Found {_filteredJobs.Count} visible jobs."
                    : $"Loaded more jobs. Showing {_filteredJobs.Count} jobs after filters.");
            }
            catch (Exception ex)
            {
                SearchMessageLabel.TextColor = Colors.Red;
                SearchMessageLabel.Text = UiTranslationService.TranslateText(ex.Message);
            }
            finally
            {
                _isSearching = false;
            }
        }

        private void OnFiltersChanged(object sender, EventArgs e)
        {
            if (_allJobs.Count == 0)
                return;

            ApplyFilters();
        }

        private void OnApplyFiltersClicked(object sender, EventArgs e)
        {
            ApplyFilters();
        }

        private void OnResetFiltersClicked(object sender, EventArgs e)
        {
            SetDefaultFilters();
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            IEnumerable<JobSearchResultDto> jobs = _allJobs;

            jobs = ApplyWorkModeFilter(jobs);
            jobs = ApplySalaryFilter(jobs);
            jobs = ApplySourceFilter(jobs);
            jobs = ApplySort(jobs);

            _filteredJobs = jobs.ToList();

            JobsCollectionView.ItemsSource = _filteredJobs;

            JobsCollectionView.IsVisible = _filteredJobs.Count > 0;
            EmptyJobsState.IsVisible = _filteredJobs.Count == 0;

            LoadMoreButton.IsVisible = _allJobs.Count > 0;

            JobsCountLabel.Text = _filteredJobs.Count.ToString();

            if (_allJobs.Count == 0)
            {
                FilterSummaryLabel.Text = UiTranslationService.TranslateText("Filters are applied after a search.");
                EmptyJobsMessageLabel.Text = UiTranslationService.TranslateText("Search for a role to see job offers here.");
                return;
            }

            FilterSummaryLabel.Text = UiTranslationService.TranslateText($"Showing {_filteredJobs.Count} of {_allJobs.Count} loaded jobs.");
            EmptyJobsMessageLabel.Text = UiTranslationService.TranslateText("No jobs match the selected filters.");
        }

        private IEnumerable<JobSearchResultDto> ApplyWorkModeFilter(IEnumerable<JobSearchResultDto> jobs)
        {
            // Do not use SelectedItem text here.
            // The UI translation service changes picker text in Romanian, so filtering must use indexes.
            return WorkModePicker.SelectedIndex switch
            {
                1 => jobs.Where(job => ContainsAny(job, "remote", "work from home", "wfh")),
                2 => jobs.Where(job => ContainsAny(job, "hybrid")),
                3 => jobs.Where(job => ContainsAny(job, "on-site", "onsite", "office")),
                _ => jobs
            };
        }

        private IEnumerable<JobSearchResultDto> ApplySalaryFilter(IEnumerable<JobSearchResultDto> jobs)
        {
            // 0 = Any, 1 = With salary, 2 = No salary specified
            return SalaryFilterPicker.SelectedIndex switch
            {
                1 => jobs.Where(HasSalary),
                2 => jobs.Where(job => !HasSalary(job)),
                _ => jobs
            };
        }

        private IEnumerable<JobSearchResultDto> ApplySourceFilter(IEnumerable<JobSearchResultDto> jobs)
        {
            // 0 = All, 1 = Adzuna
            if (SourceFilterPicker.SelectedIndex <= 0)
                return jobs;

            return jobs.Where(job =>
                !string.IsNullOrWhiteSpace(job.Source) &&
                job.Source.Equals("Adzuna", StringComparison.OrdinalIgnoreCase));
        }

        private IEnumerable<JobSearchResultDto> ApplySort(IEnumerable<JobSearchResultDto> jobs)
        {
            // 0 = Default, 1 = Salary available first, 2 = Company A-Z, 3 = Title A-Z
            return SortPicker.SelectedIndex switch
            {
                1 => jobs
                    .OrderByDescending(HasSalary)
                    .ThenBy(job => job.Title),

                2 => jobs
                    .OrderBy(job => job.Company)
                    .ThenBy(job => job.Title),

                3 => jobs
                    .OrderBy(job => job.Title)
                    .ThenBy(job => job.Company),

                _ => jobs
            };
        }

        private static bool HasSalary(JobSearchResultDto job)
        {
            if (string.IsNullOrWhiteSpace(job.Salary))
                return false;

            var salary = job.Salary.Trim();

            if (salary.Equals("Not specified", StringComparison.OrdinalIgnoreCase))
                return false;

            if (salary.Equals("N/A", StringComparison.OrdinalIgnoreCase))
                return false;

            return true;
        }

        private static bool ContainsAny(JobSearchResultDto job, params string[] keywords)
        {
            var text = $"{job.Title} {job.Company} {job.Location} {job.Description} {job.Source}"
                .ToLowerInvariant();

            return keywords.Any(keyword => text.Contains(keyword.ToLowerInvariant()));
        }

        private async void OnViewJobDetailsClicked(object sender, EventArgs e)
        {
            if ((sender as Button)?.CommandParameter is not JobSearchResultDto job)
                return;

            await Navigation.PushAsync(new JobDetailsPage(_user, _apiService, job));
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

        private async void OnMyApplicationsClicked(object sender, EventArgs e)
        {
            GetMainMenu().IsVisible = false;
            await Navigation.PushAsync(new ApplicationsPage(_user, _apiService));
        }

        private void OnFindJobsClicked(object sender, EventArgs e)
        {
            GetMainMenu().IsVisible = false;
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
        private void OnTranslateLoadedElement(object sender, EventArgs e)
        {
            if (sender is Element element)
                UiTranslationService.ApplyToElement(element);
        }

    }
}