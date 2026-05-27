namespace JobCv.Mobile.Models
{
    public class CvSelectionOption
    {
        public string SourceType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public string CvText { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"{Title} - {Subtitle}";
        }
    }
}