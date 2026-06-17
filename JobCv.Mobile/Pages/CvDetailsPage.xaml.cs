using JobCv.Mobile.Models;
using JobCv.Mobile.Services;

namespace JobCv.Mobile.Pages
{
    public partial class CvDetailsPage : ContentPage
    {
        private readonly int _cvId;
        private readonly ApiService _apiService;
        private CvDto? _cv;

        public CvDetailsPage(int cvId, ApiService apiService)
        {
            InitializeComponent();

            _cvId = cvId;
            _apiService = apiService;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadCvAsync();
        }

        private T GetControl<T>(string name) where T : Element
        {
            var control = this.FindByName<T>(name);

            if (control == null)
            {
                throw new Exception($"Control '{name}' was not found in CvDetailsPage.xaml.");
            }

            return control;
        }

        private async Task LoadCvAsync()
        {
            _cv = await _apiService.GetCvByIdAsync(_cvId);

            if (_cv == null)
            {
                await DisplayAlert("Error", "The CV could not be loaded.", "OK");
                await Navigation.PopAsync();
                return;
            }

            var titleLabel = GetControl<Label>("TitleLabel");
            var subtitleLabel = GetControl<Label>("SubtitleLabel");
            var fullNameLabel = GetControl<Label>("FullNameLabel");
            var contactLabel = GetControl<Label>("ContactLabel");
            var summaryLabel = GetControl<Label>("SummaryLabel");
            var languageLabel = GetControl<Label>("LanguageLabel");
            var templateLabel = GetControl<Label>("TemplateLabel");
            var linkedInLabel = GetControl<Label>("LinkedInLabel");
            var gitHubLabel = GetControl<Label>("GitHubLabel");
            var portfolioLabel = GetControl<Label>("PortfolioLabel");
            var linksSection = GetControl<VerticalStackLayout>("LinksSection");
            var baseCvBadge = GetControl<Border>("BaseCvBadge");

            titleLabel.Text = _cv.Title;

            subtitleLabel.Text = _cv.IsBaseCv
                ? "This is your base CV."
                : "Preview and manage this CV version.";

            fullNameLabel.Text = string.IsNullOrWhiteSpace(_cv.FullName)
                ? "No name added"
                : _cv.FullName;

            var contactParts = new List<string>();

            if (!string.IsNullOrWhiteSpace(_cv.Email))
                contactParts.Add(_cv.Email);

            if (!string.IsNullOrWhiteSpace(_cv.Phone))
                contactParts.Add(_cv.Phone);

            if (!string.IsNullOrWhiteSpace(_cv.Location))
                contactParts.Add(_cv.Location);

            contactLabel.Text = contactParts.Count == 0
                ? "No contact information added"
                : string.Join(" • ", contactParts);

            summaryLabel.Text = string.IsNullOrWhiteSpace(_cv.Summary)
                ? "No summary added yet."
                : _cv.Summary;

            languageLabel.Text = GetLanguageDisplay(_cv.Language);

            templateLabel.Text = string.IsNullOrWhiteSpace(_cv.TemplateName)
                ? "modern-blue"
                : _cv.TemplateName;

            linkedInLabel.Text = string.IsNullOrWhiteSpace(_cv.LinkedInUrl)
                ? ""
                : $"LinkedIn: {_cv.LinkedInUrl}";

            gitHubLabel.Text = string.IsNullOrWhiteSpace(_cv.GitHubUrl)
                ? ""
                : $"GitHub: {_cv.GitHubUrl}";

            portfolioLabel.Text = string.IsNullOrWhiteSpace(_cv.PortfolioUrl)
                ? ""
                : $"Portfolio: {_cv.PortfolioUrl}";

            var hasLinks =
                !string.IsNullOrWhiteSpace(_cv.LinkedInUrl) ||
                !string.IsNullOrWhiteSpace(_cv.GitHubUrl) ||
                !string.IsNullOrWhiteSpace(_cv.PortfolioUrl);

            linksSection.IsVisible = hasLinks;
            baseCvBadge.IsVisible = _cv.IsBaseCv;
        }

        private static string GetLanguageDisplay(string language)
        {
            return language switch
            {
                "en" => "English",
                "ro" => "Romanian",
                _ => string.IsNullOrWhiteSpace(language) ? "-" : language
            };
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnEditClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new EditCvPage(_cvId, _apiService));
        }

        private async void OnExportPdfClicked(object sender, EventArgs e)
        {
            try
            {
                var downloadUrl = _apiService.GetCvPdfDownloadUrl(_cvId);
                await Launcher.OpenAsync(downloadUrl);
            }
            catch
            {
                await DisplayAlert(
                    "Error",
                    "The PDF could not be opened.",
                    "OK");
            }
        }
    }
}