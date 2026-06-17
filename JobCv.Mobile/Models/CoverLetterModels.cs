namespace JobCv.Mobile.Models
{
    public class CoverLetterRequest
    {
        public string JobTitle { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string JobDescription { get; set; } = string.Empty;
        public string CvText { get; set; } = string.Empty;
    }

    public class CoverLetterResponse
    {
        public string JobTitle { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Letter { get; set; } = string.Empty;
        public bool IsMock { get; set; }
    }
}