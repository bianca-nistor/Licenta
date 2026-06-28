namespace JobCv.Mobile.Models
{
    public class CareerTestQuestion
    {
        public int Id { get; set; }

        // Kept for compatibility with older code.
        public string Text { get; set; } = string.Empty;
        public string Area { get; set; } = string.Empty;

        public string TextEn { get; set; } = string.Empty;
        public string TextRo { get; set; } = string.Empty;

        public string AreaCode { get; set; } = string.Empty; // R, I, A, S, E, C
        public string AreaEn { get; set; } = string.Empty;
        public string AreaRo { get; set; } = string.Empty;
    }

    public class CareerAreaScore
    {
        public string Area { get; set; } = string.Empty;
        public string AreaCode { get; set; } = string.Empty;

        // Percentage, used by older PDF/UI logic and progress bars.
        public int Score { get; set; }

        // O*NET Short Form paper-and-pencil scoring: checked activities / total activities.
        public int RawScore { get; set; }
        public int MaxScore { get; set; } = 10;

        public string Description { get; set; } = string.Empty;
    }

    public class CareerTestResult
    {
        public string ProfileTitle { get; set; } = string.Empty;
        public string ProfileCode { get; set; } = string.Empty;
        public string Language { get; set; } = "en";
        public string Summary { get; set; } = string.Empty;
        public string SourceAttribution { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; } = DateTime.Now;

        public List<CareerAreaScore> AreaScores { get; set; } = new();
        public List<string> RecommendedRoles { get; set; } = new();
        public List<string> NextSteps { get; set; } = new();
    }

    public class CareerTestPdfRequest
    {
        public string ProfileTitle { get; set; } = string.Empty;
        public string ProfileCode { get; set; } = string.Empty;
        public string Language { get; set; } = "en";
        public string Summary { get; set; } = string.Empty;
        public string SourceAttribution { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        public List<CareerAreaScore> AreaScores { get; set; } = new();
        public List<string> RecommendedRoles { get; set; } = new();
        public List<string> NextSteps { get; set; } = new();
    }
    public class CareerAiRecommendationsRequest
    {
        public string ProfileTitle { get; set; } = string.Empty;
        public string ProfileCode { get; set; } = string.Empty;
        public string Language { get; set; } = "en";
        public string Summary { get; set; } = string.Empty;

        public List<CareerAiAreaScore> AreaScores { get; set; } = new();
    }

    public class CareerAiAreaScore
    {
        public string Area { get; set; } = string.Empty;
        public string AreaCode { get; set; } = string.Empty;

        public int Score { get; set; }
        public int RawScore { get; set; }
        public int MaxScore { get; set; } = 10;

        public string Description { get; set; } = string.Empty;
    }

    public class CareerAiRecommendationsResponse
    {
        public string Summary { get; set; } = string.Empty;

        public List<string> CareerDirections { get; set; } = new();

        public List<CareerAiRecommendedRole> RecommendedRoles { get; set; } = new();

        public List<string> SkillsToDevelop { get; set; } = new();

        public List<string> NextSteps { get; set; } = new();

        public string Disclaimer { get; set; } = string.Empty;

        public bool IsMock { get; set; }
    }

    public class CareerAiRecommendedRole
    {
        public string Title { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }
}
