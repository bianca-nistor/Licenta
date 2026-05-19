namespace JobCv.Api.Models
{
    public class Application
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int JobId { get; set; }

        public int? CvId { get; set; }

        public string Status { get; set; } = "Saved";
        // Saved / Applied / InterviewScheduled / InterviewCompleted / Rejected / Accepted / OfferReceived

        public DateTime? AppliedAt { get; set; }

        public DateTime? InterviewDate { get; set; }

        public string Notes { get; set; } = string.Empty;

        public string InterviewNotes { get; set; } = string.Empty;

        public string QuestionsAsked { get; set; } = string.Empty;

        public int? InterviewRating { get; set; }
        // 1-5: cât de bine crede utilizatorul că s-a descurcat

        public int? InterviewDifficulty { get; set; }
        // 1-5: cât de greu a fost interviul

        public bool? WouldApplyAgain { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public User? User { get; set; }

        public Job? Job { get; set; }

        public Cv? Cv { get; set; }
    }
}