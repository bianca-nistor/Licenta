using JobCv.Mobile.Models;
using JobCv.Mobile.Services;
using Microsoft.Maui.ApplicationModel;

namespace JobCv.Mobile.Pages
{
    public partial class MyCvsPage : ContentPage
    {
        private readonly UserDto _user;
        private readonly ApiService _apiService;

        public MyCvsPage(UserDto user, ApiService apiService)
        {
            InitializeComponent();

            _user = user;
            _apiService = apiService;
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
                throw new Exception($"Control '{name}' was not found in MyCvsPage.xaml.");

            return control;
        }

        private Border GetMainMenu()
        {
            return GetControl<Border>("MainMenu");
        }

        private Border GetProfileMenu()
        {
            return GetControl<Border>("ProfileMenu");
        }

        private void SetLabelText(string labelName, string text)
        {
            var label = this.FindByName<Label>(labelName);

            if (label != null)
                label.Text = text;
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var createdCvs = await _apiService.GetUserCvsAsync(_user.Id);
                var uploadedCvs = await _apiService.GetUploadedCvFilesAsync(_user.Id);

                GetControl<CollectionView>("CvsCollectionView").ItemsSource = createdCvs;
                GetControl<CollectionView>("UploadedCvsCollectionView").ItemsSource = uploadedCvs;

                GetControl<Border>("EmptyCreatedCvsState").IsVisible = createdCvs.Count == 0;
                GetControl<Border>("EmptyUploadedCvsState").IsVisible = uploadedCvs.Count == 0;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private void OnMenuClicked(object sender, EventArgs e)
        {
            var mainMenu = GetMainMenu();
            var profileMenu = GetProfileMenu();

            mainMenu.IsVisible = !mainMenu.IsVisible;

            if (profileMenu.IsVisible)
                profileMenu.IsVisible = false;
        }

        private async void OnDashboardClicked(object sender, EventArgs e)
        {
            GetMainMenu().IsVisible = false;
            await Navigation.PushAsync(new CvsPage(_user, _apiService));
        }

        private void OnMyCvsClicked(object sender, EventArgs e)
        {
            GetMainMenu().IsVisible = false;
        }

        private async void OnMyApplicationsClicked(object sender, EventArgs e)
        {
            GetMainMenu().IsVisible = false;
            await Navigation.PushAsync(new ApplicationsPage(_user, _apiService));
        }

        private async void OnFindJobsClicked(object sender, EventArgs e)
        {
            GetMainMenu().IsVisible = false;
            await Navigation.PushAsync(new JobsPage(_user, _apiService));
        }

        private async void OnCreateCvClicked(object sender, EventArgs e)
        {
            GetMainMenu().IsVisible = false;
            await Navigation.PushAsync(new CreateCvPage(_user, _apiService));
        }

        private async void OnUploadCvClicked(object sender, EventArgs e)
        {
            GetMainMenu().IsVisible = false;
            await Navigation.PushAsync(new UploadCvPage(_user, _apiService));
        }

        private async void OnViewDetailsClicked(object sender, EventArgs e)
        {
            if ((sender as Button)?.CommandParameter is not int cvId)
                return;

            await Navigation.PushAsync(new CvDetailsPage(cvId, _apiService));
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

        private void OnProfileClicked(object sender, EventArgs e)
        {
            var profileMenu = GetProfileMenu();
            var mainMenu = GetMainMenu();

            profileMenu.IsVisible = !profileMenu.IsVisible;

            if (mainMenu.IsVisible)
                mainMenu.IsVisible = false;
        }

        private async void OnViewProfileClicked(object sender, EventArgs e)
        {
            GetProfileMenu().IsVisible = false;

            await Navigation.PushAsync(new ProfilePage(_user, _apiService));
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            GetProfileMenu().IsVisible = false;

            var confirm = await DisplayAlert(
                "Logout",
                "Are you sure you want to log out?",
                "Yes",
                "No");

            if (!confirm)
                return;

            Application.Current!.Windows[0].Page = new NavigationPage(new MainPage());
        }
        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            if (PageTitleLabel == null)
                return;

            if (width < 360)
            {
                PageTitleLabel.FontSize = 26;
            }
            else if (width < 430)
            {
                PageTitleLabel.FontSize = 30;
            }
            else
            {
                PageTitleLabel.FontSize = 34;
            }
        }
        private async void OnCheckCvQualityClicked(object sender, EventArgs e)
        {
            if ((sender as Button)?.CommandParameter is not int cvId)
                return;

            await Navigation.PushAsync(new CvQualityCheckPage(cvId, _apiService));
        }
       

    }
}