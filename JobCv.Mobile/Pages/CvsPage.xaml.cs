using JobCv.Mobile.Models;
using JobCv.Mobile.Services;

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

            var displayName = string.IsNullOrWhiteSpace(_user.FullName)
                ? _user.Email
                : _user.FullName;

            SetLabelText(
                "WelcomeLabel",
                $"Welcome, {displayName}. Manage your CVs, jobs and applications in one place.");

            if (PeriodPicker != null)
                PeriodPicker.SelectedIndex = 1;
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
                _createdCvs = await _apiService.GetUserCvsAsync(_user.Id);
                _uploadedCvs = await _apiService.GetUploadedCvFilesAsync(_user.Id);
                _applications = await _apiService.GetUserApplicationsAsync(_user.Id);

                UpdateOverviewStats();
                UpdatePipelineStats();
                UpdatePeriodStats();
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
            var applicationsForSelectedPeriod = GetApplicationsForSelectedPeriod();

            var savedCount = applicationsForSelectedPeriod.Count(x => GetApplicationStage(x) == ApplicationStage.Saved);
            var appliedCount = applicationsForSelectedPeriod.Count(x => GetApplicationStage(x) == ApplicationStage.Applied);
            var interviewCount = applicationsForSelectedPeriod.Count(x => GetApplicationStage(x) == ApplicationStage.Interview);
            var offerCount = applicationsForSelectedPeriod.Count(x => GetApplicationStage(x) == ApplicationStage.Offer);
            var rejectedCount = applicationsForSelectedPeriod.Count(x => GetApplicationStage(x) == ApplicationStage.Rejected);

            SetLabelText("SavedJobsCountLabel", savedCount.ToString());
            SetLabelText("AppliedJobsCountLabel", appliedCount.ToString());
            SetLabelText("PipelineInterviewsCountLabel", interviewCount.ToString());
            SetLabelText("OffersCountLabel", offerCount.ToString());
            SetLabelText("RejectedCountLabel", rejectedCount.ToString());
        }

        private void UpdatePeriodStats()
        {
            var selectedPeriod = PeriodPicker?.SelectedItem?.ToString() ?? "Last 30 days";
            var normalizedPeriod = NormalizePeriod(selectedPeriod);
            var startDate = GetPeriodStartDate(normalizedPeriod);

            var createdCvCount = CountByPeriod(_createdCvs, x => x.CreatedAt, startDate);
            var uploadedCvCount = CountByPeriod(_uploadedCvs, x => x.UploadedAt, startDate);
            var applicationCount = CountByPeriod(_applications, x => x.CreatedAt, startDate);
            var interviewCount = _applications.Count(application =>
            {
                if (application.InterviewAt.HasValue)
                    return IsInPeriod(application.InterviewAt.Value, startDate);

                return GetApplicationStage(application) == ApplicationStage.Interview &&
                       IsInPeriod(application.CreatedAt, startDate);
            });

            SetLabelText("PeriodCreatedCvCountLabel", createdCvCount.ToString());
            SetLabelText("PeriodUploadedCvCountLabel", uploadedCvCount.ToString());
            SetLabelText("PeriodApplicationsCountLabel", applicationCount.ToString());
            SetLabelText("PeriodInterviewsCountLabel", interviewCount.ToString());

            var periodText = normalizedPeriod == "All time"
                ? "Showing activity for all time."
                : $"Showing activity for {normalizedPeriod.ToLowerInvariant()}.";

            SetLabelText("PeriodSummaryLabel", UiTranslationService.TranslateText(periodText));
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

        private static string NormalizePeriod(string? selectedPeriod)
        {
            if (string.IsNullOrWhiteSpace(selectedPeriod))
                return "Last 30 days";

            return selectedPeriod.Trim() switch
            {
                "Ultimele 7 zile" => "Last 7 days",
                "Ultimele 30 de zile" => "Last 30 days",
                "Ultimele 90 de zile" => "Last 90 days",
                "Toată perioada" => "All time",
                "Last 7 days" => "Last 7 days",
                "Last 30 days" => "Last 30 days",
                "Last 90 days" => "Last 90 days",
                "All time" => "All time",
                _ => "Last 30 days"
            };
        }

        private static bool IsInPeriod(DateTime date, DateTime? startDate)
        {
            if (startDate == null)
                return true;

            return date.Date >= startDate.Value.Date;
        }

        private List<JobApplicationDto> GetApplicationsForSelectedPeriod()
        {
            var selectedPeriod = PeriodPicker?.SelectedItem?.ToString() ?? "Last 30 days";
            var normalizedPeriod = NormalizePeriod(selectedPeriod);
            var startDate = GetPeriodStartDate(normalizedPeriod);

            return _applications
                .Where(application => IsInPeriod(application.CreatedAt, startDate))
                .ToList();
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

            var applicationsForSelectedPeriod = GetApplicationsForSelectedPeriod();

            var savedCount = applicationsForSelectedPeriod.Count(x => GetApplicationStage(x) == ApplicationStage.Saved);
            var appliedCount = applicationsForSelectedPeriod.Count(x => GetApplicationStage(x) == ApplicationStage.Applied);
            var interviewCount = applicationsForSelectedPeriod.Count(x => GetApplicationStage(x) == ApplicationStage.Interview);
            var offerCount = applicationsForSelectedPeriod.Count(x => GetApplicationStage(x) == ApplicationStage.Offer);
            var rejectedCount = applicationsForSelectedPeriod.Count(x => GetApplicationStage(x) == ApplicationStage.Rejected);

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

        private enum ApplicationStage
        {
            Saved,
            Applied,
            Interview,
            Offer,
            Rejected
        }

        private static ApplicationStage GetApplicationStage(JobApplicationDto application)
        {
            var status = NormalizeStatus(application.Status);

            if (status == "Rejected")
                return ApplicationStage.Rejected;

            if (status == "Offer")
                return ApplicationStage.Offer;

            if (status == "Interview" || application.InterviewAt.HasValue)
                return ApplicationStage.Interview;

            if (status == "Applied" || application.AppliedAt.HasValue)
                return ApplicationStage.Applied;

            return ApplicationStage.Saved;
        }

        private static string NormalizeStatus(string? status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return "Saved";

            var value = status.Trim();
            var compactValue = value.Replace(" ", string.Empty);

            if (value.Equals("Salvat", StringComparison.OrdinalIgnoreCase))
                return "Saved";

            if (value.Equals("Aplicat", StringComparison.OrdinalIgnoreCase))
                return "Applied";

            if (value.Equals("Respins", StringComparison.OrdinalIgnoreCase))
                return "Rejected";

            if (value.Equals("Ofertă", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("Oferta", StringComparison.OrdinalIgnoreCase) ||
                compactValue.Equals("OfferReceived", StringComparison.OrdinalIgnoreCase) ||
                compactValue.Equals("Offered", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("Offer", StringComparison.OrdinalIgnoreCase))
            {
                return "Offer";
            }

            if (value.Contains("Interview", StringComparison.OrdinalIgnoreCase) ||
                value.Contains("Interviu", StringComparison.OrdinalIgnoreCase) ||
                compactValue.Equals("InterviewScheduled", StringComparison.OrdinalIgnoreCase) ||
                compactValue.Equals("InterviewCompleted", StringComparison.OrdinalIgnoreCase) ||
                compactValue.Equals("InterviewDone", StringComparison.OrdinalIgnoreCase))
            {
                return "Interview";
            }

            if (value.Equals("Applied", StringComparison.OrdinalIgnoreCase))
                return "Applied";

            if (value.Equals("Rejected", StringComparison.OrdinalIgnoreCase))
                return "Rejected";

            return "Saved";
        }

        private static bool IsInterviewApplication(JobApplicationDto application)
        {
            return application.InterviewAt.HasValue ||
                   NormalizeStatus(application.Status) == "Interview";
        }

        private void OnPeriodChanged(object sender, EventArgs e)
        {
            if (_createdCvs.Count == 0 &&
                _uploadedCvs.Count == 0 &&
                _applications.Count == 0)
            {
                return;
            }

            UpdatePeriodStats();
            UpdatePipelineStats();
            UpdateQuickInsight();
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
        }
        private async void OnStartCareerTestClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CareerTestPage(_user, _apiService));
        }
    }
}