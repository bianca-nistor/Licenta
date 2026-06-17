using JobCv.Mobile.Models;
using JobCv.Mobile.Services;
using System.Net.Mail;

namespace JobCv.Mobile.Pages
{
    public partial class RegisterPage : ContentPage
    {
        private readonly ApiService _apiService;
        private bool _isRegistering;

        public RegisterPage(ApiService apiService)
        {
            InitializeComponent();

            _apiService = apiService;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var mailAddress = new MailAddress(email);
                return mailAddress.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private T GetControl<T>(string name) where T : Element
        {
            var control = this.FindByName<T>(name);

            if (control == null)
                throw new Exception($"Control '{name}' was not found in RegisterPage.xaml.");

            return control;
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            if (_isRegistering)
                return;

            var messageLabel = GetControl<Label>("MessageLabel");
            var fullNameEntry = GetControl<Entry>("FullNameEntry");
            var emailEntry = GetControl<Entry>("EmailEntry");
            var passwordEntry = GetControl<Entry>("PasswordEntry");
            var confirmPasswordEntry = GetControl<Entry>("ConfirmPasswordEntry");

            messageLabel.Text = "";
            messageLabel.TextColor = Colors.Red;

            var fullName = fullNameEntry.Text?.Trim() ?? string.Empty;
            var email = emailEntry.Text?.Trim() ?? string.Empty;
            var password = passwordEntry.Text ?? string.Empty;
            var confirmPassword = confirmPasswordEntry.Text ?? string.Empty;

            if (string.IsNullOrWhiteSpace(fullName) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(confirmPassword))
            {
                messageLabel.Text = "All fields are required.";
                return;
            }

            if (!IsValidEmail(email))
            {
                messageLabel.Text = "Please enter a valid email address.";
                return;
            }

            if (password.Length < 6)
            {
                messageLabel.Text = "Password must have at least 6 characters.";
                return;
            }

            if (password != confirmPassword)
            {
                messageLabel.Text = "Password and confirmation do not match.";
                return;
            }

            _isRegistering = true;

            try
            {
                var request = new RegisterRequest
                {
                    FullName = fullName,
                    Email = email,
                    Password = password
                };

                var user = await _apiService.RegisterAsync(request);

                if (user == null)
                {
                    messageLabel.TextColor = Colors.Red;
                    messageLabel.Text = "Account could not be created.";
                    return;
                }

                await DisplayAlert(
                    "Success",
                    "Account created successfully. You can now sign in.",
                    "OK");

                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                messageLabel.TextColor = Colors.Red;
                messageLabel.Text = ex.Message;
            }
            finally
            {
                _isRegistering = false;
            }
        }

        private async void OnBackToLoginClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}