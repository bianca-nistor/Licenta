using JobCv.Mobile.Models;
using JobCv.Mobile.Services;
using System.Net.Mail;

namespace JobCv.Mobile.Pages
{
    public partial class RegisterPage : ContentPage
    {
        private readonly ApiService _apiService;

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

        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            MessageLabel.Text = "";

            if (string.IsNullOrWhiteSpace(FullNameEntry.Text) ||
    string.IsNullOrWhiteSpace(EmailEntry.Text) ||
    string.IsNullOrWhiteSpace(PasswordEntry.Text))
            {
                MessageLabel.TextColor = Colors.Red;
                MessageLabel.Text = "All fields are required.";
                return;
            }

            if (!IsValidEmail(EmailEntry.Text))
            {
                MessageLabel.TextColor = Colors.Red;
                MessageLabel.Text = "Please enter a valid email address.";
                return;
            }

            if (PasswordEntry.Text.Length < 6)
            {
                MessageLabel.TextColor = Colors.Red;
                MessageLabel.Text = "Password must have at least 6 characters.";
                return;
            }

            var request = new RegisterRequest
            {
                FullName = FullNameEntry.Text ?? "",
                Email = EmailEntry.Text ?? "",
                Password = PasswordEntry.Text ?? ""
            };

            var user = await _apiService.RegisterAsync(request);

            if (user == null)
            {
                MessageLabel.TextColor = Colors.Red;
                MessageLabel.Text = "Account could not be created.";
                return;
            }

            await DisplayAlert("Success", "Account created successfully. You can now sign in.", "OK");
            await Navigation.PopAsync();
        }

        private async void OnBackToLoginClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}