namespace JobCv.Api.Dtos
{
    public class InterviewQuestionDto
    {
        public string Category { get; set; } = string.Empty;
        public string Question { get; set; } = string.Empty;
        public string SuggestedAnswer { get; set; } = string.Empty;
    }
}