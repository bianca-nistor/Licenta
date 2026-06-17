namespace JobCv.Api.Dtos
{
    public class CvQualityCheckResponseDto
    {
        public int Score { get; set; }

        public string CompletenessLevel { get; set; } = string.Empty;

        public string Summary { get; set; } = string.Empty;

        public List<string> Strengths { get; set; } = new();

        public List<CvQualityIssueDto> Issues { get; set; } = new();

        public List<string> Suggestions { get; set; } = new();

        public bool IsMock { get; set; }
    }
}
