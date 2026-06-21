namespace JobCv.Api.Dtos
{
    public class ImportUploadedCvRequestDto
    {
        public string Title { get; set; } = string.Empty;

        public string Language { get; set; } = "ro";

        public string TemplateName { get; set; } = "modern-blue";

        public bool UseAi { get; set; } = true;
    }
}