namespace JobCv.Api.Dtos
{
    public class CoverLetterRequestDto
    {
        public string JobTitle { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string JobDescription { get; set; } = string.Empty;
        public string CvText { get; set; } = string.Empty;

        public string Language { get; set; } = "en";

    }
}