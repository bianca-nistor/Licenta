namespace JobCv.Mobile.Models
{
    public class JobSearchResultDto
    {
        public string ExternalId { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Company { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string ApplyUrl { get; set; } = string.Empty;

        public string Source { get; set; } = string.Empty;

        public string Salary { get; set; } = string.Empty;

        public DateTime? PostedAt { get; set; }

        public string PostedText
        {
            get
            {
                if (!PostedAt.HasValue)
                    return "Posted date not available";

                return $"Posted {PostedAt.Value:MMM dd, yyyy}";
            }
        }
    }
}