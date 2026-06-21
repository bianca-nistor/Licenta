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
                    PickerTitle = "Choose your PDF CV"
                });

                if (result == null)
                    return;

                if (!IsPdfFile(result))
                {
                    MessageLabel.TextColor = Colors.Red;
                    MessageLabel.Text = "Please choose a PDF file.";
                    return;
                }

                _selectedFile = result;
                SelectedFileLabel.Text = result.FileName;
                MessageLabel.Text = string.Empty;

                if (string.IsNullOrWhiteSpace(TitleEntry.Text) || TitleEntry.Text == "Imported CV")
                {
                    var fileTitle = Path.GetFileNameWithoutExtension(result.FileName);
                    TitleEntry.Text = string.IsNullOrWhiteSpace(fileTitle)
                        ? "Imported CV"
                        : fileTitle;
                }
            }
            catch (Exception ex)
            {
                MessageLabel.TextColor = Colors.Red;
                MessageLabel.Text = ex.Message;
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
                MessageLabel.Text = "Please choose a PDF file first.";
                return;
            }

            if (!IsPdfFile(_selectedFile))
            {
                MessageLabel.TextColor = Colors.Red;
                MessageLabel.Text = "Please choose a PDF file.";
                return;
            }

            _isImporting = true;
            ImportActivityIndicator.IsVisible = true;
            ImportActivityIndicator.IsRunning = true;
            MessageLabel.TextColor = Color.FromArgb("#64748B");
            MessageLabel.Text = "Uploading and extracting information...";

            try
            {
                var uploadedFile = await _apiService.UploadCvFileAsync(_user.Id, _selectedFile);

                if (uploadedFile == null)
                {
                    MessageLabel.TextColor = Colors.Red;
                    MessageLabel.Text = "The PDF could not be uploaded.";
                    return;
                }

                var title = string.IsNullOrWhiteSpace(TitleEntry.Text)
                    ? Path.GetFileNameWithoutExtension(_selectedFile.FileName)
                    : TitleEntry.Text.Trim();

                if (string.IsNullOrWhiteSpace(title))
                    title = "Imported CV";

                var importedCv = await _apiService.ImportUploadedCvToEditableCvAsync(
                    _user.Id,
                    uploadedFile.Id,
                    new ImportUploadedCvRequest
                    {
                        Title = title,
                        Language = "en",
                        TemplateName = "modern-blue",
                        UseAi = false
                    });

                if (importedCv == null)
                {
                    MessageLabel.TextColor = Colors.Red;
                    MessageLabel.Text = "The CV could not be created from this PDF.";
                    return;
                }

                await DisplayAlert(
                    "CV created",
                    "The information found in your PDF was copied into a new editable CV. Please review and complete any missing fields.",
                    "OK");

                await Navigation.PushAsync(new EditCvPage(importedCv.Id, _apiService));
            }
            catch (Exception ex)
            {
                MessageLabel.TextColor = Colors.Red;
                MessageLabel.Text = ex.Message;
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
