using JobCv.Mobile.Models;
using JobCv.Mobile.Services;

namespace JobCv.Mobile.Pages
{
    public partial class EditCvPage : ContentPage
    {
        private readonly int _cvId;
        private readonly ApiService _apiService;

        private CvDto? _cv;
        private List<CvTemplateDto> _templates = new();

        public EditCvPage(int cvId, ApiService apiService)
        {
            InitializeComponent();

            _cvId = cvId;
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
                throw new Exception($"Control '{name}' was not found in EditCvPage.xaml.");

            return control;
        }

        private async Task LoadDataAsync()
        {
            _cv = await _apiService.GetCvByIdAsync(_cvId);
            _templates = await _apiService.GetCvTemplatesAsync();

            if (_cv == null)
            {
                await DisplayAlert("Error", "The CV could not be loaded.", "OK");
                await Navigation.PopAsync();
                return;
            }

            GetControl<Entry>("TitleEntry").Text = _cv.Title;
            GetControl<Entry>("FullNameEntry").Text = _cv.FullName;
            GetControl<Entry>("EmailEntry").Text = _cv.Email;
            GetControl<Entry>("PhoneEntry").Text = _cv.Phone;
            GetControl<Entry>("LocationEntry").Text = _cv.Location;
            GetControl<Entry>("LinkedInEntry").Text = _cv.LinkedInUrl;
            GetControl<Entry>("GitHubEntry").Text = _cv.GitHubUrl;
            GetControl<Entry>("PortfolioEntry").Text = _cv.PortfolioUrl;
            GetControl<Editor>("SummaryEditor").Text = _cv.Summary;
            GetControl<CheckBox>("BaseCvCheckBox").IsChecked = _cv.IsBaseCv;

            var photoBorder = GetControl<Border>("CvPhotoBorder");
            var photoImage = GetControl<Image>("CvPhotoImage");

            if (_cv.HasPhoto)
            {
                photoBorder.IsVisible = true;
                photoImage.Source = ImageSource.FromUri(new Uri(_apiService.GetCvPhotoUrl(_cvId)));
            }
            else
            {
                photoBorder.IsVisible = false;
                photoImage.Source = null;
            }

            var languagePicker = GetControl<Picker>("LanguagePicker");
            languagePicker.SelectedIndex = _cv.Language == "ro" ? 1 : 0;

            var templatePicker = GetControl<Picker>("TemplatePicker");
            templatePicker.ItemsSource = _templates;
            var templateIndex = _templates.FindIndex(x => x.Id == _cv.TemplateName);
            templatePicker.SelectedIndex = templateIndex >= 0 ? templateIndex : 0;

            RefreshSectionLists();

        }

        private void RefreshSectionLists()
        {
            if (_cv == null)
                return;

            GetControl<CollectionView>("SkillsCollectionView").ItemsSource = null;
            GetControl<CollectionView>("SkillsCollectionView").ItemsSource = _cv.Skills;

            GetControl<CollectionView>("LanguagesCollectionView").ItemsSource = null;
            GetControl<CollectionView>("LanguagesCollectionView").ItemsSource = _cv.Languages;

            GetControl<CollectionView>("EducationsCollectionView").ItemsSource = null;
            GetControl<CollectionView>("EducationsCollectionView").ItemsSource = _cv.Educations;

            GetControl<CollectionView>("ExperiencesCollectionView").ItemsSource = null;
            GetControl<CollectionView>("ExperiencesCollectionView").ItemsSource = _cv.Experiences;

            GetControl<CollectionView>("ProjectsCollectionView").ItemsSource = null;
            GetControl<CollectionView>("ProjectsCollectionView").ItemsSource = _cv.Projects;

            GetControl<CollectionView>("CertificationsCollectionView").ItemsSource = null;
            GetControl<CollectionView>("CertificationsCollectionView").ItemsSource = _cv.Certifications;
        }

        private async Task ReloadCvAsync()
        {
            _cv = await _apiService.GetCvByIdAsync(_cvId);
            RefreshSectionLists();
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnSaveBasicInfoClicked(object sender, EventArgs e)
        {
            var messageLabel = GetControl<Label>("MessageLabel");

            messageLabel.Text = "";

            try
            {
                await SaveBasicInformationAsync();

                messageLabel.TextColor = Colors.Green;
                messageLabel.Text = "Saved successfully.";
            }
            catch (Exception ex)
            {
                messageLabel.TextColor = Colors.Red;
                messageLabel.Text = ex.Message;
            }
        }
        private async Task SaveBasicInformationAsync()
        {
            var title = GetControl<Entry>("TitleEntry").Text?.Trim() ?? "";
            var fullName = GetControl<Entry>("FullNameEntry").Text?.Trim() ?? "";
            var email = GetControl<Entry>("EmailEntry").Text?.Trim() ?? "";
            var phone = GetControl<Entry>("PhoneEntry").Text?.Trim() ?? "";
            var location = GetControl<Entry>("LocationEntry").Text?.Trim() ?? "";
            var linkedIn = GetControl<Entry>("LinkedInEntry").Text?.Trim() ?? "";
            var gitHub = GetControl<Entry>("GitHubEntry").Text?.Trim() ?? "";
            var portfolio = GetControl<Entry>("PortfolioEntry").Text?.Trim() ?? "";
            var summary = GetControl<Editor>("SummaryEditor").Text?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(title))
            {
                throw new Exception("CV title is required.");
            }

            var languagePicker = GetControl<Picker>("LanguagePicker");
            var templatePicker = GetControl<Picker>("TemplatePicker");
            var baseCvCheckBox = GetControl<CheckBox>("BaseCvCheckBox");

            var selectedTemplate = templatePicker.SelectedItem as CvTemplateDto;

            var updateRequest = new UpdateCvRequest
            {
                Title = title,
                Language = languagePicker.SelectedItem?.ToString() == "Romanian" ? "ro" : "en",
                Summary = summary,
                TemplateName = selectedTemplate?.Id ?? "modern-blue",
                IsBaseCv = baseCvCheckBox.IsChecked,
                TargetJobId = null
            };

            var updated = await _apiService.UpdateCvAsync(_cvId, updateRequest);

            if (!updated)
            {
                throw new Exception("The CV could not be saved.");
            }

            var personalInfoRequest = new UpdateCvPersonalInfoRequest
            {
                FullName = fullName,
                Email = email,
                Phone = phone,
                Location = location,
                LinkedInUrl = linkedIn,
                GitHubUrl = gitHub,
                PortfolioUrl = portfolio
            };

            var personalInfoUpdated = await _apiService.UpdateCvPersonalInfoAsync(_cvId, personalInfoRequest);

            if (!personalInfoUpdated)
            {
                throw new Exception("The personal information could not be saved.");
            }

            _cv = await _apiService.GetCvByIdAsync(_cvId);
        }
        private async void OnAddSkillClicked(object sender, EventArgs e)
        {
            var name = await DisplayPromptAsync("Add skill", "Skill name:");

            if (string.IsNullOrWhiteSpace(name))
                return;

            await _apiService.AddSkillAsync(_cvId, new AddSkillRequest { Name = name.Trim() });
            await ReloadCvAsync();
        }

        private async void OnEditSkillClicked(object sender, EventArgs e)
        {
            if ((sender as Button)?.CommandParameter is not CvSkillDto skill)
                return;

            var name = await DisplayPromptAsync("Edit skill", "Skill name:", initialValue: skill.Name);

            if (string.IsNullOrWhiteSpace(name))
                return;

            await _apiService.UpdateSkillAsync(skill.Id, new UpdateSkillRequest { Name = name.Trim() });
            await ReloadCvAsync();
        }

        private async void OnDeleteSkillClicked(object sender, EventArgs e)
        {
            if ((sender as Button)?.CommandParameter is not CvSkillDto skill)
                return;

            var confirm = await DisplayAlert("Delete skill", $"Delete {skill.Name}?", "Yes", "No");

            if (!confirm)
                return;

            await _apiService.DeleteSkillAsync(skill.Id);
            await ReloadCvAsync();
        }

        private async void OnAddLanguageClicked(object sender, EventArgs e)
        {
            var name = await DisplayPromptAsync("Add language", "Language name:");
            if (string.IsNullOrWhiteSpace(name)) return;

            var level = await DisplayPromptAsync("Language level", "Example: Beginner, Intermediate, Advanced, Native:");
            if (string.IsNullOrWhiteSpace(level)) return;

            await _apiService.AddLanguageAsync(_cvId, new AddLanguageRequest
            {
                Name = name.Trim(),
                Level = level.Trim()
            });

            await ReloadCvAsync();
        }

        private async void OnEditLanguageClicked(object sender, EventArgs e)
        {
            if ((sender as Button)?.CommandParameter is not CvLanguageDto language)
                return;

            var name = await DisplayPromptAsync("Edit language", "Language name:", initialValue: language.Name);
            if (string.IsNullOrWhiteSpace(name)) return;

            var level = await DisplayPromptAsync("Language level", "Level:", initialValue: language.Level);
            if (string.IsNullOrWhiteSpace(level)) return;

            await _apiService.UpdateLanguageAsync(language.Id, new UpdateLanguageRequest
            {
                Name = name.Trim(),
                Level = level.Trim()
            });

            await ReloadCvAsync();
        }

        private async void OnDeleteLanguageClicked(object sender, EventArgs e)
        {
            if ((sender as Button)?.CommandParameter is not CvLanguageDto language)
                return;

            var confirm = await DisplayAlert("Delete language", $"Delete {language.Name}?", "Yes", "No");

            if (!confirm)
                return;

            await _apiService.DeleteLanguageAsync(language.Id);
            await ReloadCvAsync();
        }

        private async void OnAddEducationClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new EducationFormPage(_cvId, _apiService));
        }

        private async void OnEditEducationClicked(object sender, EventArgs e)
        {
            if ((sender as Button)?.CommandParameter is not CvEducationDto education)
                return;

            await Navigation.PushAsync(new EducationFormPage(_cvId, _apiService, education));
        }

        private async void OnDeleteEducationClicked(object sender, EventArgs e)
        {
            if ((sender as Button)?.CommandParameter is not CvEducationDto education)
                return;

            var confirm = await DisplayAlert("Delete education", $"Delete {education.Degree}?", "Yes", "No");

            if (!confirm)
                return;

            await _apiService.DeleteEducationAsync(education.Id);
            await ReloadCvAsync();
        }

        private async void OnAddExperienceClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ExperienceFormPage(_cvId, _apiService));
        }

        private async void OnEditExperienceClicked(object sender, EventArgs e)
        {
            if ((sender as Button)?.CommandParameter is not CvExperienceDto experience)
                return;

            await Navigation.PushAsync(new ExperienceFormPage(_cvId, _apiService, experience));
        }
        private async void OnDeleteExperienceClicked(object sender, EventArgs e)
        {
            if ((sender as Button)?.CommandParameter is not CvExperienceDto experience)
                return;

            var confirm = await DisplayAlert("Delete experience", $"Delete {experience.JobTitle}?", "Yes", "No");

            if (!confirm)
                return;

            await _apiService.DeleteExperienceAsync(experience.Id);
            await ReloadCvAsync();
        }

        private async void OnAddProjectClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ProjectFormPage(_cvId, _apiService));
        }

        private async void OnEditProjectClicked(object sender, EventArgs e)
        {
            if ((sender as Button)?.CommandParameter is not CvProjectDto project)
                return;

            await Navigation.PushAsync(new ProjectFormPage(_cvId, _apiService, project));
        }

        private async void OnDeleteProjectClicked(object sender, EventArgs e)
        {
            if ((sender as Button)?.CommandParameter is not CvProjectDto project)
                return;

            var confirm = await DisplayAlert("Delete project", $"Delete {project.Title}?", "Yes", "No");

            if (!confirm)
                return;

            await _apiService.DeleteProjectAsync(project.Id);
            await ReloadCvAsync();
        }

        private async void OnAddCertificationClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CertificationFormPage(_cvId, _apiService));
        }

        private async void OnEditCertificationClicked(object sender, EventArgs e)
        {
            if ((sender as Button)?.CommandParameter is not CvCertificationDto certification)
                return;

            await Navigation.PushAsync(new CertificationFormPage(_cvId, _apiService, certification));
        }

        private async void OnDeleteCertificationClicked(object sender, EventArgs e)
        {
            if ((sender as Button)?.CommandParameter is not CvCertificationDto certification)
                return;

            var confirm = await DisplayAlert("Delete certification", $"Delete {certification.Name}?", "Yes", "No");

            if (!confirm)
                return;

            await _apiService.DeleteCertificationAsync(certification.Id);
            await ReloadCvAsync();
        }
        private async void OnChooseCvPhotoClicked(object sender, EventArgs e)
        {
            var messageLabel = GetControl<Label>("PhotoMessageLabel");

            messageLabel.Text = "";

            var customFileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
    {
        { DevicePlatform.WinUI, new[] { ".jpg", ".jpeg", ".png", ".webp" } },
        { DevicePlatform.Android, new[] { "image/jpeg", "image/png", "image/webp" } }
    });

            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Choose CV photo",
                FileTypes = customFileType
            });

            if (result == null)
                return;

            var extension = Path.GetExtension(result.FileName).ToLowerInvariant();

            if (extension != ".jpg" && extension != ".jpeg" && extension != ".png" && extension != ".webp")
            {
                messageLabel.TextColor = Colors.Red;
                messageLabel.Text = "Only JPG, JPEG, PNG and WEBP images are allowed.";
                return;
            }

            try
            {
                var updatedCv = await _apiService.UploadCvPhotoAsync(_cvId, result);

                if (updatedCv == null)
                {
                    messageLabel.TextColor = Colors.Red;
                    messageLabel.Text = "The photo could not be uploaded.";
                    return;
                }

                _cv = updatedCv;

                GetControl<Border>("CvPhotoBorder").IsVisible = true;

                GetControl<Image>("CvPhotoImage").Source =
                    ImageSource.FromUri(new Uri(_apiService.GetCvPhotoUrl(_cvId) + $"?v={DateTime.UtcNow.Ticks}"));

                messageLabel.TextColor = Colors.Green;
                messageLabel.Text = "Photo uploaded successfully.";
            }
            catch (Exception ex)
            {
                messageLabel.TextColor = Colors.Red;
                messageLabel.Text = ex.Message;
            }
        }
        private async void OnSaveCvClicked(object sender, EventArgs e)
        {
            var bottomMessageLabel = GetControl<Label>("BottomSaveMessageLabel");

            bottomMessageLabel.Text = "";

            try
            {
                await SaveBasicInformationAsync();

                bottomMessageLabel.TextColor = Colors.Green;
                bottomMessageLabel.Text = "CV saved successfully.";
            }
            catch (Exception ex)
            {
                bottomMessageLabel.TextColor = Colors.Red;
                bottomMessageLabel.Text = ex.Message;
            }
        }
    }
}