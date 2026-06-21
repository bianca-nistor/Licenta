using JobCv.Mobile.Models;
using JobCv.Mobile.Services;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel.DataTransfer;

namespace JobCv.Mobile.Pages
{
    public partial class ApplicationDetailsPage : ContentPage
    {
        private readonly UserDto _user;
        private readonly ApiService _apiService;
        private readonly int _applicationId;

        private JobApplicationDto? _application;
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

        public ApplicationDetailsPage(UserDto user, ApiService apiService, JobApplicationDto application)
        {
            InitializeComponent();

            _user = user;
            _apiService = apiService;
            _applicationId = application.Id;
            _application = application;

            SetupStatusPicker();

            AppliedDatePicker.Date = DateTime.Today;
            InterviewDatePicker.Date = DateTime.Today;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await LoadCvsAsync();
            await LoadApplicationAsync();
        }

        private void SetupStatusPicker()
        {
            StatusPicker.Items.Clear();

            foreach (var status in _statuses)
            {
                StatusPicker.Items.Add(status);
            }

            if (StatusPicker.SelectedIndex < 0)
            {
                StatusPicker.SelectedIndex = 0;
            }
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

            SetStatusPickerValue(_application.Status);

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

        private void SetStatusPickerValue(string? status)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                StatusPicker.SelectedIndex = 0;
                return;
            }

            var normalizedStatus = status.Trim();

            var index = StatusPicker.Items
                .Select((item, itemIndex) => new { item, itemIndex })
                .FirstOrDefault(x =>
                    string.Equals(x.item, normalizedStatus, StringComparison.OrdinalIgnoreCase))
                ?.itemIndex ?? -1;

            StatusPicker.SelectedIndex = index >= 0 ? index : 0;
        }

        private void OnUseAppliedDateChanged(object sender, CheckedChangedEventArgs e)
        {
            AppliedDatePicker.IsEnabled = e.Value;

            if (e.Value && IsCurrentStatus("Saved"))
            {
                SetStatusPickerValue("Applied");
            }
        }

        private void OnUseInterviewDateChanged(object sender, CheckedChangedEventArgs e)
        {
            InterviewDatePicker.IsEnabled = e.Value;

            if (e.Value &&
                (IsCurrentStatus("Saved") || IsCurrentStatus("Applied")))
            {
                SetStatusPickerValue("Interview Scheduled");
            }
        }

        private bool IsCurrentStatus(string status)
        {
            return string.Equals(
                StatusPicker.SelectedItem?.ToString(),
                status,
                StringComparison.OrdinalIgnoreCase);
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

            var selectedStatus = StatusPicker.SelectedItem?.ToString() ?? "Saved";

            var request = new UpdateJobApplicationRequest
            {
                CvId = selectedCvId,
                JobTitle = jobTitle,
                Company = company,
                Location = LocationEntry.Text?.Trim() ?? string.Empty,
                JobUrl = JobUrlEntry.Text?.Trim() ?? string.Empty,
                Source = SourceEntry.Text?.Trim() ?? string.Empty,
                Status = selectedStatus,
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
        private async void OnGenerateCoverLetterClicked(object sender, EventArgs e)
        {
            if (_application == null)
                return;

            var jobTitle = JobTitleEntry.Text?.Trim() ?? string.Empty;
            var company = CompanyEntry.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(jobTitle))
            {
                await DisplayAlert("Missing job title", "Please add a job title before generating the cover letter.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(company))
            {
                await DisplayAlert("Missing company", "Please add a company before generating the cover letter.", "OK");
                return;
            }

            CvDto? selectedCv = null;

            if (CvPicker.SelectedItem is CvDto cv && cv.Id > 0)
            {
                selectedCv = cv;
            }

            var request = new CoverLetterRequest
            {
                JobTitle = jobTitle,
                Company = company,
                Location = LocationEntry.Text?.Trim() ?? string.Empty,
                JobDescription = BuildJobContextForCoverLetter(),
                CvText = BuildCvTextForCoverLetter(selectedCv),
                Language = LanguageService.CurrentLanguage
            };

            try
            {
                MessageLabel.TextColor = Colors.Gray;
                MessageLabel.Text = "Generating cover letter...";

                CoverLetterSubjectLabel.IsVisible = false;
                CoverLetterEditor.IsVisible = false;
                CopyCoverLetterButton.IsVisible = false;

                var result = await _apiService.GenerateCoverLetterAsync(request);

                if (result == null || string.IsNullOrWhiteSpace(result.Letter))
                {
                    MessageLabel.TextColor = Colors.Red;
                    MessageLabel.Text = "The cover letter could not be generated.";
                    return;
                }

                CoverLetterSubjectLabel.Text = $"Subject: {result.Subject}";
                CoverLetterEditor.Text = result.Letter;

                CoverLetterSubjectLabel.IsVisible = true;
                CoverLetterEditor.IsVisible = true;
                CopyCoverLetterButton.IsVisible = true;

                MessageLabel.TextColor = Colors.Green;
                MessageLabel.Text = result.IsMock
                    ? "Demo cover letter generated."
                    : "Cover letter generated with AI.";
            }
            catch (Exception ex)
            {
                MessageLabel.TextColor = Colors.Red;
                MessageLabel.Text = ex.Message;
            }
        }

        private async void OnCopyCoverLetterClicked(object sender, EventArgs e)
        {
            var text = CoverLetterEditor.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(text))
            {
                await DisplayAlert("Nothing to copy", "Generate a cover letter first.", "OK");
                return;
            }

            await Clipboard.Default.SetTextAsync(text);

            MessageLabel.TextColor = Colors.Green;
            MessageLabel.Text = "Cover letter copied to clipboard.";
        }

        private string BuildJobContextForCoverLetter()
        {
            var parts = new List<string>();

            var source = SourceEntry.Text?.Trim();
            var salary = SalaryRangeEntry.Text?.Trim();
            var contact = ContactPersonEntry.Text?.Trim();
            var notes = NotesEditor.Text?.Trim();
            var interviewNotes = InterviewNotesEditor.Text?.Trim();

            if (!string.IsNullOrWhiteSpace(source))
                parts.Add($"Source: {source}");

            if (!string.IsNullOrWhiteSpace(salary))
                parts.Add($"Salary range: {salary}");

            if (!string.IsNullOrWhiteSpace(contact))
                parts.Add($"Contact person: {contact}");

            if (!string.IsNullOrWhiteSpace(notes))
                parts.Add($"Application notes or job description: {notes}");

            if (!string.IsNullOrWhiteSpace(interviewNotes))
                parts.Add($"Interview notes: {interviewNotes}");

            return string.Join(Environment.NewLine, parts);
        }

        private static string BuildCvTextForCoverLetter(CvDto? cv)
        {
            if (cv == null)
                return "No CV selected.";

            var parts = new List<string>
    {
        $"CV title: {cv.Title}",
        $"Full name: {cv.FullName}",
        $"Location: {cv.Location}",
        $"Profile summary: {cv.Summary}"
    };

            if (cv.Skills.Any())
                parts.Add("Skills: " + string.Join(", ", cv.Skills.Select(skill => skill.Name)));

            if (cv.Experiences.Any())
            {
                parts.Add("Experience:");

                foreach (var exp in cv.Experiences.Take(4))
                {
                    parts.Add($"- {exp.JobTitle} at {exp.Company}: {exp.Description}");
                }
            }

            if (cv.Educations.Any())
            {
                parts.Add("Education:");

                foreach (var edu in cv.Educations.Take(3))
                {
                    parts.Add($"- {edu.Degree} at {edu.Institution}: {edu.Description}");
                }
            }

            if (cv.Projects.Any())
            {
                parts.Add("Projects:");

                foreach (var project in cv.Projects.Take(3))
                {
                    parts.Add($"- {project.Title}: {project.Description}. Technologies: {project.Technologies}");
                }
            }

            if (cv.Certifications.Any())
            {
                parts.Add("Certifications: " + string.Join(", ", cv.Certifications.Select(cert => cert.Name)));
            }

            if (cv.Languages.Any())
            {
                parts.Add("Languages: " + string.Join(", ", cv.Languages.Select(lang => $"{lang.Name} - {lang.Level}")));
            }

            return string.Join(Environment.NewLine, parts);
        }
    }
}