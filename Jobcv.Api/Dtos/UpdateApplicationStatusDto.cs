namespace JobCv.Api.Dtos
{
    public class UpdateApplicationStatusDto
    {
        public string Status { get; set; } = string.Empty;

        public DateTime? AppliedAt { get; set; }

        public DateTime? InterviewDate { get; set; }

        public string Notes { get; set; } = string.Empty;
    }
}