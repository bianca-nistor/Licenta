using JobCv.Mobile.Models;
using JobCv.Mobile.Services;

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

        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            MessageLabel.Text = "";

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
                MessageLabel.Text = "Nu s-a putut crea contul.";
                return;
            }

            await DisplayAlert("Succes", "Cont creat cu succes. Te poți autentifica.", "OK");
            await Navigation.PopAsync();
        }
    }
}