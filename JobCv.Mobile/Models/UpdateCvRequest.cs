namespace JobCv.Mobile.Models
{
    public class UpdateCvRequest
    {
        public string Title { get; set; } = string.Empty;

        public string Language { get; set; } = "en";

        public string Summary { get; set; } = string.Empty;

        public string TemplateName { get; set; } = "modern-blue";

        public bool IsBaseCv { get; set; }

        public int? TargetJobId { get; set; }
    }
}