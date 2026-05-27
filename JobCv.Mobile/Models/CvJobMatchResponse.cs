namespace JobCv.Mobile.Models
{
    public class CvJobMatchResponse
    {
        public string JobTitle { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public int MatchScore { get; set; }
        public string Recommendation { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public List<string> Strengths { get; set; } = new();
        public List<string> MissingSkills { get; set; } = new();
        public List<string> Improvements { get; set; } = new();
        public bool IsMock { get; set; }
    }
}
