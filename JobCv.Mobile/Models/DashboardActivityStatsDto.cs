namespace JobCv.Mobile.Models
{
    public class DashboardActivityStatsDto
    {
        public string Period { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public int CreatedCvsCount { get; set; }
        public int UploadedCvsCount { get; set; }
        public int ApplicationsCount { get; set; }
        public int InterviewsCount { get; set; }
    }
}
