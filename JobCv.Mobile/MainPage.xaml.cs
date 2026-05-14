using JobCv.Mobile.Models;
using JobCv.Mobile.Pages;
using JobCv.Mobile.Services;

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

            var request = new LoginRequest
            {
                Email = EmailEntry.Text ?? "",
                Password = PasswordEntry.Text ?? ""
            };

            var user = await _apiService.LoginAsync(request);

            if (user == null)
            {
                MessageLabel.TextColor = Colors.Red;
                MessageLabel.Text = "Email sau parolă incorectă.";
                return;
            }

            await Navigation.PushAsync(new CvsPage(user, _apiService));
        }

        private async void OnGoToRegisterClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RegisterPage(_apiService));
        }
    }
}