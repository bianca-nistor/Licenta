using JobCv.Mobile.Models;
using JobCv.Mobile.Pages;
using JobCv.Mobile.Services;
using System.Net.Mail;

namespace JobCv.Mobile
{
    public partial class MainPage : ContentPage
    {
        private readonly ApiService _apiService = new();

        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            MessageLabel.Text = "";

            if (string.IsNullOrWhiteSpace(EmailEntry.Text) ||
                string.IsNullOrWhiteSpace(PasswordEntry.Text))
            {
                MessageLabel.TextColor = Colors.Red;
                MessageLabel.Text = "Email and password are required.";
                return;
            }

            var email = EmailEntry.Text.Trim();

            if (!IsValidEmail(email))
            {
                MessageLabel.TextColor = Colors.Red;
                MessageLabel.Text = "Please enter a valid email address.";
                return;
            }

            var request = new LoginRequest
            {
                Email = email,
                Password = PasswordEntry.Text
            };

            var user = await _apiService.LoginAsync(request);

            if (user == null)
            {
                MessageLabel.TextColor = Colors.Red;
                MessageLabel.Text = "Invalid email or password.";
                return;
            }

            await Navigation.PushAsync(new CvsPage(user, _apiService));
        }

        private async void OnGoToRegisterClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RegisterPage(_apiService));
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
    }
}