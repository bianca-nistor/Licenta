using JobCv.Mobile.Models;
using JobCv.Mobile.Services;

namespace JobCv.Mobile.Pages
{
    public partial class ProfilePage : ContentPage
    {
        private readonly UserDto _user;
        private readonly ApiService _apiService;

        public ProfilePage(UserDto user, ApiService apiService)
        {
            InitializeComponent();

            _user = user;
            _apiService = apiService;

            LoadProfile();
        }

        private void LoadProfile()
        {
            var displayName = string.IsNullOrWhiteSpace(_user.FullName)
                ? "JobCv user"
                : _user.FullName.Trim();

            var email = string.IsNullOrWhiteSpace(_user.Email)
                ? "Email not available"
                : _user.Email.Trim();

            FullNameLabel.Text = displayName;
            EmailLabel.Text = email;
            InitialsLabel.Text = GetInitials(displayName, email);
        }

        private static string GetInitials(string displayName, string email)
        {
            var source = !string.IsNullOrWhiteSpace(displayName) && displayName != "JobCv user"
                ? displayName
                : email;

            if (string.IsNullOrWhiteSpace(source))
                return "U";

            var parts = source
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(part => !string.IsNullOrWhiteSpace(part))
                .ToList();

            if (parts.Count >= 2)
            {
                return $"{parts[0][0]}{parts[1][0]}".ToUpperInvariant();
            }

            if (parts.Count == 1)
            {
                return parts[0][0].ToString().ToUpperInvariant();
            }

            return "U";
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnDashboardClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CvsPage(_user, _apiService));
        }

        private async void OnMyCvsClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MyCvsPage(_user, _apiService));
        }

        private async void OnApplicationsClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ApplicationsPage(_user, _apiService));
        }

        private async void OnJobsClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new JobsPage(_user, _apiService));
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            var confirm = await DisplayAlert(
                "Logout",
                "Are you sure you want to log out?",
                "Yes",
                "No");

            if (!confirm)
                return;

            Application.Current!.Windows[0].Page = new NavigationPage(new MainPage());
        }
    }
}