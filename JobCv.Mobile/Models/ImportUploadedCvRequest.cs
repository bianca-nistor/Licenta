namespace JobCv.Mobile.Models
{
    public class ImportUploadedCvRequest
    {
        public string Title { get; set; } = string.Empty;

        public string Language { get; set; } = "en";

        public string TemplateName { get; set; } = "modern-blue";

        public bool UseAi { get; set; } = false;
    }
}
