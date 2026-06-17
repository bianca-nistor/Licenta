using JobCv.Mobile.Models;
using JobCv.Mobile.Services;


namespace JobCv.Mobile.Pages
{
    public partial class ChangePasswordPage : ContentPage
    {
        private readonly UserDto _user;
        private readonly ApiService _apiService;
        private bool _isSaving;

        public ChangePasswordPage(UserDto user, ApiService apiService)
        {
            InitializeComponent();

            _user = user;
            _apiService = apiService;
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
            MessageLabel.TextColor = Colors.Red;

            try
            {
                var currentPassword = CurrentPasswordEntry.Text?.Trim() ?? string.Empty;
                var newPassword = NewPasswordEntry.Text?.Trim() ?? string.Empty;
                var confirmNewPassword = ConfirmNewPasswordEntry.Text?.Trim() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(currentPassword) ||
                    string.IsNullOrWhiteSpace(newPassword) ||
                    string.IsNullOrWhiteSpace(confirmNewPassword))
                {
                    MessageLabel.Text = "All password fields are required.";
                    return;
                }

                if (newPassword.Length < 6)
                {
                    MessageLabel.Text = "New password must have at least 6 characters.";
                    return;
                }

                if (newPassword != confirmNewPassword)
                {
                    MessageLabel.Text = "New password and confirmation do not match.";
                    return;
                }

                if (currentPassword == newPassword)
                {
                    MessageLabel.Text = "New password must be different from the current password.";
                    return;
                }

                var request = new ChangePasswordRequest
                {
                    CurrentPassword = currentPassword,
                    NewPassword = newPassword,
                    ConfirmNewPassword = confirmNewPassword
                };

                var changed = await _apiService.ChangePasswordAsync(_user.Id, request);

                if (!changed)
                {
                    MessageLabel.Text = "Password could not be changed.";
                    return;
                }

                CurrentPasswordEntry.Text = string.Empty;
                NewPasswordEntry.Text = string.Empty;
                ConfirmNewPasswordEntry.Text = string.Empty;

                await DisplayAlert("Password changed", "Your password was changed successfully.", "OK");
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