using JobCv.Mobile.Models;
using JobCv.Mobile.Services;
using Microsoft.Maui.Storage;

namespace JobCv.Mobile.Pages
{
    public partial class ImportCvPage : ContentPage
    {
        private readonly UserDto _user;
        private readonly ApiService _apiService;

        private FileResult? _selectedFile;
        private bool _isImporting;

        public ImportCvPage(UserDto user, ApiService apiService)
        {
            InitializeComponent();

            _user = user;
            _apiService = apiService;
        }

        private static string CurrentLanguage =>
            Preferences.Get("AppLanguage", "en").Equals("ro", StringComparison.OrdinalIgnoreCase)
                ? "ro"
                : "en";

        private static string T(string text)
        {
            return UiTranslationService.TranslateText(text);
        }

        private static string DefaultImportedCvTitle =>
            CurrentLanguage == "ro" ? "CV importat" : "Imported CV";

        private static bool IsDefaultTitle(string? title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return true;

            var value = title.Trim();

            return value.Equals("Imported CV", StringComparison.OrdinalIgnoreCase)
                || value.Equals("CV importat", StringComparison.OrdinalIgnoreCase)
                || value.Equals("CV-nou", StringComparison.OrdinalIgnoreCase)
                || value.Equals("New CV", StringComparison.OrdinalIgnoreCase);
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnChooseFileClicked(object sender, EventArgs e)
        {
            if (_isImporting)
                return;

            try
            {
                var result = await FilePicker.Default.PickAsync(new PickOptions
                {
                    PickerTitle = T("Choose your PDF CV")
                });

                if (result == null)
                    return;

                if (!IsPdfFile(result))
                {
                    MessageLabel.TextColor = Colors.Red;
                    MessageLabel.Text = T("Please choose a PDF file.");
                    return;
                }

                _selectedFile = result;
                SelectedFileLabel.Text = result.FileName;
                MessageLabel.Text = string.Empty;

                if (IsDefaultTitle(TitleEntry.Text))
                {
                    var fileTitle = Path.GetFileNameWithoutExtension(result.FileName);
                    TitleEntry.Text = string.IsNullOrWhiteSpace(fileTitle)
                        ? DefaultImportedCvTitle
                        : fileTitle;
                }
            }
            catch
            {
                MessageLabel.TextColor = Colors.Red;
                MessageLabel.Text = T("An error occurred while choosing the PDF file.");
            }
        }

        private async void OnExtractClicked(object sender, EventArgs e)
        {
            if (_isImporting)
                return;

            MessageLabel.Text = string.Empty;

            if (_selectedFile == null)
            {
                MessageLabel.TextColor = Colors.Red;
                MessageLabel.Text = T("Please choose a PDF file first.");
                return;
            }

            if (!IsPdfFile(_selectedFile))
            {
                MessageLabel.TextColor = Colors.Red;
                MessageLabel.Text = T("Please choose a PDF file.");
                return;
            }

            _isImporting = true;
            ImportActivityIndicator.IsVisible = true;
            ImportActivityIndicator.IsRunning = true;
            MessageLabel.TextColor = Color.FromArgb("#64748B");
            MessageLabel.Text = T("Uploading and extracting information...");

            try
            {
                var uploadedFile = await _apiService.UploadCvFileAsync(_user.Id, _selectedFile);

                if (uploadedFile == null)
                {
                    MessageLabel.TextColor = Colors.Red;
                    MessageLabel.Text = T("The PDF could not be uploaded.");
                    return;
                }

                var title = string.IsNullOrWhiteSpace(TitleEntry.Text)
                    ? Path.GetFileNameWithoutExtension(_selectedFile.FileName)
                    : TitleEntry.Text.Trim();

                if (string.IsNullOrWhiteSpace(title))
                    title = DefaultImportedCvTitle;

                var importedCv = await _apiService.ImportUploadedCvToEditableCvAsync(
                    _user.Id,
                    uploadedFile.Id,
                    new ImportUploadedCvRequest
                    {
                        Title = title,
                        Language = CurrentLanguage,
                        TemplateName = "modern-blue",
                        UseAi = false
                    });

                if (importedCv == null)
                {
                    MessageLabel.TextColor = Colors.Red;
                    MessageLabel.Text = T("The CV could not be created from this PDF.");
                    return;
                }

                await DisplayAlert(
                    T("CV created"),
                    T("The information found in your PDF was copied into a new editable CV. Please review and complete any missing fields."),
                    T("OK"));

                await Navigation.PushAsync(new EditCvPage(importedCv.Id, _apiService));
            }
            catch
            {
                MessageLabel.TextColor = Colors.Red;
                MessageLabel.Text = T("An error occurred while importing the CV.");
            }
            finally
            {
                _isImporting = false;
                ImportActivityIndicator.IsVisible = false;
                ImportActivityIndicator.IsRunning = false;
            }
        }

        private static bool IsPdfFile(FileResult file)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var contentType = file.ContentType ?? string.Empty;

            return extension == ".pdf" || contentType.Contains("pdf", StringComparison.OrdinalIgnoreCase);
        }
    }
}