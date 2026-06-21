using JobCv.Mobile.Models;
using JobCv.Mobile.Services;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;

namespace JobCv.Mobile.Pages
{
    public partial class UploadCvPage : ContentPage
    {
        private readonly UserDto _user;
        private readonly ApiService _apiService;

        private FileResult? _selectedFile;

        public UploadCvPage(UserDto user, ApiService apiService)
        {
            InitializeComponent();

            _user = user;
            _apiService = apiService;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadUploadedFilesAsync();
        }

        private T GetControl<T>(string name) where T : Element
        {
            var control = this.FindByName<T>(name);

            if (control == null)
                throw new Exception($"Control '{name}' was not found in UploadCvPage.xaml.");

            return control;
        }

        private async Task LoadUploadedFilesAsync()
        {
            var files = await _apiService.GetUploadedCvFilesAsync(_user.Id);

            GetControl<CollectionView>("UploadedFilesCollectionView").ItemsSource = files;
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnChooseFileClicked(object sender, EventArgs e)
        {
            var customFileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.WinUI, new[] { ".pdf", ".doc", ".docx" } },
                { DevicePlatform.Android, new[] {
                    "application/pdf",
                    "application/msword",
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
                } }
            });

            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Choose your CV file",
                FileTypes = customFileType
            });

            if (result == null)
                return;

            _selectedFile = result;

            GetControl<Label>("SelectedFileLabel").Text = result.FileName;
            GetControl<Label>("MessageLabel").Text = "";
        }

        private async void OnUploadClicked(object sender, EventArgs e)
        {
            var messageLabel = GetControl<Label>("MessageLabel");

            messageLabel.Text = "";

            if (_selectedFile == null)
            {
                messageLabel.TextColor = Colors.Red;
                messageLabel.Text = "Please choose a file first.";
                return;
            }

            var extension = Path.GetExtension(_selectedFile.FileName).ToLowerInvariant();

            if (extension != ".pdf" && extension != ".doc" && extension != ".docx")
            {
                messageLabel.TextColor = Colors.Red;
                messageLabel.Text = "Only PDF, DOC and DOCX files are allowed.";
                return;
            }

            var uploadedFile = await _apiService.UploadCvFileAsync(_user.Id, _selectedFile);

            if (uploadedFile == null)
            {
                messageLabel.TextColor = Colors.Red;
                messageLabel.Text = "The file could not be uploaded.";
                return;
            }

            await UiTranslationService.DisplayAlertAsync(this, "Success", "File uploaded successfully.");
            await Navigation.PopAsync();
        }

        private async void OnOpenUploadedFileClicked(object sender, EventArgs e)
        {
            if ((sender as Button)?.CommandParameter is not int fileId)
                return;

            var url = _apiService.GetUploadedCvDownloadUrl(fileId);

            try
            {
                await Launcher.OpenAsync(url);
            }
            catch
            {
                await UiTranslationService.DisplayAlertAsync(this, "Error", "The file could not be opened.");
            }
        }

        private async void OnDeleteUploadedFileClicked(object sender, EventArgs e)
        {
            if ((sender as Button)?.CommandParameter is not int fileId)
                return;

            var confirm = await UiTranslationService.DisplayConfirmAsync(
                this,
                "Delete file",
                "Are you sure you want to delete this uploaded CV?");

            if (!confirm)
                return;

            var deleted = await _apiService.DeleteUploadedCvFileAsync(fileId);

            if (!deleted)
            {
                await UiTranslationService.DisplayAlertAsync(this, "Error", "The file could not be deleted.");
                return;
            }

            await LoadUploadedFilesAsync();
        }
    }
}