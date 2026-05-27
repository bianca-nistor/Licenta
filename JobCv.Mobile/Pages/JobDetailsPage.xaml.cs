using JobCv.Mobile.Models;
using JobCv.Mobile.Services;
using Microsoft.Maui.ApplicationModel;

namespace JobCv.Mobile.Pages
{
    public partial class JobDetailsPage : ContentPage
    {
        private readonly UserDto _user;
        private readonly ApiService _apiService;
        private readonly JobSearchResultDto _job;
        private bool _isSavingJob;

        public JobDetailsPage(UserDto user, ApiService apiService, JobSearchResultDto job)
        {
            InitializeComponent();

            _user = user;
            _apiService = apiService;
            _job = job;

            LoadJobDetails();
        }

        private void LoadJobDetails()
        {
            TitleLabel.Text = string.IsNullOrWhiteSpace(_job.Title)
                ? "Untitled job"
                : _job.Title;

            CompanyLabel.Text = string.IsNullOrWhiteSpace(_job.Company)
                ? "Company not available"
                : _job.Company;

            LocationLabel.Text = string.IsNullOrWhiteSpace(_job.Location)
                ? "Location not available"
                : _job.Location;

            SourceLabel.Text = string.IsNullOrWhiteSpace(_job.Source)
                ? "Source"
                : _job.Source;

            SalaryLabel.Text = string.IsNullOrWhiteSpace(_job.Salary)
                ? "Not specified"
                : _job.Salary;

            PostedLabel.Text = string.IsNullOrWhiteSpace(_job.PostedText)
                ? "Not available"
                : _job.PostedText;

            DescriptionLabel.Text = string.IsNullOrWhiteSpace(_job.Description)
                ? "No description available for this job."
                : _job.Description;
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnApplyClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_job.ApplyUrl))
            {
                await DisplayAlert("Missing link", "This job does not have an application link.", "OK");
                return;
            }

            try
            {
                await Launcher.OpenAsync(_job.ApplyUrl);
            }
            catch
            {
                await DisplayAlert("Error", "The job link could not be opened.", "OK");
            }
        }

        private async void OnCreateCvClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CreateCvPage(_user, _apiService));
        }

        private async void OnTailorCvClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CvTailoringPage(_user, _apiService, _job));
        }

        private async void OnPrepareInterviewClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new InterviewPrepPage(_user, _apiService, _job));
        }

        private async void OnMatchScoreClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CvJobMatchPage(_user, _apiService, _job));
        }

        private async void OnSaveJobClicked(object sender, EventArgs e)
        {
            if (_isSavingJob)
                return;

            _isSavingJob = true;
            MessageLabel.Text = "";
            MessageLabel.TextColor = Color.FromArgb("#CBD5E1");

            try
            {
                var request = new CreateJobApplicationRequest
                {
                    UserId = _user.Id,
                    CvId = null,
                    JobExternalId = _job.ExternalId ?? string.Empty,
                    JobTitle = string.IsNullOrWhiteSpace(_job.Title)
                        ? "Untitled job"
                        : _job.Title.Trim(),
                    Company = string.IsNullOrWhiteSpace(_job.Company)
                        ? "Company not available"
                        : _job.Company.Trim(),
                    Location = _job.Location?.Trim() ?? string.Empty,
                    JobUrl = _job.ApplyUrl?.Trim() ?? string.Empty,
                    Source = _job.Source?.Trim() ?? string.Empty,
                    Status = "Saved",
                    AppliedAt = null,
                    InterviewAt = null,
                    Notes = "Saved from Job Details.",
                    InterviewNotes = string.Empty,
                    SalaryRange = _job.Salary?.Trim() ?? string.Empty,
                    ContactPerson = string.Empty
                };

                var saved = await _apiService.CreateApplicationAsync(request);

                if (saved == null)
                {
                    MessageLabel.TextColor = Colors.Red;
                    MessageLabel.Text = "The job could not be saved.";
                    return;
                }

                await DisplayAlert(
                    "Job saved",
                    "This job was saved successfully. You can find it in My Applications with status Saved.",
                    "OK");

                MessageLabel.TextColor = Color.FromArgb("#BBF7D0");
                MessageLabel.Text = "Job saved successfully.";
            }
            catch (Exception ex)
            {
                MessageLabel.TextColor = Colors.Red;
                MessageLabel.Text = ex.Message;
            }
            finally
            {
                _isSavingJob = false;
            }
        }

        private async void OnTrackApplicationClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ApplicationFormPage(_user, _apiService, _job));
        }
    }
}