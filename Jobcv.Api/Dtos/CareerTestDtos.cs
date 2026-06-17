namespace JobCv.Api.Dtos
{
    public class CareerTestPdfRequestDto
    {
        public string ProfileTitle { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        public List<CareerAreaScoreDto> AreaScores { get; set; } = new();
        public List<string> RecommendedRoles { get; set; } = new();
        public List<string> NextSteps { get; set; } = new();
    }

    public class CareerAreaScoreDto
    {
        public string Area { get; set; } = string.Empty;
        public int Score { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}