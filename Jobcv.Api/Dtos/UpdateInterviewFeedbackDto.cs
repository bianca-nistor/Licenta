namespace JobCv.Api.Dtos
{
    public class UpdateInterviewFeedbackDto
    {
        public DateTime? InterviewDate { get; set; }

        public string InterviewNotes { get; set; } = string.Empty;

        public string QuestionsAsked { get; set; } = string.Empty;

        public int? InterviewRating { get; set; }

        public int? InterviewDifficulty { get; set; }

        public bool? WouldApplyAgain { get; set; }
    }
}