namespace JobCv.Mobile.Models
{
    public class CvQualityIssueDto
    {
        public string Section { get; set; } = string.Empty;

        public string Problem { get; set; } = string.Empty;

        public string Suggestion { get; set; } = string.Empty;

        public string Severity { get; set; } = string.Empty;

        public string Language { get; set; } = "en";
    }
}
