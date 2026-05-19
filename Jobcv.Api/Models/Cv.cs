namespace JobCv.Api.Models
{
    public class Cv
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Language { get; set; } = "ro";

        public string Summary { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public string PhotoFileName { get; set; } = string.Empty;

        public string PhotoPath { get; set; } = string.Empty;

        public string PhotoContentType { get; set; } = string.Empty;

        public string LinkedInUrl { get; set; } = string.Empty;

        public string GitHubUrl { get; set; } = string.Empty;

        public string PortfolioUrl { get; set; } = string.Empty;
        public string TemplateName { get; set; } = "modern-blue";

        public bool IsBaseCv { get; set; } = false;

        public int? ParentCvId { get; set; }

        public int? TargetJobId { get; set; }

        public Cv? ParentCv { get; set; }

        public Job? TargetJob { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User? User { get; set; }

        public List<CvSkill> Skills { get; set; } = new();

        public List<CvExperience> Experiences { get; set; } = new();

        public List<CvEducation> Educations { get; set; } = new();
        public List<CvProject> Projects { get; set; } = new();

        public List<CvLanguage> Languages { get; set; } = new();

        public List<CvCertification> Certifications { get; set; } = new();
    }
}