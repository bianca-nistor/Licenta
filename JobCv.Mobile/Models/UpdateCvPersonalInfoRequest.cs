namespace JobCv.Mobile.Models
{
    public class UpdateCvPersonalInfoRequest
    {
        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public string LinkedInUrl { get; set; } = string.Empty;

        public string GitHubUrl { get; set; } = string.Empty;

        public string PortfolioUrl { get; set; } = string.Empty;
    }
}