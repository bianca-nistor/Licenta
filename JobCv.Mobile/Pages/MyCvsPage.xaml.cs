using JobCv.Mobile.Models;
using JobCv.Mobile.Services;
using Microsoft.Maui.ApplicationModel;

namespace JobCv.Mobile.Pages
{
    public partial class MyCvsPage : ContentPage
    {
        private readonly UserDto _user;
        private readonly ApiService _apiService;

        private List<CvDto> _allCreatedCvs = new();
        private List<UploadedCvFileDto> _allUploadedCvs = new();

        private string _searchText = string.Empty;
        private string _selectedFilter = "All";
        private string _selectedSort = "Newest";

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
                _allCreatedCvs = await _apiService.GetUserCvsAsync(_user.Id);
                _allUploadedCvs = await _apiService.GetUploadedCvFilesAsync(_user.Id);

                ApplyCvFilters();
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

            await Navigation.PushAsync(
                new CvTemplatePreviewPage(
                    cv.Id,
                    _apiService));
        }

        private async void OnDeleteCvClicked(object sender, EventArgs e)
        {
            if ((sender as Button)?.CommandParameter is not int cvId)
                return;

            var confirm = await UiTranslationService.DisplayConfirmAsync(
                this,
                "Delete CV",
                "Are you sure you want to delete this CV?");

            if (!confirm)
                return;

            var deleted = await _apiService.DeleteCvAsync(cvId);

            if (!deleted)
            {
                await UiTranslationService.DisplayAlertAsync(this, "Error", "The CV could not be deleted.");
                return;
            }

            await LoadDataAsync();
        }

        private async void OnOpenUploadedCvClicked(object sender, EventArgs e)
        {
            if ((sender as Button)?.CommandParameter is not UploadedCvFileDto uploadedFile)
                return;

            var downloadUrl = _apiService.GetUploadedCvDownloadUrl(uploadedFile.Id);

            try
            {
                await Launcher.OpenAsync(downloadUrl);
            }
            catch
            {
                await UiTranslationService.DisplayAlertAsync(
                    this,
                    "Error",
                    "The file could not be opened. Please make sure your phone has an app that can open this file type.");
            }
        }

        private async void OnDeleteUploadedCvClicked(object sender, EventArgs e)
        {
            if ((sender as Button)?.CommandParameter is not int fileId)
                return;

            var confirm = await UiTranslationService.DisplayConfirmAsync(
                this,
                "Delete uploaded CV",
                "Are you sure you want to delete this uploaded CV?");

            if (!confirm)
                return;

            var deleted = await _apiService.DeleteUploadedCvFileAsync(fileId);

            if (!deleted)
            {
                await UiTranslationService.DisplayAlertAsync(this, "Error", "The uploaded CV could not be deleted.");
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

            var confirm = await UiTranslationService.DisplayConfirmAsync(
                this,
                "Logout",
                "Are you sure you want to log out?");

            if (!confirm)
                return;

            Application.Current!.Windows[0].Page = new LocalizedNavigationPage(new MainPage());
        }
        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            if (PageTitleLabel == null)
                return;

            if (width < 360)
            {
                PageTitleLabel.FontSize = 24;
            }
            else if (width < 430)
            {
                PageTitleLabel.FontSize = 26;
            }
            else
            {
                PageTitleLabel.FontSize = 30;
            }
        }
        private async void OnCheckCvQualityClicked(object sender, EventArgs e)
        {
            if ((sender as Button)?.CommandParameter is not int cvId)
                return;

            await Navigation.PushAsync(new CvQualityCheckPage(cvId, _apiService));
        }
        private void ApplyCvFilters()
        {
            var createdCvs = _allCreatedCvs.AsEnumerable();
            var uploadedCvs = _allUploadedCvs.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(_searchText))
            {
                var normalizedSearch = _searchText.Trim();

                createdCvs = createdCvs.Where(cv =>
                    !string.IsNullOrWhiteSpace(cv.Title) &&
                    cv.Title.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase));

                uploadedCvs = uploadedCvs.Where(cv =>
                    !string.IsNullOrWhiteSpace(cv.OriginalFileName) &&
                    cv.OriginalFileName.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase));
            }

            createdCvs = _selectedSort switch
            {
                "Oldest" => createdCvs.OrderBy(cv => cv.CreatedAt),
                "A-Z" => createdCvs.OrderBy(cv => cv.Title),
                _ => createdCvs.OrderByDescending(cv => cv.CreatedAt)
            };

            uploadedCvs = _selectedSort switch
            {
                "Oldest" => uploadedCvs.OrderBy(cv => cv.UploadedAt),
                "A-Z" => uploadedCvs.OrderBy(cv => cv.OriginalFileName),
                _ => uploadedCvs.OrderByDescending(cv => cv.UploadedAt)
            };

            var filteredCreatedCvs = createdCvs.ToList();
            var filteredUploadedCvs = uploadedCvs.ToList();

            var showCreated = _selectedFilter == "All" || _selectedFilter == "Created";
            var showUploaded = _selectedFilter == "All" || _selectedFilter == "Uploaded";

            CreatedCvsSectionHeader.IsVisible = showCreated;
            CvsCollectionView.IsVisible = showCreated;
            CreateAnotherCvButton.IsVisible = showCreated;

            UploadedCvsSectionHeader.IsVisible = showUploaded;
            UploadedCvsCollectionView.IsVisible = showUploaded;
            UploadAnotherCvButton.IsVisible = showUploaded;

            GetControl<CollectionView>("CvsCollectionView").ItemsSource = showCreated
                ? filteredCreatedCvs
                : new List<CvDto>();

            GetControl<CollectionView>("UploadedCvsCollectionView").ItemsSource = showUploaded
                ? filteredUploadedCvs
                : new List<UploadedCvFileDto>();

            GetControl<Border>("EmptyCreatedCvsState").IsVisible =
                showCreated && filteredCreatedCvs.Count == 0;

            GetControl<Border>("EmptyUploadedCvsState").IsVisible =
                showUploaded && filteredUploadedCvs.Count == 0;
        }

        private void OnCvSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            _searchText = e.NewTextValue ?? string.Empty;
            ApplyCvFilters();
        }

        private void OnCvFilterChanged(object sender, EventArgs e)
        {
            if (CvTypeFilterPicker.SelectedIndex < 0)
                return;

            _selectedFilter = CvTypeFilterPicker.Items[CvTypeFilterPicker.SelectedIndex];
            ApplyCvFilters();
        }

        private void OnCvSortChanged(object sender, EventArgs e)
        {
            if (CvSortPicker.SelectedIndex < 0)
                return;

            _selectedSort = CvSortPicker.Items[CvSortPicker.SelectedIndex];
            ApplyCvFilters();
        }

    }
}