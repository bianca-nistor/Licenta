namespace JobCv.Mobile.Models
{
    public class UploadedCvFileDto
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string OriginalFileName { get; set; } = string.Empty;

        public string ContentType { get; set; } = string.Empty;

        public long FileSize { get; set; }

        public DateTime UploadedAt { get; set; }

        public string DisplaySize
        {
            get
            {
                if (FileSize <= 0)
                    return "Unknown size";

                var sizeInMb = FileSize / 1024.0 / 1024.0;
                return $"{sizeInMb:0.00} MB";
            }
        }
    }
}