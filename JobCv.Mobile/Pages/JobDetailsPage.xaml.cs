using JobCv.Mobile.Models;
using JobCv.Mobile.Services;
using Microsoft.Maui.ApplicationModel;
using System.Net;

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


            SalaryLabel.Text = _job.DisplaySalary;

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
                await UiTranslationService.DisplayAlertAsync(
                    this,
                    "Missing link",
                    "This job does not have an application link.");
                return;
            }

            try
            {
                await Launcher.OpenAsync(_job.ApplyUrl);
            }
            catch
            {
                await UiTranslationService.DisplayAlertAsync(
                    this,
                    "Error",
                    "The job link could not be opened.");
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



        private async void OnTrackApplicationClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ApplicationFormPage(_user, _apiService, _job));
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
                var existingApplications = await _apiService.GetUserApplicationsAsync(_user.Id);

                var alreadySavedOrTracked = existingApplications.Any(existing =>
                    IsSameJob(existing, _job));

                if (alreadySavedOrTracked)
                {
                    await UiTranslationService.DisplayAlertAsync(
                        this,
                        "Already saved",
                        "This job is already saved or tracked in My Applications.");

                    MessageLabel.TextColor = Color.FromArgb("#FDE68A");
                    MessageLabel.Text = UiTranslationService.TranslateText("This job is already saved or tracked.");
                    return;
                }

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
                    SalaryRange = _job.DisplaySalary,
                    ContactPerson = string.Empty
                };

                var saved = await _apiService.CreateApplicationAsync(request);

                if (saved == null)
                {
                    MessageLabel.TextColor = Colors.Red;
                    MessageLabel.Text = UiTranslationService.TranslateText("The job could not be saved.");
                    return;
                }

                await UiTranslationService.DisplayAlertAsync(
                    this,
                    "Job saved",
                    "This job was saved successfully. You can find it in My Applications with status Saved.");

                MessageLabel.TextColor = Color.FromArgb("#BBF7D0");
                MessageLabel.Text = UiTranslationService.TranslateText("Job saved successfully.");
            }
            catch (Exception ex)
            {
                MessageLabel.TextColor = Colors.Red;
                MessageLabel.Text = UiTranslationService.TranslateText(ex.Message);
            }
            finally
            {
                _isSavingJob = false;
            }
        }

        private static bool IsSameJob(JobApplicationDto existingApplication, JobSearchResultDto currentJob)
        {
            var existingExternalId = existingApplication.JobExternalId?.Trim();
            var currentExternalId = currentJob.ExternalId?.Trim();

            if (!string.IsNullOrWhiteSpace(existingExternalId) &&
                !string.IsNullOrWhiteSpace(currentExternalId) &&
                string.Equals(existingExternalId, currentExternalId, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            var existingUrl = NormalizeText(existingApplication.JobUrl);
            var currentUrl = NormalizeText(currentJob.ApplyUrl);

            if (!string.IsNullOrWhiteSpace(existingUrl) &&
                !string.IsNullOrWhiteSpace(currentUrl) &&
                string.Equals(existingUrl, currentUrl, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            var existingTitle = NormalizeText(existingApplication.JobTitle);
            var currentTitle = NormalizeText(currentJob.Title);

            var existingCompany = NormalizeText(existingApplication.Company);
            var currentCompany = NormalizeText(currentJob.Company);

            if (!string.IsNullOrWhiteSpace(existingTitle) &&
                !string.IsNullOrWhiteSpace(currentTitle) &&
                !string.IsNullOrWhiteSpace(existingCompany) &&
                !string.IsNullOrWhiteSpace(currentCompany) &&
                string.Equals(existingTitle, currentTitle, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(existingCompany, currentCompany, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        private static string NormalizeText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : value.Trim();
        }
        private async void OnViewFullJobPostClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_job.ApplyUrl))
            {
                await UiTranslationService.DisplayAlertAsync(
                    this,
                    "Missing link",
                    "This job does not have a link to the full post.");
                return;
            }

            try
            {
                await Launcher.OpenAsync(_job.ApplyUrl);
            }
            catch
            {
                await UiTranslationService.DisplayAlertAsync(
                    this,
                    "Error",
                    "The full job post could not be opened.");
            }
        }
    }
}