namespace JobCv.Mobile.Models
{
    public class CareerTestQuestion
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public string Area { get; set; } = string.Empty;
    }

    public class CareerAreaScore
    {
        public string Area { get; set; } = string.Empty;
        public int Score { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class CareerTestResult
    {
        public string ProfileTitle { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; } = DateTime.Now;

        public List<CareerAreaScore> AreaScores { get; set; } = new();
        public List<string> RecommendedRoles { get; set; } = new();
        public List<string> NextSteps { get; set; } = new();
    }

    public class CareerTestPdfRequest
    {
        public string ProfileTitle { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        public List<CareerAreaScore> AreaScores { get; set; } = new();
        public List<string> RecommendedRoles { get; set; } = new();
        public List<string> NextSteps { get; set; } = new();
    }
}