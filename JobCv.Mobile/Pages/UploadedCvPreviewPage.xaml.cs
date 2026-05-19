using Microsoft.Maui.ApplicationModel;

namespace JobCv.Mobile.Pages
{
    public partial class UploadedCvPreviewPage : ContentPage
    {
        private readonly string _title;
        private readonly string _previewUrl;
        private readonly string _downloadUrl;

        public UploadedCvPreviewPage(string title, string previewUrl, string downloadUrl)
        {
            InitializeComponent();

            _title = title;
            _previewUrl = previewUrl;
            _downloadUrl = downloadUrl;

            TitleLabel.Text = _title;
            PdfWebView.Source = _previewUrl;
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnOpenExternalClicked(object sender, EventArgs e)
        {
            try
            {
                await Launcher.OpenAsync(_downloadUrl);
            }
            catch
            {
                await DisplayAlert("Error", "The PDF could not be opened.", "OK");
            }
        }
    }
}