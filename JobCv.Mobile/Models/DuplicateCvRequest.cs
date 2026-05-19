namespace JobCv.Mobile.Models
{
    public class DuplicateCvRequest
    {
        public string NewTitle { get; set; } = string.Empty;

        public string TemplateName { get; set; } = string.Empty;

        public int? TargetJobId { get; set; }
    }
}