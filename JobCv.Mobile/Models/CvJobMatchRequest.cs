namespace JobCv.Mobile.Models
{
    public class CvJobMatchRequest
    {
        public string JobTitle { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string JobDescription { get; set; } = string.Empty;
        public string CurrentCvText { get; set; } = string.Empty;

        public string Language { get; set; } = "en";
    }
}
