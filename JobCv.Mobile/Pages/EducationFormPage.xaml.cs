using JobCv.Mobile.Models;
using JobCv.Mobile.Services;


namespace JobCv.Mobile.Pages
{
    public partial class EducationFormPage : ContentPage
    {
        private readonly int _cvId;
        private readonly ApiService _apiService;
        private readonly CvEducationDto? _educationToEdit;

        private readonly bool _isEditMode;

        public EducationFormPage(int cvId, ApiService apiService)
        {
            InitializeComponent();

            _cvId = cvId;
            _apiService = apiService;
            _isEditMode = false;

            StartDatePicker.Date = DateTime.Today;
            EndDatePicker.Date = DateTime.Today;
        }

        public EducationFormPage(int cvId, ApiService apiService, CvEducationDto educationToEdit)
        {
            InitializeComponent();

            _cvId = cvId;
            _apiService = apiService;
            _educationToEdit = educationToEdit;
            _isEditMode = true;

            LoadEducationForEdit();
        }

        private void LoadEducationForEdit()
        {
            if (_educationToEdit == null)
                return;

            PageTitleLabel.Text = "Edit Education";
            SaveButton.Text = "Save changes";

            InstitutionEntry.Text = _educationToEdit.Institution;
            DegreeEntry.Text = _educationToEdit.Degree;
            DescriptionEditor.Text = _educationToEdit.Description;

            if (_educationToEdit.StartDate.HasValue)
            {
                UseStartDateCheckBox.IsChecked = true;
                StartDatePicker.IsEnabled = true;
                StartDatePicker.Date = _educationToEdit.StartDate.Value;
            }
            else
            {
                UseStartDateCheckBox.IsChecked = false;
                StartDatePicker.IsEnabled = false;
                StartDatePicker.Date = DateTime.Today;
            }

            if (_educationToEdit.EndDate.HasValue)
            {
                UseEndDateCheckBox.IsChecked = true;
                EndDatePicker.IsEnabled = true;
                EndDatePicker.Date = _educationToEdit.EndDate.Value;
            }
            else
            {
                UseEndDateCheckBox.IsChecked = false;
                EndDatePicker.IsEnabled = false;
                EndDatePicker.Date = DateTime.Today;
            }
        }

        private void OnUseStartDateChanged(object sender, CheckedChangedEventArgs e)
        {
            StartDatePicker.IsEnabled = e.Value;
        }

        private void OnUseEndDateChanged(object sender, CheckedChangedEventArgs e)
        {
            EndDatePicker.IsEnabled = e.Value;
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            MessageLabel.Text = "";

            var institution = InstitutionEntry.Text?.Trim() ?? "";
            var degree = DegreeEntry.Text?.Trim() ?? "";
            var description = DescriptionEditor.Text?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(institution))
            {
                MessageLabel.TextColor = Colors.Red;
                MessageLabel.Text = "Institution is required.";
                return;
            }

            if (string.IsNullOrWhiteSpace(degree))
            {
                MessageLabel.TextColor = Colors.Red;
                MessageLabel.Text = "Degree is required.";
                return;
            }

            DateTime? startDate = UseStartDateCheckBox.IsChecked
                ? StartDatePicker.Date
                : null;

            DateTime? endDate = UseEndDateCheckBox.IsChecked
                ? EndDatePicker.Date
                : null;

            if (startDate.HasValue && endDate.HasValue && endDate.Value < startDate.Value)
            {
                MessageLabel.TextColor = Colors.Red;
                MessageLabel.Text = "End date cannot be before start date.";
                return;
            }

            if (_isEditMode)
            {
                await UpdateEducationAsync(
                    institution,
                    degree,
                    description,
                    startDate,
                    endDate);

                return;
            }

            await AddEducationAsync(
                institution,
                degree,
                description,
                startDate,
                endDate);
        }

        private async Task AddEducationAsync(
            string institution,
            string degree,
            string description,
            DateTime? startDate,
            DateTime? endDate)
        {
            try
            {
                var result = await _apiService.AddEducationAsync(_cvId, new AddEducationRequest
                {
                    Institution = institution,
                    Degree = degree,
                    Description = description,
                    StartDate = startDate,
                    EndDate = endDate
                });

                if (result == null)
                {
                    MessageLabel.TextColor = Colors.Red;
                    MessageLabel.Text = "Education could not be saved.";
                    return;
                }

                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async Task UpdateEducationAsync(
            string institution,
            string degree,
            string description,
            DateTime? startDate,
            DateTime? endDate)
        {
            if (_educationToEdit == null)
                return;

            try
            {
                var updated = await _apiService.UpdateEducationAsync(_educationToEdit.Id, new UpdateEducationRequest
                {
                    Institution = institution,
                    Degree = degree,
                    Description = description,
                    StartDate = startDate,
                    EndDate = endDate
                });

                if (!updated)
                {
                    MessageLabel.TextColor = Colors.Red;
                    MessageLabel.Text = "Education could not be updated.";
                    return;
                }

                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }
}