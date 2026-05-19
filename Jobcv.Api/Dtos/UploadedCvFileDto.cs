namespace JobCv.Api.Dtos
{
    public class UploadedCvFileDto
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string OriginalFileName { get; set; } = string.Empty;

        public string ContentType { get; set; } = string.Empty;

        public long FileSize { get; set; }

        public DateTime UploadedAt { get; set; }
    }
}