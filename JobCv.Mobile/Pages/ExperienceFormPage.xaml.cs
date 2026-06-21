using JobCv.Mobile.Models;
using JobCv.Mobile.Services;


namespace JobCv.Mobile.Pages
{
    public partial class ExperienceFormPage : ContentPage
    {
        private readonly int _cvId;
        private readonly ApiService _apiService;
        private readonly CvExperienceDto? _experienceToEdit;

        private readonly bool _isEditMode;

        public ExperienceFormPage(int cvId, ApiService apiService)
        {
            InitializeComponent();

            _cvId = cvId;
            _apiService = apiService;
            _isEditMode = false;

            StartDatePicker.Date = DateTime.Today;
            EndDatePicker.Date = DateTime.Today;
        }

        public ExperienceFormPage(int cvId, ApiService apiService, CvExperienceDto experienceToEdit)
        {
            InitializeComponent();

            _cvId = cvId;
            _apiService = apiService;
            _experienceToEdit = experienceToEdit;
            _isEditMode = true;

            LoadExperienceForEdit();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            ApplyLocalTranslations();
        }

        private void ApplyLocalTranslations()
        {
            UiTranslationService.ApplyToPage(this);
        }

        private void SetMessage(string message, Color color)
        {
            MessageLabel.TextColor = color;
            MessageLabel.Text = UiTranslationService.TranslateText(message);
        }

        private void LoadExperienceForEdit()
        {
            if (_experienceToEdit == null)
                return;

            PageTitleLabel.Text = "Edit Experience";
            SaveButton.Text = "Save changes";

            JobTitleEntry.Text = _experienceToEdit.JobTitle;
            CompanyEntry.Text = _experienceToEdit.Company;
            DescriptionEditor.Text = _experienceToEdit.Description;
            IsCurrentCheckBox.IsChecked = _experienceToEdit.IsCurrent;

            if (_experienceToEdit.StartDate.HasValue)
            {
                UseStartDateCheckBox.IsChecked = true;
                StartDatePicker.IsEnabled = true;
                StartDatePicker.Date = _experienceToEdit.StartDate.Value;
            }
            else
            {
                UseStartDateCheckBox.IsChecked = false;
                StartDatePicker.IsEnabled = false;
                StartDatePicker.Date = DateTime.Today;
            }

            if (_experienceToEdit.EndDate.HasValue && !_experienceToEdit.IsCurrent)
            {
                UseEndDateCheckBox.IsChecked = true;
                EndDatePicker.IsEnabled = true;
                EndDatePicker.Date = _experienceToEdit.EndDate.Value;
            }
            else
            {
                UseEndDateCheckBox.IsChecked = false;
                EndDatePicker.IsEnabled = false;
                EndDatePicker.Date = DateTime.Today;
            }

            UpdateEndDateState();
        }

        private void OnUseStartDateChanged(object sender, CheckedChangedEventArgs e)
        {
            StartDatePicker.IsEnabled = e.Value;
        }

        private void OnUseEndDateChanged(object sender, CheckedChangedEventArgs e)
        {
            UpdateEndDateState();
        }

        private void OnIsCurrentChanged(object sender, CheckedChangedEventArgs e)
        {
            UpdateEndDateState();
        }

        private void UpdateEndDateState()
        {
            if (IsCurrentCheckBox.IsChecked)
            {
                UseEndDateCheckBox.IsChecked = false;
                UseEndDateCheckBox.IsEnabled = false;
                EndDatePicker.IsEnabled = false;
                return;
            }

            UseEndDateCheckBox.IsEnabled = true;
            EndDatePicker.IsEnabled = UseEndDateCheckBox.IsChecked;
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
            var description = DescriptionEditor.Text?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(jobTitle))
            {
                SetMessage("Job title is required.", Colors.Red);
                return;
            }

            if (string.IsNullOrWhiteSpace(company))
            {
                SetMessage("Company is required.", Colors.Red);
                return;
            }

            DateTime? startDate = UseStartDateCheckBox.IsChecked
                ? StartDatePicker.Date
                : null;

            DateTime? endDate = UseEndDateCheckBox.IsChecked && !IsCurrentCheckBox.IsChecked
                ? EndDatePicker.Date
                : null;

            if (startDate.HasValue && endDate.HasValue && endDate.Value < startDate.Value)
            {
                SetMessage("End date cannot be before start date.", Colors.Red);
                return;
            }

            if (_isEditMode)
            {
                await UpdateExperienceAsync(
                    jobTitle,
                    company,
                    description,
                    startDate,
                    endDate,
                    IsCurrentCheckBox.IsChecked);

                return;
            }

            await AddExperienceAsync(
                jobTitle,
                company,
                description,
                startDate,
                endDate,
                IsCurrentCheckBox.IsChecked);
        }

        private async Task AddExperienceAsync(
            string jobTitle,
            string company,
            string description,
            DateTime? startDate,
            DateTime? endDate,
            bool isCurrent)
        {
            try
            {
                var result = await _apiService.AddExperienceAsync(_cvId, new AddExperienceRequest
                {
                    JobTitle = jobTitle,
                    Company = company,
                    Description = description,
                    StartDate = startDate,
                    EndDate = endDate,
                    IsCurrent = isCurrent
                });

                if (result == null)
                {
                    SetMessage("Experience could not be saved.", Colors.Red);
                    return;
                }

                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await UiTranslationService.DisplayAlertAsync(this, "Error", UiTranslationService.TranslateText(ex.Message));
            }
        }

        private async Task UpdateExperienceAsync(
            string jobTitle,
            string company,
            string description,
            DateTime? startDate,
            DateTime? endDate,
            bool isCurrent)
        {
            if (_experienceToEdit == null)
                return;

            try
            {
                var updated = await _apiService.UpdateExperienceAsync(_experienceToEdit.Id, new UpdateExperienceRequest
                {
                    JobTitle = jobTitle,
                    Company = company,
                    Description = description,
                    StartDate = startDate,
                    EndDate = endDate,
                    IsCurrent = isCurrent
                });

                if (!updated)
                {
                    SetMessage("Experience could not be updated.", Colors.Red);
                    return;
                }

                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await UiTranslationService.DisplayAlertAsync(this, "Error", UiTranslationService.TranslateText(ex.Message));
            }
        }
    }
}