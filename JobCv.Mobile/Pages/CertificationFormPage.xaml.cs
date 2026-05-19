using JobCv.Mobile.Models;
using JobCv.Mobile.Services;

namespace JobCv.Mobile.Pages
{
    public partial class CertificationFormPage : ContentPage
    {
        private readonly int _cvId;
        private readonly ApiService _apiService;
        private readonly CvCertificationDto? _certificationToEdit;
        private readonly bool _isEditMode;

        public CertificationFormPage(int cvId, ApiService apiService)
        {
            InitializeComponent();

            _cvId = cvId;
            _apiService = apiService;
            _isEditMode = false;

            CertificationDatePicker.Date = DateTime.Today;
        }

        public CertificationFormPage(int cvId, ApiService apiService, CvCertificationDto certificationToEdit)
        {
            InitializeComponent();

            _cvId = cvId;
            _apiService = apiService;
            _certificationToEdit = certificationToEdit;
            _isEditMode = true;

            LoadCertificationForEdit();
        }

        private void LoadCertificationForEdit()
        {
            if (_certificationToEdit == null)
                return;

            PageTitleLabel.Text = "Edit Certification";
            SaveButton.Text = "Save changes";

            NameEntry.Text = _certificationToEdit.Name;
            IssuerEntry.Text = _certificationToEdit.Issuer;
            UrlEntry.Text = _certificationToEdit.Url;

            if (_certificationToEdit.Date.HasValue)
            {
                UseDateCheckBox.IsChecked = true;
                CertificationDatePicker.IsEnabled = true;
                CertificationDatePicker.Date = _certificationToEdit.Date.Value;
            }
            else
            {
                UseDateCheckBox.IsChecked = false;
                CertificationDatePicker.IsEnabled = false;
                CertificationDatePicker.Date = DateTime.Today;
            }
        }

        private void OnUseDateChanged(object sender, CheckedChangedEventArgs e)
        {
            CertificationDatePicker.IsEnabled = e.Value;
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            MessageLabel.Text = "";

            var name = NameEntry.Text?.Trim() ?? "";
            var issuer = IssuerEntry.Text?.Trim() ?? "";
            var url = UrlEntry.Text?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(name))
            {
                MessageLabel.TextColor = Colors.Red;
                MessageLabel.Text = "Certification name is required.";
                return;
            }

            DateTime? date = UseDateCheckBox.IsChecked
                ? CertificationDatePicker.Date
                : null;

            if (_isEditMode)
            {
                await UpdateCertificationAsync(name, issuer, date, url);
                return;
            }

            await AddCertificationAsync(name, issuer, date, url);
        }

        private async Task AddCertificationAsync(
            string name,
            string issuer,
            DateTime? date,
            string url)
        {
            try
            {
                var result = await _apiService.AddCertificationAsync(_cvId, new AddCertificationRequest
                {
                    Name = name,
                    Issuer = issuer,
                    Date = date,
                    Url = url
                });

                if (result == null)
                {
                    MessageLabel.TextColor = Colors.Red;
                    MessageLabel.Text = "Certification could not be saved.";
                    return;
                }

                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async Task UpdateCertificationAsync(
            string name,
            string issuer,
            DateTime? date,
            string url)
        {
            if (_certificationToEdit == null)
                return;

            try
            {
                var updated = await _apiService.UpdateCertificationAsync(_certificationToEdit.Id, new UpdateCertificationRequest
                {
                    Name = name,
                    Issuer = issuer,
                    Date = date,
                    Url = url
                });

                if (!updated)
                {
                    MessageLabel.TextColor = Colors.Red;
                    MessageLabel.Text = "Certification could not be updated.";
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