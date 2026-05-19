namespace JobCv.Api.Models
{
    public class JobApplication
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public User? User { get; set; }

        public int? CvId { get; set; }

        public Cv? Cv { get; set; }

        public string JobExternalId { get; set; } = string.Empty;

        public string JobTitle { get; set; } = string.Empty;

        public string Company { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public string JobUrl { get; set; } = string.Empty;

        public string Source { get; set; } = string.Empty;

        public string Status { get; set; } = "Saved";

        public DateTime? AppliedAt { get; set; }

        public DateTime? InterviewAt { get; set; }

        public string Notes { get; set; } = string.Empty;

        public string InterviewNotes { get; set; } = string.Empty;

        public string SalaryRange { get; set; } = string.Empty;

        public string ContactPerson { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}