namespace JobCv.Api.Dtos
{
    public class UpdateCvDto
    {
        public string Title { get; set; } = string.Empty;

        public string Language { get; set; } = "ro";

        public string Summary { get; set; } = string.Empty;

        public string TemplateName { get; set; } = "modern-blue";

        public bool IsBaseCv { get; set; } = false;

        public int? TargetJobId { get; set; }
    }
}