using JobCv.Mobile.Models;
using JobCv.Mobile.Services;

namespace JobCv.Mobile.Pages
{
    public partial class CvsPage : ContentPage
    {
        private readonly UserDto _user;
        private readonly ApiService _apiService;

        public CvsPage(UserDto user, ApiService apiService)
        {
            InitializeComponent();

            _user = user;
            _apiService = apiService;

            WelcomeLabel.Text = $"Bun venit, {_user.FullName}";
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadCvsAsync();
        }

        private async Task LoadCvsAsync()
        {
            var cvs = await _apiService.GetUserCvsAsync(_user.Id);
            CvsCollectionView.ItemsSource = cvs;
        }

        private async void OnCreateCvClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CreateCvPage(_user, _apiService));
        }

        private async void OnDeleteCvClicked(object sender, EventArgs e)
        {
            if (sender is not Button button)
                return;

            if (button.CommandParameter is not int cvId)
                return;

            var confirm = await DisplayAlert(
                "Confirmare",
                "Sigur vrei să ștergi acest CV?",
                "Da",
                "Nu");

            if (!confirm)
                return;

            var deleted = await _apiService.DeleteCvAsync(cvId);

            if (!deleted)
            {
                await DisplayAlert("Eroare", "CV-ul nu a putut fi șters.", "OK");
                return;
            }

            await LoadCvsAsync();
        }
    }
}