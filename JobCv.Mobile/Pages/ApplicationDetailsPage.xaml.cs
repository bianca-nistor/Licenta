using JobCv.Mobile.Models;
using JobCv.Mobile.Services;
using Microsoft.Maui.ApplicationModel;

namespace JobCv.Mobile.Pages
{
    public partial class ApplicationDetailsPage : ContentPage
    {
        private readonly UserDto _user;
        private readonly ApiService _apiService;
        private readonly int _applicationId;

        private JobApplicationDto? _application;
        private List<CvDto> _cvs = new();

        public ApplicationDetailsPage(UserDto user, ApiService apiService, JobApplicationDto application)
        {
            InitializeComponent();

            _user = user;
            _apiService = apiService;
            _applicationId = application.Id;
            _application = application;

            AppliedDatePicker.Date = DateTime.Today;
            InterviewDatePicker.Date = DateTime.Today;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await LoadCvsAsync();
            await LoadApplicationAsync();
        }

        private async Task LoadCvsAsync()
        {
            _cvs = await _apiService.GetUserCvsAsync(_user.Id);

            var pickerItems = new List<CvDto>
            {
                new CvDto
                {
                    Id = 0,
                    Title = "No CV selected"
                }
            };

            pickerItems.AddRange(_cvs);

            CvPicker.ItemsSource = pickerItems;
            CvPicker.SelectedIndex = 0;
        }

        private async Task LoadApplicationAsync()
        {
            MessageLabel.TextColor = Colors.Gray;
            MessageLabel.Text = "Loading application...";

            var latest = await _apiService.GetApplicationByIdAsync(_applicationId);

            if (latest == null)
            {
                MessageLabel.TextColor = Colors.Red;
                MessageLabel.Text = "The application could not be loaded.";
                return;
            }

            _application = latest;
            BindApplication();

            MessageLabel.Text = "";
        }

        private void BindApplication()
        {
            if (_application == null)
                return;

            JobTitleEntry.Text = _application.JobTitle;
            CompanyEntry.Text = _application.Company;
            LocationEntry.Text = _application.Location;
            JobUrlEntry.Text = _application.JobUrl;
            SourceEntry.Text = _application.Source;
            SalaryRangeEntry.Text = _application.SalaryRange;
            ContactPersonEntry.Text = _application.ContactPerson;
            NotesEditor.Text = _application.Notes;
            InterviewNotesEditor.Text = _application.InterviewNotes;

            var statusIndex = StatusPicker.Items.IndexOf(_application.Status);
            StatusPicker.SelectedIndex = statusIndex >= 0 ? statusIndex : 0;

            if (_application.CvId.HasValue && CvPicker.ItemsSource is List<CvDto> pickerItems)
            {
                var selected = pickerItems.FirstOrDefault(x => x.Id == _application.CvId.Value);
                if (selected != null)
                    CvPicker.SelectedItem = selected;
            }

            UseAppliedDateCheckBox.IsChecked = _application.AppliedAt.HasValue;
            AppliedDatePicker.IsEnabled = _application.AppliedAt.HasValue;
            AppliedDatePicker.Date = _application.AppliedAt?.Date ?? DateTime.Today;

            UseInterviewDateCheckBox.IsChecked = _application.InterviewAt.HasValue;
            InterviewDatePicker.IsEnabled = _application.InterviewAt.HasValue;
            InterviewDatePicker.Date = _application.InterviewAt?.Date ?? DateTime.Today;
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

        private async void OnOpenJobClicked(object sender, EventArgs e)
        {
            var url = JobUrlEntry.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(url))
            {
                await DisplayAlert("Missing link", "This application does not have a job link.", "OK");
                return;
            }

            try
            {
                await Launcher.OpenAsync(url);
            }
            catch
            {
                await DisplayAlert("Error", "The job link could not be opened.", "OK");
            }
        }

        private async void OnPrepareInterviewClicked(object sender, EventArgs e)
        {
            if (_application == null)
                return;

            var job = new JobSearchResultDto
            {
                ExternalId = _application.JobExternalId,
                Title = JobTitleEntry.Text?.Trim() ?? _application.JobTitle,
                Company = CompanyEntry.Text?.Trim() ?? _application.Company,
                Location = LocationEntry.Text?.Trim() ?? _application.Location,
                ApplyUrl = JobUrlEntry.Text?.Trim() ?? _application.JobUrl,
                Source = SourceEntry.Text?.Trim() ?? _application.Source,
                Salary = SalaryRangeEntry.Text?.Trim() ?? _application.SalaryRange,
                Description = NotesEditor.Text?.Trim() ?? string.Empty
            };

            await Navigation.PushAsync(new InterviewPrepPage(_user, _apiService, job));
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            if (_application == null)
                return;

            MessageLabel.Text = "";

            var jobTitle = JobTitleEntry.Text?.Trim() ?? string.Empty;
            var company = CompanyEntry.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(jobTitle))
            {
                MessageLabel.TextColor = Colors.Red;
                MessageLabel.Text = "Job title is required.";
                return;
            }

            if (string.IsNullOrWhiteSpace(company))
            {
                MessageLabel.TextColor = Colors.Red;
                MessageLabel.Text = "Company is required.";
                return;
            }

            int? selectedCvId = null;

            if (CvPicker.SelectedItem is CvDto selectedCv && selectedCv.Id > 0)
                selectedCvId = selectedCv.Id;

            var request = new UpdateJobApplicationRequest
            {
                CvId = selectedCvId,
                JobTitle = jobTitle,
                Company = company,
                Location = LocationEntry.Text?.Trim() ?? string.Empty,
                JobUrl = JobUrlEntry.Text?.Trim() ?? string.Empty,
                Source = SourceEntry.Text?.Trim() ?? string.Empty,
                Status = StatusPicker.SelectedItem?.ToString() ?? "Saved",
                AppliedAt = UseAppliedDateCheckBox.IsChecked ? AppliedDatePicker.Date : null,
                InterviewAt = UseInterviewDateCheckBox.IsChecked ? InterviewDatePicker.Date : null,
                Notes = NotesEditor.Text?.Trim() ?? string.Empty,
                InterviewNotes = InterviewNotesEditor.Text?.Trim() ?? string.Empty,
                SalaryRange = SalaryRangeEntry.Text?.Trim() ?? string.Empty,
                ContactPerson = ContactPersonEntry.Text?.Trim() ?? string.Empty
            };

            try
            {
                var updated = await _apiService.UpdateApplicationAsync(_application.Id, request);

                if (updated == null)
                {
                    MessageLabel.TextColor = Colors.Red;
                    MessageLabel.Text = "The application could not be updated.";
                    return;
                }

                _application = updated;
                BindApplication();

                MessageLabel.TextColor = Colors.Green;
                MessageLabel.Text = "Application saved successfully.";
            }
            catch (Exception ex)
            {
                MessageLabel.TextColor = Colors.Red;
                MessageLabel.Text = ex.Message;
            }
        }
    }
}
