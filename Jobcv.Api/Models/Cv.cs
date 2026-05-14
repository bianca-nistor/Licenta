namespace JobCv.Api.Models
{
    public class Cv
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Language { get; set; } = "ro";

        public string Summary { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User? User { get; set; }

        public List<CvSkill> Skills { get; set; } = new();

        public List<CvExperience> Experiences { get; set; } = new();

        public List<CvEducation> Educations { get; set; } = new();
    }
}