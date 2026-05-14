using JobCv.Mobile.Models;
using JobCv.Mobile.Services;

namespace JobCv.Mobile.Pages
{
    public partial class CreateCvPage : ContentPage
    {
        private readonly UserDto _user;
        private readonly ApiService _apiService;

        public CreateCvPage(UserDto user, ApiService apiService)
        {
            InitializeComponent();

            _user = user;
            _apiService = apiService;

            LanguagePicker.SelectedIndex = 0;
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            MessageLabel.Text = "";

            var request = new CreateCvRequest
            {
                UserId = _user.Id,
                Title = TitleEntry.Text ?? "",
                Language = LanguagePicker.SelectedItem?.ToString() ?? "ro",
                Summary = SummaryEditor.Text ?? ""
            };

            var cv = await _apiService.CreateCvAsync(request);

            if (cv == null)
            {
                MessageLabel.TextColor = Colors.Red;
                MessageLabel.Text = "CV-ul nu a putut fi creat.";
                return;
            }

            await DisplayAlert("Succes", "CV creat cu succes.", "OK");
            await Navigation.PopAsync();
        }
    }
}