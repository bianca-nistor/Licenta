namespace JobCv.Mobile.Models
{
    public class CvQualityCheckRequest
    {
        public int CvId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public string Summary { get; set; } = string.Empty;

        public List<string> Skills { get; set; } = new();

        public List<string> Educations { get; set; } = new();

        public List<string> Experiences { get; set; } = new();

        public List<string> Projects { get; set; } = new();

        public List<string> Certifications { get; set; } = new();

        public List<string> Languages { get; set; } = new();

        public string Language { get; set; } = "en";
    }
}
