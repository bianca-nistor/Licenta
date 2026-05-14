namespace JobCv.Api.Dtos
{
    public class CreateCvDto
    {
        public int UserId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Language { get; set; } = "ro";

        public string Summary { get; set; } = string.Empty;
    }
}