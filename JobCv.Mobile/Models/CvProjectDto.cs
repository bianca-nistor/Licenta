namespace JobCv.Mobile.Models
{
    public class CvProjectDto
    {
        public int Id { get; set; }

        public int CvId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Technologies { get; set; } = string.Empty;

        public string ProjectUrl { get; set; } = string.Empty;

        public string GitHubUrl { get; set; } = string.Empty;
    }
}