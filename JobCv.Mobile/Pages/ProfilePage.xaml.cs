using JobCv.Mobile.Models;
using JobCv.Mobile.Services;

namespace JobCv.Mobile.Pages
{
    public partial class ProfilePage : ContentPage
    {
        private UserDto _user;
        private readonly ApiService _apiService;

        public ProfilePage(UserDto user, ApiService apiService)
        {
            InitializeComponent();

            _user = user;
            _apiService = apiService;

            LoadProfile();
            UpdateLanguageButtons();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                var updatedUser = await _apiService.GetUserByIdAsync(_user.Id);

                if (updatedUser != null)
                {
                    _user = updatedUser;
                    LoadProfile();
                    UpdateLanguageButtons();
                }
            }
            catch
            {
                LoadProfile();
                UpdateLanguageButtons();
            }
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

            CareerLevelLabel.Text = string.IsNullOrWhiteSpace(_user.CareerLevel)
                ? "Not set"
                : _user.CareerLevel;

            PreferredJobTypeLabel.Text = string.IsNullOrWhiteSpace(_user.PreferredJobType)
                ? "Not set"
                : _user.PreferredJobType;

            PhoneNumberLabel.Text = string.IsNullOrWhiteSpace(_user.PhoneNumber)
     ? "Not provided"
     : _user.PhoneNumber;

            AlternativeEmailLabel.Text = string.IsNullOrWhiteSpace(_user.AlternativeEmail)
                ? "Not provided"
                : _user.AlternativeEmail;

            LocationLabel.Text = string.IsNullOrWhiteSpace(_user.Location)
                ? "Not provided"
                : _user.Location;
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

        private async void OnEditProfileClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new EditProfilePage(_user, _apiService));
        }

        private async void OnChangePasswordClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ChangePasswordPage(_user, _apiService));
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

        private void OnEnglishLanguageClicked(object sender, EventArgs e)
        {
            LanguageService.SetLanguage("en");
            UpdateLanguageButtons();
            UiTranslationService.ApplyToPage(this);
        }

        private void OnRomanianLanguageClicked(object sender, EventArgs e)
        {
            LanguageService.SetLanguage("ro");
            UpdateLanguageButtons();
            UiTranslationService.ApplyToPage(this);
        }

        private void UpdateLanguageButtons()
        {
            var isRomanian = LanguageService.IsRomanian;

            EnglishLanguageButton.BackgroundColor = isRomanian ? Color.FromArgb("#EFF6FF") : Color.FromArgb("#1D4ED8");
            EnglishLanguageButton.TextColor = isRomanian ? Color.FromArgb("#1D4ED8") : Colors.White;

            RomanianLanguageButton.BackgroundColor = isRomanian ? Color.FromArgb("#1D4ED8") : Color.FromArgb("#EFF6FF");
            RomanianLanguageButton.TextColor = isRomanian ? Colors.White : Color.FromArgb("#1D4ED8");

            LanguageStatusLabel.Text = isRomanian
                ? "Current language: Romanian"
                : "Current language: English";
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            var confirm = await DisplayAlert(
                UiTranslationService.TranslateText("Logout"),
                UiTranslationService.TranslateText("Are you sure you want to log out?"),
                UiTranslationService.TranslateText("Yes"),
                UiTranslationService.TranslateText("No"));

            if (!confirm)
                return;

            Application.Current!.Windows[0].Page = new LocalizedNavigationPage(new MainPage());
        }
    }
}