namespace JobCv.Api.Models
{
    public class CvExperience
    {
        public int Id { get; set; }

        public int CvId { get; set; }

        public string JobTitle { get; set; } = string.Empty;

        public string Company { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool IsCurrent { get; set; }

        public Cv? Cv { get; set; }
    }
}