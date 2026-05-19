namespace JobCv.Api.Dtos
{
    public class AddProjectDto
    {
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Technologies { get; set; } = string.Empty;

        public string ProjectUrl { get; set; } = string.Empty;

        public string GitHubUrl { get; set; } = string.Empty;
    }
}