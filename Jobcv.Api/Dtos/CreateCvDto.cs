namespace JobCv.Api.Dtos
{
    public class CreateCvDto
    {
        public int UserId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Language { get; set; } = "ro";

        public string Summary { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public string LinkedInUrl { get; set; } = string.Empty;

        public string GitHubUrl { get; set; } = string.Empty;

        public string PortfolioUrl { get; set; } = string.Empty;

        public string TemplateName { get; set; } = "modern-blue";

        public bool IsBaseCv { get; set; } = false;

        public int? ParentCvId { get; set; }

        public int? TargetJobId { get; set; }
    }
}