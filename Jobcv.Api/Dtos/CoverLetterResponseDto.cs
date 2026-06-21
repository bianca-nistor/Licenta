namespace JobCv.Api.Dtos
{
    public class CoverLetterResponseDto
    {
        public string JobTitle { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Letter { get; set; } = string.Empty;
        public bool IsMock { get; set; }

       

    }
}