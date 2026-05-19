using JobCv.Mobile.Models;
using JobCv.Mobile.Services;
using Microsoft.Maui.ApplicationModel;

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

            WelcomeLabel.Text = $"Welcome, {_user.FullName}. Manage your CVs here.";
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadDataAsync();
        }

        private T GetControl<T>(string name) where T : Element
        {
            var control = this.FindByName<T>(name);

            if (control == null)
                throw new Exception($"Control '{name}' was not found in CvsPage.xaml.");

            return control;
        }

        private async Task LoadDataAsync()
        {
            var createdCvs = await _apiService.GetUserCvsAsync(_user.Id);
            var uploadedCvs = await _apiService.GetUploadedCvFilesAsync(_user.Id);

            GetControl<CollectionView>("CvsCollectionView").ItemsSource = createdCvs;
            GetControl<CollectionView>("UploadedCvsCollectionView").ItemsSource = uploadedCvs;

            GetControl<Border>("EmptyCreatedCvsState").IsVisible = createdCvs.Count == 0;
            GetControl<Border>("EmptyUploadedCvsState").IsVisible = uploadedCvs.Count == 0;
        }

        private async void OnCreateCvClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CreateCvPage(_user, _apiService));
        }

        private async void OnUploadCvClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new UploadCvPage(_user, _apiService));
        }

        private async void OnViewDetailsClicked(object sender, EventArgs e)
        {
            if ((sender as Button)?.CommandParameter is not int cvId)
                return;

            await Navigation.PushAsync(new CvDetailsPage(cvId, _apiService));
        }

        private async void OnOpenUploadedCvClicked(object sender, EventArgs e)
        {
            if ((sender as Button)?.CommandParameter is not UploadedCvFileDto uploadedFile)
                return;

            var extension = Path.GetExtension(uploadedFile.OriginalFileName).ToLowerInvariant();

            var downloadUrl = _apiService.GetUploadedCvDownloadUrl(uploadedFile.Id);

            if (extension == ".pdf")
            {
                var previewUrl = _apiService.GetUploadedCvPreviewUrl(uploadedFile.Id);

                await Navigation.PushAsync(
                    new UploadedCvPreviewPage(
                        uploadedFile.OriginalFileName,
                        previewUrl,
                        downloadUrl));

                return;
            }

            try
            {
                await Launcher.OpenAsync(downloadUrl);
            }
            catch
            {
                await DisplayAlert("Error", "The file could not be opened.", "OK");
            }
        }
        private async void OnPreviewCvPdfClicked(object sender, EventArgs e)
        {
            if ((sender as Button)?.CommandParameter is not CvDto cv)
                return;

            var previewUrl = _apiService.GetCvPdfPreviewUrl(cv.Id);
            var downloadUrl = _apiService.GetCvPdfDownloadUrl(cv.Id);

            await Navigation.PushAsync(
                new UploadedCvPreviewPage(
                    cv.Title,
                    previewUrl,
                    downloadUrl));
        }
        private async void OnDeleteCvClicked(object sender, EventArgs e)
        {
            if ((sender as Button)?.CommandParameter is not int cvId)
                return;

            var confirm = await DisplayAlert(
                "Delete CV",
                "Are you sure you want to delete this CV?",
                "Yes",
                "No");

            if (!confirm)
                return;

            var deleted = await _apiService.DeleteCvAsync(cvId);

            if (!deleted)
            {
                await DisplayAlert("Error", "The CV could not be deleted.", "OK");
                return;
            }

            await LoadDataAsync();
        }

        private async void OnDeleteUploadedCvClicked(object sender, EventArgs e)
        {
            if ((sender as Button)?.CommandParameter is not int fileId)
                return;

            var confirm = await DisplayAlert(
                "Delete uploaded CV",
                "Are you sure you want to delete this uploaded CV?",
                "Yes",
                "No");

            if (!confirm)
                return;

            var deleted = await _apiService.DeleteUploadedCvFileAsync(fileId);

            if (!deleted)
            {
                await DisplayAlert("Error", "The uploaded CV could not be deleted.", "OK");
                return;
            }

            await LoadDataAsync();
        }


    }
}