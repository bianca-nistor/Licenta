namespace JobCv.Api.Dtos
{
    public class DuplicateCvDto
    {
        public string NewTitle { get; set; } = string.Empty;

        public int? TargetJobId { get; set; }

        public string? TemplateName { get; set; }
    }
}