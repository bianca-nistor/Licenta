namespace JobCv.Api.Dtos
{
    public class CreateJobDto
    {
        public string Title { get; set; } = string.Empty;

        public string Company { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Requirements { get; set; } = string.Empty;

        public string WorkMode { get; set; } = string.Empty;

        public string EmploymentType { get; set; } = string.Empty;

        public string Source { get; set; } = "Local";

        public string ExternalUrl { get; set; } = string.Empty;

        public DateTime? PublishedAt { get; set; }
    }
}