namespace JobCv.Mobile.Models
{
    public class InterviewPrepResponse
    {
        public string JobTitle { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;

        public List<string> KeySkills { get; set; } = new();
        public List<InterviewQuestionDto> Questions { get; set; } = new();
        public List<string> BeforeInterviewTips { get; set; } = new();

        public bool IsMock { get; set; }
    }
}