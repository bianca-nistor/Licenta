namespace JobCv.Api.Dtos
{
    public class CreateApplicationDto
    {
        public int UserId { get; set; }

        public int JobId { get; set; }

        public int? CvId { get; set; }

        public string Status { get; set; } = "Saved";

        public DateTime? AppliedAt { get; set; }

        public string Notes { get; set; } = string.Empty;
    }
}