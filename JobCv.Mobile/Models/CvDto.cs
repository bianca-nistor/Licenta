namespace JobCv.Mobile.Models
{
    public class CvDto
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Language { get; set; } = string.Empty;

        public string Summary { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}