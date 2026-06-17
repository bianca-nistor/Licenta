using JobCv.Mobile.Models;
using JobCv.Mobile.Services;
<<<<<<< Updated upstream
=======

>>>>>>> Stashed changes

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
<<<<<<< Updated upstream
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
=======
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
>>>>>>> Stashed changes
        }

        private async void OnSearchClicked(object sender, EventArgs e)
        {
<<<<<<< Updated upstream
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
=======
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
                SearchMessageLabel.Text = "Please enter a job keyword before searching.";
                return;
            }

            _isSearching = true;
            SearchMessageLabel.TextColor = Color.FromArgb("#64748B");
            SearchMessageLabel.Text = resetResults ? "Searching jobs..." : "Loading more jobs...";

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
                    EmptyJobsMessageLabel.Text = "No jobs were found for this search.";
                    SearchMessageLabel.Text = "";
                    return;
                }

                _allJobs.AddRange(jobs);

                ApplyFilters();

                SearchMessageLabel.TextColor = Color.FromArgb("#166534");
                SearchMessageLabel.Text = resetResults
                    ? $"Search completed. Found {_filteredJobs.Count} visible jobs."
                    : $"Loaded more jobs. Showing {_filteredJobs.Count} jobs after filters.";
            }
            catch (Exception ex)
            {
                SearchMessageLabel.TextColor = Colors.Red;
                SearchMessageLabel.Text = ex.Message;
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
                FilterSummaryLabel.Text = "Filters are applied after a search.";
                EmptyJobsMessageLabel.Text = "Search for a role to see job offers here.";
                return;
            }

            FilterSummaryLabel.Text = $"Showing {_filteredJobs.Count} of {_allJobs.Count} loaded jobs.";
            EmptyJobsMessageLabel.Text = "No jobs match the selected filters.";
        }

        private IEnumerable<JobSearchResultDto> ApplyWorkModeFilter(IEnumerable<JobSearchResultDto> jobs)
        {
            var selected = WorkModePicker.SelectedItem?.ToString() ?? "All";

            if (selected == "All")
                return jobs;

            return selected switch
            {
                "Remote" => jobs.Where(job => ContainsAny(job, "remote", "work from home", "wfh")),
                "Hybrid" => jobs.Where(job => ContainsAny(job, "hybrid")),
                "On-site" => jobs.Where(job => ContainsAny(job, "on-site", "onsite", "office")),
                _ => jobs
            };
        }

        private IEnumerable<JobSearchResultDto> ApplySalaryFilter(IEnumerable<JobSearchResultDto> jobs)
        {
            var selected = SalaryFilterPicker.SelectedItem?.ToString() ?? "Any";

            if (selected == "Any")
                return jobs;

            if (selected == "With salary")
            {
                return jobs.Where(job => HasSalary(job));
            }

            if (selected == "No salary specified")
            {
                return jobs.Where(job => !HasSalary(job));
            }

            return jobs;
        }

        private IEnumerable<JobSearchResultDto> ApplySourceFilter(IEnumerable<JobSearchResultDto> jobs)
        {
            var selected = SourceFilterPicker.SelectedItem?.ToString() ?? "All";

            if (selected == "All")
                return jobs;

            return jobs.Where(job =>
                !string.IsNullOrWhiteSpace(job.Source) &&
                job.Source.Contains(selected, StringComparison.OrdinalIgnoreCase));
        }

        private IEnumerable<JobSearchResultDto> ApplySort(IEnumerable<JobSearchResultDto> jobs)
        {
            var selected = SortPicker.SelectedItem?.ToString() ?? "Default";

            return selected switch
            {
                "Salary available first" => jobs
                    .OrderByDescending(HasSalary)
                    .ThenBy(job => job.Title),

                "Company A-Z" => jobs
                    .OrderBy(job => job.Company)
                    .ThenBy(job => job.Title),

                "Title A-Z" => jobs
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
>>>>>>> Stashed changes
        {
            if ((sender as Button)?.CommandParameter is not JobSearchResultDto job)
                return;

<<<<<<< Updated upstream
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
=======
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

            var confirm = await DisplayAlert(
                "Logout",
                "Are you sure you want to log out?",
>>>>>>> Stashed changes
                "Yes",
                "No");

            if (!confirm)
                return;

<<<<<<< Updated upstream
            await Navigation.PushAsync(new CreateCvPage(_user, _apiService));
        }

        private async void OnTrackApplicationClicked(object sender, EventArgs e)
        {
            if ((sender as Button)?.CommandParameter is not JobSearchResultDto job)
                return;

            await Navigation.PushAsync(new ApplicationFormPage(_user, _apiService, job));
        }
=======
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
       
        
>>>>>>> Stashed changes
    }
}