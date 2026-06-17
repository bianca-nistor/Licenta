namespace JobCv.Mobile.Models
{
    public class CvTailoringResponse
    {
        public string JobTitle { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;

        public string TailoredProfileSummary { get; set; } = string.Empty;

        public List<string> ImportantKeywords { get; set; } = new();
        public List<string> SkillsToHighlight { get; set; } = new();
        public List<string> ExperienceToEmphasize { get; set; } = new();
        public string CvQualityWarning { get; set; } = string.Empty;

        public List<CvTailoringSuggestionDto> Suggestions { get; set; } = new();

        public bool IsMock { get; set; }
    }
}