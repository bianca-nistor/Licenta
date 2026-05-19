using static System.Net.Mime.MediaTypeNames;

namespace JobCv.Api.Models
{
    public class Job
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Company { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Requirements { get; set; } = string.Empty;

        public string WorkMode { get; set; } = string.Empty;
        // Remote / Hybrid / On-site

        public string EmploymentType { get; set; } = string.Empty;
        // Internship / Full-time / Part-time

        public string Source { get; set; } = string.Empty;
        // Local / Jooble / Adzuna etc.

        public string ExternalUrl { get; set; } = string.Empty;

        public DateTime? PublishedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<Application> Applications { get; set; } = new();
    }
}