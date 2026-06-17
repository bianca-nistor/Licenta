using JobCv.Mobile.Models;
using JobCv.Mobile.Services;
using Microsoft.Maui.ApplicationModel;

namespace JobCv.Mobile.Pages
{
    public partial class CvsPage : ContentPage
    {
        private readonly UserDto _user;
        private readonly ApiService _apiService;

        private List<CvDto> _createdCvs = new();
        private List<UploadedCvFileDto> _uploadedCvs = new();
        private List<JobApplicationDto> _applications = new();

        public CvsPage(UserDto user, ApiService apiService)
        {
            InitializeComponent();

            _user = user;
            _apiService = apiService;

<<<<<<< Updated upstream
            WelcomeLabel.Text = $"Welcome, {_user.FullName}. Manage your CVs here.";
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadDataAsync();
        }

=======
            var displayName = string.IsNullOrWhiteSpace(_user.FullName)
                ? _user.Email
                : _user.FullName;

            SetLabelText(
                "WelcomeLabel",
                $"Welcome, {displayName}. Manage your CVs, jobs and applications in one place.");

            if (PeriodPicker != null)
                PeriodPicker.SelectedIndex = 1;
        }

>>>>>>> Stashed changes
        private T GetControl<T>(string name) where T : Element
        {
            var control = this.FindByName<T>(name);

            if (control == null)
                throw new Exception($"Control '{name}' was not found in CvsPage.xaml.");

            return control;
        }

        private async Task LoadDataAsync()
        {
            var createdCvs = await _apiService.GetUserCvsAsync(_user.Id);
            var uploadedCvs = await _apiService.GetUploadedCvFilesAsync(_user.Id);

            GetControl<CollectionView>("CvsCollectionView").ItemsSource = createdCvs;
            GetControl<CollectionView>("UploadedCvsCollectionView").ItemsSource = uploadedCvs;

<<<<<<< Updated upstream
            GetControl<Border>("EmptyCreatedCvsState").IsVisible = createdCvs.Count == 0;
            GetControl<Border>("EmptyUploadedCvsState").IsVisible = uploadedCvs.Count == 0;
=======
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
                _createdCvs = await _apiService.GetUserCvsAsync(_user.Id);
                _uploadedCvs = await _apiService.GetUploadedCvFilesAsync(_user.Id);
                _applications = await _apiService.GetUserApplicationsAsync(_user.Id);

                UpdateOverviewStats();
                UpdatePipelineStats();
                await UpdatePeriodStatsAsync();
                UpdateQuickInsight();
                UpdateRecentApplications();
            }
            catch (Exception ex)
            {
                var emptyLabel = this.FindByName<Label>("RecentApplicationsEmptyLabel");

                if (emptyLabel != null)
                {
                    emptyLabel.IsVisible = true;
                    emptyLabel.Text = $"Dashboard could not be loaded: {ex.Message}";
                }

                ResetDashboardStats();
            }
        }

        private void ResetDashboardStats()
        {
            SetLabelText("CreatedCvCountLabel", "0");
            SetLabelText("UploadedCvCountLabel", "0");
            SetLabelText("ApplicationsCountLabel", "0");
            SetLabelText("InterviewsCountLabel", "0");

            SetLabelText("SavedJobsCountLabel", "0");
            SetLabelText("AppliedJobsCountLabel", "0");
            SetLabelText("PipelineInterviewsCountLabel", "0");
            SetLabelText("OffersCountLabel", "0");
            SetLabelText("RejectedCountLabel", "0");

            SetLabelText("PeriodCreatedCvCountLabel", "0");
            SetLabelText("PeriodUploadedCvCountLabel", "0");
            SetLabelText("PeriodApplicationsCountLabel", "0");
            SetLabelText("PeriodInterviewsCountLabel", "0");

            SetLabelText("PeriodSummaryLabel", "Dashboard data could not be loaded.");
            SetLabelText("QuickInsightLabel", "No insight available right now.");
        }

        private void UpdateOverviewStats()
        {
            SetLabelText("CreatedCvCountLabel", _createdCvs.Count.ToString());
            SetLabelText("UploadedCvCountLabel", _uploadedCvs.Count.ToString());
            SetLabelText("ApplicationsCountLabel", _applications.Count.ToString());

            var interviewsCount = _applications.Count(IsInterviewApplication);
            SetLabelText("InterviewsCountLabel", interviewsCount.ToString());
        }

        private void UpdatePipelineStats()
        {
            var savedCount = _applications.Count(x => IsStatus(x, "Saved"));

            var appliedCount = _applications.Count(x =>
                IsStatus(x, "Applied") ||
                x.AppliedAt.HasValue);

            var interviewCount = _applications.Count(IsInterviewApplication);

            var offerCount = _applications.Count(x => IsStatus(x, "Offer"));
            var rejectedCount = _applications.Count(x => IsStatus(x, "Rejected"));

            SetLabelText("SavedJobsCountLabel", savedCount.ToString());
            SetLabelText("AppliedJobsCountLabel", appliedCount.ToString());
            SetLabelText("PipelineInterviewsCountLabel", interviewCount.ToString());
            SetLabelText("OffersCountLabel", offerCount.ToString());
            SetLabelText("RejectedCountLabel", rejectedCount.ToString());
        }

        private async Task UpdatePeriodStatsAsync()
        {
            var selectedPeriod = PeriodPicker?.SelectedItem?.ToString() ?? "Last 30 days";
            var periodKey = GetPeriodKey(selectedPeriod);

            try
            {
                var stats = await _apiService.GetDashboardActivityStatsAsync(_user.Id, periodKey);

                if (stats != null)
                {
                    SetLabelText("PeriodCreatedCvCountLabel", stats.CreatedCvsCount.ToString());
                    SetLabelText("PeriodUploadedCvCountLabel", stats.UploadedCvsCount.ToString());
                    SetLabelText("PeriodApplicationsCountLabel", stats.ApplicationsCount.ToString());
                    SetLabelText("PeriodInterviewsCountLabel", stats.InterviewsCount.ToString());

                    SetLabelText(
                        "PeriodSummaryLabel",
                        string.IsNullOrWhiteSpace(stats.Summary)
                            ? BuildPeriodSummary(selectedPeriod)
                            : stats.Summary);

                    return;
                }
            }
            catch
            {
                // If the deployed API does not have the new dashboard endpoint yet,
                // keep the dashboard usable by falling back to the local data already loaded.
            }

            UpdatePeriodStatsFromLoadedData(selectedPeriod);
        }

        private void UpdatePeriodStatsFromLoadedData(string selectedPeriod)
        {
            var startDate = GetPeriodStartDate(selectedPeriod);

            var createdCvCount = CountByPeriod(_createdCvs, x => x.CreatedAt, startDate);
            var uploadedCvCount = CountByPeriod(_uploadedCvs, x => x.UploadedAt, startDate);
            var applicationCount = CountByPeriod(_applications, x => x.CreatedAt, startDate);
            var interviewCount = _applications.Count(application =>
                application.InterviewAt.HasValue &&
                IsInPeriod(application.InterviewAt.Value, startDate));

            SetLabelText("PeriodCreatedCvCountLabel", createdCvCount.ToString());
            SetLabelText("PeriodUploadedCvCountLabel", uploadedCvCount.ToString());
            SetLabelText("PeriodApplicationsCountLabel", applicationCount.ToString());
            SetLabelText("PeriodInterviewsCountLabel", interviewCount.ToString());
            SetLabelText("PeriodSummaryLabel", BuildPeriodSummary(selectedPeriod));
        }

        private static string GetPeriodKey(string selectedPeriod)
        {
            return selectedPeriod switch
            {
                "Last 7 days" => "last7",
                "Last 30 days" => "last30",
                "Last 90 days" => "last90",
                "All time" => "all",
                _ => "last30"
            };
        }

        private static string BuildPeriodSummary(string selectedPeriod)
        {
            return selectedPeriod == "All time"
                ? "Showing activity for all time."
                : $"Showing activity for {selectedPeriod.ToLower()}.";
        }

        private int CountByPeriod<T>(
            IEnumerable<T> items,
            Func<T, DateTime> dateSelector,
            DateTime? startDate)
        {
            return items.Count(item => IsInPeriod(dateSelector(item), startDate));
        }

        private static DateTime? GetPeriodStartDate(string selectedPeriod)
        {
            var today = DateTime.Today;

            return selectedPeriod switch
            {
                "Last 7 days" => today.AddDays(-7),
                "Last 30 days" => today.AddDays(-30),
                "Last 90 days" => today.AddDays(-90),
                "All time" => null,
                _ => today.AddDays(-30)
            };
        }

        private static bool IsInPeriod(DateTime date, DateTime? startDate)
        {
            if (startDate == null)
                return true;

            return date.Date >= startDate.Value.Date;
        }

        private void UpdateQuickInsight()
        {
            if (_applications.Count == 0)
            {
                SetLabelText(
                    "QuickInsightLabel",
                    "You have no saved or tracked applications yet. Start by searching jobs and saving the best opportunities.");
                return;
            }

            var savedCount = _applications.Count(x => IsStatus(x, "Saved"));
            var appliedCount = _applications.Count(x => IsStatus(x, "Applied") || x.AppliedAt.HasValue);
            var interviewCount = _applications.Count(IsInterviewApplication);
            var offerCount = _applications.Count(x => IsStatus(x, "Offer"));
            var rejectedCount = _applications.Count(x => IsStatus(x, "Rejected"));

            if (interviewCount > 0)
            {
                SetLabelText(
                    "QuickInsightLabel",
                    $"You have {interviewCount} application(s) in the interview stage. Use Interview Prep to prepare before the next meeting.");
                return;
            }

            if (offerCount > 0)
            {
                SetLabelText(
                    "QuickInsightLabel",
                    $"Great progress. You have {offerCount} offer(s). Review your applications and keep tracking the next steps.");
                return;
            }

            if (savedCount > appliedCount && savedCount > 0)
            {
                SetLabelText(
                    "QuickInsightLabel",
                    $"Most of your opportunities are still saved. Choose the best matches and move them to Applied when you send your CV.");
                return;
            }

            if (rejectedCount > appliedCount && rejectedCount > 0)
            {
                SetLabelText(
                    "QuickInsightLabel",
                    "You have several rejected applications. Review your CV Quality Check and use AI Match Score before applying again.");
                return;
            }

            SetLabelText(
                "QuickInsightLabel",
                "Your activity is balanced. Keep creating targeted CVs and tracking each application.");
        }

        private void UpdateRecentApplications()
        {
            var recentApplications = _applications
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

        private static bool IsStatus(JobApplicationDto application, string status)
        {
            return string.Equals(
                application.Status?.Trim(),
                status,
                StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsInterviewApplication(JobApplicationDto application)
        {
            return application.InterviewAt.HasValue ||
                   (
                       !string.IsNullOrWhiteSpace(application.Status) &&
                       application.Status.Contains("Interview", StringComparison.OrdinalIgnoreCase)
                   );
        }

        private async void OnPeriodChanged(object sender, EventArgs e)
        {
            await UpdatePeriodStatsAsync();
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
>>>>>>> Stashed changes
        }

        private async void OnCreateCvClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CreateCvPage(_user, _apiService));
        }

        private async void OnUploadCvClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new UploadCvPage(_user, _apiService));
        }

        private async void OnViewDetailsClicked(object sender, EventArgs e)
        {
            if ((sender as Button)?.CommandParameter is not int cvId)
                return;

            await Navigation.PushAsync(new CvDetailsPage(cvId, _apiService));
        }

        private async void OnOpenUploadedCvClicked(object sender, EventArgs e)
        {
            if ((sender as Button)?.CommandParameter is not UploadedCvFileDto uploadedFile)
                return;

            var extension = Path.GetExtension(uploadedFile.OriginalFileName).ToLowerInvariant();

            var downloadUrl = _apiService.GetUploadedCvDownloadUrl(uploadedFile.Id);

            if (extension == ".pdf")
            {
                var previewUrl = _apiService.GetUploadedCvPreviewUrl(uploadedFile.Id);

                await Navigation.PushAsync(
                    new UploadedCvPreviewPage(
                        uploadedFile.OriginalFileName,
                        previewUrl,
                        downloadUrl));

                return;
            }

            try
            {
                await Launcher.OpenAsync(downloadUrl);
            }
            catch
            {
                await DisplayAlert("Error", "The file could not be opened.", "OK");
            }
        }
        private async void OnPreviewCvPdfClicked(object sender, EventArgs e)
        {
            if ((sender as Button)?.CommandParameter is not CvDto cv)
                return;

            var previewUrl = _apiService.GetCvPdfPreviewUrl(cv.Id);
            var downloadUrl = _apiService.GetCvPdfDownloadUrl(cv.Id);

            await Navigation.PushAsync(
                new UploadedCvPreviewPage(
                    cv.Title,
                    previewUrl,
                    downloadUrl));
        }
        private async void OnDeleteCvClicked(object sender, EventArgs e)
        {
            if ((sender as Button)?.CommandParameter is not int cvId)
                return;

            var confirm = await DisplayAlert(
                "Delete CV",
                "Are you sure you want to delete this CV?",
                "Yes",
                "No");

            if (!confirm)
                return;

            var deleted = await _apiService.DeleteCvAsync(cvId);

            if (!deleted)
            {
                await DisplayAlert("Error", "The CV could not be deleted.", "OK");
                return;
            }

            await LoadDataAsync();
        }

        private async void OnDeleteUploadedCvClicked(object sender, EventArgs e)
        {
            if ((sender as Button)?.CommandParameter is not int fileId)
                return;

            var confirm = await DisplayAlert(
                "Delete uploaded CV",
                "Are you sure you want to delete this uploaded CV?",
                "Yes",
                "No");

            if (!confirm)
                return;

            var deleted = await _apiService.DeleteUploadedCvFileAsync(fileId);

            if (!deleted)
            {
                await DisplayAlert("Error", "The uploaded CV could not be deleted.", "OK");
                return;
            }

            await LoadDataAsync();
        }
        private Border GetProfileMenu()
        {
            var menu = this.FindByName<Border>("ProfileMenu");

            if (menu == null)
            {
                throw new Exception("ProfileMenu was not found in CvsPage.xaml.");
            }

            return menu;
        }

        private void OnProfileClicked(object sender, EventArgs e)
        {
            var profileMenu = GetProfileMenu();
            profileMenu.IsVisible = !profileMenu.IsVisible;
        }

        private async void OnViewProfileClicked(object sender, EventArgs e)
        {
            var profileMenu = GetProfileMenu();
            profileMenu.IsVisible = false;

            await DisplayAlert(
    "Your profile",
    $"Name: {_user.FullName}\nEmail: {_user.Email}",
    "OK");
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            var profileMenu = GetProfileMenu();
            profileMenu.IsVisible = false;

            var confirm = await DisplayAlert(
                "Logout",
                "Are you sure you want to log out?",
                "Yes",
                "No");

            if (!confirm)
                return;

            Application.Current!.Windows[0].Page = new NavigationPage(new MainPage());
        }
<<<<<<< Updated upstream
        private async void OnFindJobsClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new JobsPage(_user, _apiService));
        }
        private async void OnMyApplicationsClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ApplicationsPage(_user, _apiService));
=======

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            if (PageTitleLabel == null)
                return;

            if (width < 360)
            {
                PageTitleLabel.FontSize = 24;
            }
            else if (width < 430)
            {
                PageTitleLabel.FontSize = 26;
            }
            else
            {
                PageTitleLabel.FontSize = 30;
            }
>>>>>>> Stashed changes
        }
        private async void OnStartCareerTestClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CareerTestPage(_user, _apiService));
        }
    }
}