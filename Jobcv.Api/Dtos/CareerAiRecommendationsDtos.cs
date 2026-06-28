namespace JobCv.Api.Dtos
{
    public class CareerAiRecommendationsRequestDto
    {
        public string ProfileTitle { get; set; } = string.Empty;
        public string ProfileCode { get; set; } = string.Empty;
        public string Language { get; set; } = "en";
        public string Summary { get; set; } = string.Empty;

        public List<CareerAiAreaScoreDto> AreaScores { get; set; } = new();
    }

    public class CareerAiAreaScoreDto
    {
        public string Area { get; set; } = string.Empty;
        public string AreaCode { get; set; } = string.Empty;

        public int Score { get; set; }
        public int RawScore { get; set; }
        public int MaxScore { get; set; } = 10;

        public string Description { get; set; } = string.Empty;
    }

    public class CareerAiRecommendationsResponseDto
    {
        public string Summary { get; set; } = string.Empty;

        public List<string> CareerDirections { get; set; } = new();

        public List<CareerAiRecommendedRoleDto> RecommendedRoles { get; set; } = new();

        public List<string> SkillsToDevelop { get; set; } = new();

        public List<string> NextSteps { get; set; } = new();

        public string Disclaimer { get; set; } = string.Empty;

        public bool IsMock { get; set; }
    }

    public class CareerAiRecommendedRoleDto
    {
        public string Title { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }
}