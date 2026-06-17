using JobCv.Mobile.Models;
using JobCv.Mobile.Services;


namespace JobCv.Mobile.Pages
{
    public partial class EditProfilePage : ContentPage
    {
        private readonly UserDto _user;
        private readonly ApiService _apiService;
        private bool _isSaving;

        public EditProfilePage(UserDto user, ApiService apiService)
        {
            InitializeComponent();

            _user = user;
            _apiService = apiService;

            LoadForm();
        }

        private void LoadForm()
        {
            FullNameEntry.Text = _user.FullName;
            PhoneNumberEntry.Text = _user.PhoneNumber;
            AlternativeEmailEntry.Text = _user.AlternativeEmail;
            LocationEntry.Text = _user.Location;

            SetPickerValue(CareerLevelPicker, _user.CareerLevel);
            SetPickerValue(PreferredJobTypePicker, _user.PreferredJobType);
        }

        private static void SetPickerValue(Picker picker, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            for (var i = 0; i < picker.Items.Count; i++)
            {
                if (string.Equals(picker.Items[i], value, StringComparison.OrdinalIgnoreCase))
                {
                    picker.SelectedIndex = i;
                    return;
                }
            }
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            if (_isSaving)
                return;

            _isSaving = true;
            MessageLabel.Text = "";

            try
            {
                if (string.IsNullOrWhiteSpace(FullNameEntry.Text))
                {
                    MessageLabel.Text = "Full name is required.";
                    return;
                }

                var request = new UpdateUserProfileRequest
                {
                    FullName = FullNameEntry.Text.Trim(),
                    PhoneNumber = PhoneNumberEntry.Text?.Trim() ?? string.Empty,
                    AlternativeEmail = AlternativeEmailEntry.Text?.Trim() ?? string.Empty,
                    Location = LocationEntry.Text?.Trim() ?? string.Empty,
                    CareerLevel = CareerLevelPicker.SelectedItem?.ToString() ?? string.Empty,
                    PreferredJobType = PreferredJobTypePicker.SelectedItem?.ToString() ?? string.Empty
                };

                var updatedUser = await _apiService.UpdateUserProfileAsync(_user.Id, request);

                if (updatedUser == null)
                {
                    MessageLabel.Text = "Profile could not be updated.";
                    return;
                }

                _user.FullName = updatedUser.FullName;
                _user.PhoneNumber = updatedUser.PhoneNumber;
                _user.AlternativeEmail = updatedUser.AlternativeEmail;
                _user.Location = updatedUser.Location;
                _user.CareerLevel = updatedUser.CareerLevel;
                _user.PreferredJobType = updatedUser.PreferredJobType;

                await DisplayAlert("Profile saved", "Your profile details were saved successfully.", "OK");
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                MessageLabel.Text = ex.Message;
            }
            finally
            {
                _isSaving = false;
            }
        }
    }
}