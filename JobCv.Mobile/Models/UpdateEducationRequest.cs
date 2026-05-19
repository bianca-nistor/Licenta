namespace JobCv.Mobile.Models
{
    public class UpdateEducationRequest
    {
        public string Institution { get; set; } = string.Empty;

        public string Degree { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }
}