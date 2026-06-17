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

            _title = string.IsNullOrWhiteSpace(title)
                ? "CV Preview"
                : title.Trim();

            _previewUrl = previewUrl;
            _downloadUrl = downloadUrl;

            TitleLabel.Text = _title;

            LoadPdfPreview();
        }

        private void LoadPdfPreview()
        {
            PreviewLoadingIndicator.IsVisible = true;
            PreviewLoadingIndicator.IsRunning = true;
            PreviewMessageLabel.Text = "Loading PDF preview...";

            /*
             * Android WebView nu afi?eaz? mereu PDF direct.
             * De aceea folosim Google Docs Viewer ca s? rand?m PDF-ul în WebView.
             */
            var encodedPdfUrl = Uri.EscapeDataString(_previewUrl);
            var googleViewerUrl = $"https://docs.google.com/gview?embedded=true&url={encodedPdfUrl}";

            PdfWebView.Source = googleViewerUrl;
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
                await DisplayAlert(
                    "Error",
                    "The PDF could not be opened.",
                    "OK");
            }
        }

        private void OnPdfWebViewNavigating(object sender, WebNavigatingEventArgs e)
        {
            PreviewLoadingIndicator.IsVisible = true;
            PreviewLoadingIndicator.IsRunning = true;
            PreviewMessageLabel.Text = "Loading PDF preview...";
        }

        private void OnPdfWebViewNavigated(object sender, WebNavigatedEventArgs e)
        {
            PreviewLoadingIndicator.IsVisible = false;
            PreviewLoadingIndicator.IsRunning = false;

            if (e.Result == WebNavigationResult.Success)
            {
                PreviewMessageLabel.Text = "PDF preview loaded.";
                return;
            }

            PreviewMessageLabel.Text =
                "The preview could not be loaded inside the app. You can still use Export to open the PDF.";
        }
    }
}