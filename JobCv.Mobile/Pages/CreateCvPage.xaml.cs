using JobCv.Mobile.Models;
using JobCv.Mobile.Services;

namespace JobCv.Mobile.Pages
{
    public partial class CreateCvPage : ContentPage
    {
        private readonly UserDto _user;
        private readonly ApiService _apiService;
        private bool _isCreating;

        public CreateCvPage(UserDto user, ApiService apiService)
        {
            InitializeComponent();

            _user = user;
            _apiService = apiService;
        }

        private static string T(string value)
        {
            return UiTranslationService.TranslateText(value);
        }

        private async void OnStartFromScratchClicked(object sender, EventArgs e)
        {
            if (_isCreating)
                return;

            _isCreating = true;
            MessageLabel.Text = "";

            try
            {
                var request = new CreateCvRequest
                {
                    UserId = _user.Id,
                    Title = "Untitled CV",
                    Language = LanguageService.CurrentLanguage,
                    Summary = string.Empty,
                    TemplateName = "modern-blue",
                    IsBaseCv = false
                };

                var cv = await _apiService.CreateCvAsync(request);

                if (cv == null)
                {
                    MessageLabel.TextColor = Colors.Red;
                    MessageLabel.Text = T("The CV could not be created.");
                    return;
                }

                await Navigation.PushAsync(new EditCvPage(cv.Id, _apiService));
            }
            catch (Exception ex)
            {
                MessageLabel.TextColor = Colors.Red;
                MessageLabel.Text = ex.Message;
            }
            finally
            {
                _isCreating = false;
            }
        }

        private async void OnDuplicateCvClicked(object sender, EventArgs e)
        {
            MessageLabel.Text = "";

            try
            {
                var cvs = await _apiService.GetUserCvsAsync(_user.Id);

                if (cvs == null || cvs.Count == 0)
                {
                    await UiTranslationService.DisplayAlertAsync(
                        this,
                        "No CVs available",
                        "You need to create a CV before you can copy one.");

                    return;
                }

                var options = cvs
                    .Select(cv => $"{cv.Id} - {cv.Title}")
                    .ToArray();

                var cancelText = T("Cancel");

                var selectedOption = await DisplayActionSheet(
                    T("Choose a CV to copy"),
                    cancelText,
                    null,
                    options);

                if (string.IsNullOrWhiteSpace(selectedOption) || selectedOption == cancelText)
                    return;

                var selectedCv = cvs.FirstOrDefault(cv => selectedOption.StartsWith($"{cv.Id} - "));

                if (selectedCv == null)
                {
                    await UiTranslationService.DisplayAlertAsync(
                        this,
                        "Error",
                        "The selected CV could not be found.");
                    return;
                }

                var copySuffix = LanguageService.IsRomanian ? "copie" : "copy";

                var newTitle = await DisplayPromptAsync(
                    T("Copy CV"),
                    T("Enter a title for the new CV:"),
                    initialValue: $"{selectedCv.Title} {copySuffix}");

                if (string.IsNullOrWhiteSpace(newTitle))
                    return;

                var duplicatedCv = await _apiService.DuplicateCvAsync(selectedCv.Id, new DuplicateCvRequest
                {
                    NewTitle = newTitle.Trim(),
                    TemplateName = string.IsNullOrWhiteSpace(selectedCv.TemplateName)
                        ? "modern-blue"
                        : selectedCv.TemplateName,
                    TargetJobId = null
                });

                if (duplicatedCv == null)
                {
                    await UiTranslationService.DisplayAlertAsync(
                        this,
                        "Error",
                        "The CV could not be copied.");
                    return;
                }

                await Navigation.PushAsync(new EditCvPage(duplicatedCv.Id, _apiService));
            }
            catch (Exception ex)
            {
                MessageLabel.TextColor = Colors.Red;
                MessageLabel.Text = ex.Message;
            }
        }

        private async void OnStartFromYourCvClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ImportCvPage(_user, _apiService));
        }

        private async void OnUploadCvClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new UploadCvPage(_user, _apiService));
        }
    }
}
