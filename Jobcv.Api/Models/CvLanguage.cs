namespace JobCv.Api.Models
{
    public class CvLanguage
    {
        public int Id { get; set; }

        public int CvId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Level { get; set; } = string.Empty;

        public Cv? Cv { get; set; }
    }
}