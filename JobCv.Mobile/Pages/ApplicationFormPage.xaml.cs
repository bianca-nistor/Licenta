using JobCv.Mobile.Models;
using JobCv.Mobile.Services;

namespace JobCv.Mobile.Pages
{
    public partial class ApplicationFormPage : ContentPage
    {
        private readonly UserDto _user;
        private readonly ApiService _apiService;
        private readonly JobSearchResultDto _job;

        private List<CvDto> _cvs = new();

        private readonly List<string> _statuses = new()
        {
            "Saved",
            "Applied",
            "Interview Scheduled",
            "Interview Done",
            "Offer",
            "Rejected"
        };

        public ApplicationFormPage(UserDto user, ApiService apiService, JobSearchResultDto job)
        {
            InitializeComponent();

            _user = user;
            _apiService = apiService;
            _job = job;

            SetupStatusPicker();
            StatusPicker.SelectedIndex = 0;
            AppliedDatePicker.Date = DateTime.Today;
            InterviewDatePicker.Date = DateTime.Today;

            LoadJobData();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadCvsAsync();
            UiTranslationService.ApplyToPage(this);
        }

        private static string T(string value) => UiTranslationService.TranslateText(value);

        private void SetupStatusPicker()
        {
            StatusPicker.Items.Clear();

            foreach (var status in _statuses)
                StatusPicker.Items.Add(T(status));
        }

        private string GetSelectedStatusForApi()
        {
            var index = StatusPicker.SelectedIndex;

            if (index >= 0 && index < _statuses.Count)
                return _statuses[index];

            return "Saved";
        }

        private void LoadJobData()
        {
            JobTitleEntry.Text = _job.Title;
            CompanyEntry.Text = _job.Company;
            LocationEntry.Text = _job.Location;
            JobUrlEntry.Text = _job.ApplyUrl;
            SourceEntry.Text = _job.Source;
        }

        private async Task LoadCvsAsync()
        {
            _cvs = await _apiService.GetUserCvsAsync(_user.Id);

            var pickerItems = new List<CvDto>
            {
                new CvDto
                {
                    Id = 0,
                    Title = T("No CV selected")
                }
            };

            pickerItems.AddRange(_cvs);

            CvPicker.ItemsSource = pickerItems;
            CvPicker.SelectedIndex = 0;
        }

        private void OnUseAppliedDateChanged(object sender, CheckedChangedEventArgs e)
        {
            AppliedDatePicker.IsEnabled = e.Value;
        }

        private void OnUseInterviewDateChanged(object sender, CheckedChangedEventArgs e)
        {
            InterviewDatePicker.IsEnabled = e.Value;
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            MessageLabel.Text = "";

            var jobTitle = JobTitleEntry.Text?.Trim() ?? "";
            var company = CompanyEntry.Text?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(jobTitle))
            {
                MessageLabel.TextColor = Colors.Red;
                MessageLabel.Text = T("Job title is required.");
                return;
            }

            if (string.IsNullOrWhiteSpace(company))
            {
                MessageLabel.TextColor = Colors.Red;
                MessageLabel.Text = T("Company is required.");
                return;
            }

            int? selectedCvId = null;

            if (CvPicker.SelectedItem is CvDto selectedCv && selectedCv.Id > 0)
            {
                selectedCvId = selectedCv.Id;
            }

            DateTime? appliedAt = UseAppliedDateCheckBox.IsChecked
                ? AppliedDatePicker.Date
                : null;

            DateTime? interviewAt = UseInterviewDateCheckBox.IsChecked
                ? InterviewDatePicker.Date
                : null;

            var request = new CreateJobApplicationRequest
            {
                UserId = _user.Id,
                CvId = selectedCvId,
                JobExternalId = _job.ExternalId,
                JobTitle = jobTitle,
                Company = company,
                Location = LocationEntry.Text?.Trim() ?? "",
                JobUrl = JobUrlEntry.Text?.Trim() ?? "",
                Source = SourceEntry.Text?.Trim() ?? "",
                Status = GetSelectedStatusForApi(),
                AppliedAt = appliedAt,
                InterviewAt = interviewAt,
                Notes = NotesEditor.Text?.Trim() ?? "",
                InterviewNotes = InterviewNotesEditor.Text?.Trim() ?? "",
                SalaryRange = SalaryRangeEntry.Text?.Trim() ?? "",
                ContactPerson = ContactPersonEntry.Text?.Trim() ?? ""
            };

            try
            {
                var saved = await _apiService.CreateApplicationAsync(request);

                if (saved == null)
                {
                    MessageLabel.TextColor = Colors.Red;
                    MessageLabel.Text = T("The application could not be saved.");
                    return;
                }

                await UiTranslationService.DisplayAlertAsync(
                    this,
                    "Application saved",
                    "This job application was saved successfully.");

                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                MessageLabel.TextColor = Colors.Red;
                MessageLabel.Text = T(ex.Message);
            }
        }
    }
}