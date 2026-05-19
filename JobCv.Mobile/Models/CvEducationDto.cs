namespace JobCv.Mobile.Models
{
    public class CvEducationDto
    {
        public int Id { get; set; }

        public int CvId { get; set; }

        public string Institution { get; set; } = string.Empty;

        public string Degree { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }
}