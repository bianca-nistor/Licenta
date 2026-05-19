using JobCv.Mobile.Models;
using JobCv.Mobile.Services;


namespace JobCv.Mobile.Pages
{
    public partial class ProjectFormPage : ContentPage
    {
        private readonly int _cvId;
        private readonly ApiService _apiService;
        private readonly CvProjectDto? _projectToEdit;
        private readonly bool _isEditMode;

        public ProjectFormPage(int cvId, ApiService apiService)
        {
            InitializeComponent();

            _cvId = cvId;
            _apiService = apiService;
            _isEditMode = false;
        }

        public ProjectFormPage(int cvId, ApiService apiService, CvProjectDto projectToEdit)
        {
            InitializeComponent();

            _cvId = cvId;
            _apiService = apiService;
            _projectToEdit = projectToEdit;
            _isEditMode = true;

            LoadProjectForEdit();
        }

        private void LoadProjectForEdit()
        {
            if (_projectToEdit == null)
                return;

            PageTitleLabel.Text = "Edit Project";
            SaveButton.Text = "Save changes";

            TitleEntry.Text = _projectToEdit.Title;
            TechnologiesEntry.Text = _projectToEdit.Technologies;
            DescriptionEditor.Text = _projectToEdit.Description;
            ProjectUrlEntry.Text = _projectToEdit.ProjectUrl;
            GitHubUrlEntry.Text = _projectToEdit.GitHubUrl;
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            MessageLabel.Text = "";

            var title = TitleEntry.Text?.Trim() ?? "";
            var technologies = TechnologiesEntry.Text?.Trim() ?? "";
            var description = DescriptionEditor.Text?.Trim() ?? "";
            var projectUrl = ProjectUrlEntry.Text?.Trim() ?? "";
            var gitHubUrl = GitHubUrlEntry.Text?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(title))
            {
                MessageLabel.TextColor = Colors.Red;
                MessageLabel.Text = "Project title is required.";
                return;
            }

            if (_isEditMode)
            {
                await UpdateProjectAsync(title, technologies, description, projectUrl, gitHubUrl);
                return;
            }

            await AddProjectAsync(title, technologies, description, projectUrl, gitHubUrl);
        }

        private async Task AddProjectAsync(
            string title,
            string technologies,
            string description,
            string projectUrl,
            string gitHubUrl)
        {
            try
            {
                var result = await _apiService.AddProjectAsync(_cvId, new AddProjectRequest
                {
                    Title = title,
                    Technologies = technologies,
                    Description = description,
                    ProjectUrl = projectUrl,
                    GitHubUrl = gitHubUrl
                });

                if (result == null)
                {
                    MessageLabel.TextColor = Colors.Red;
                    MessageLabel.Text = "Project could not be saved.";
                    return;
                }

                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async Task UpdateProjectAsync(
            string title,
            string technologies,
            string description,
            string projectUrl,
            string gitHubUrl)
        {
            if (_projectToEdit == null)
                return;

            try
            {
                var updated = await _apiService.UpdateProjectAsync(_projectToEdit.Id, new UpdateProjectRequest
                {
                    Title = title,
                    Technologies = technologies,
                    Description = description,
                    ProjectUrl = projectUrl,
                    GitHubUrl = gitHubUrl
                });

                if (!updated)
                {
                    MessageLabel.TextColor = Colors.Red;
                    MessageLabel.Text = "Project could not be updated.";
                    return;
                }

                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }
}