namespace JobCv.Mobile.Models
{
    public class InterviewPrepRequest
    {
        public string JobTitle { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public string Language { get; set; } = "en";
    }
}