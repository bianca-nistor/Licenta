using JobCv.Mobile.Services;
namespace JobCv.Mobile.Models
{
    public class CvDto
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Language { get; set; } = string.Empty;

        public string Summary { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public string LinkedInUrl { get; set; } = string.Empty;

        public string GitHubUrl { get; set; } = string.Empty;

        public string PortfolioUrl { get; set; } = string.Empty;

        public string TemplateName { get; set; } = string.Empty;

        public bool IsBaseCv { get; set; }

        public int? ParentCvId { get; set; }

        public int? TargetJobId { get; set; }

        public DateTime CreatedAt { get; set; }

        public List<CvSkillDto> Skills { get; set; } = new();

        public List<CvEducationDto> Educations { get; set; } = new();

        public List<CvExperienceDto> Experiences { get; set; } = new();

        public List<CvProjectDto> Projects { get; set; } = new();

        public List<CvLanguageDto> Languages { get; set; } = new();

        public List<CvCertificationDto> Certifications { get; set; } = new();
        public string PhotoFileName { get; set; } = string.Empty;

        public bool HasPhoto { get; set; }

        public string LanguageDisplay
        {
            get
            {
                var cvLanguage = Language?.Trim().ToLowerInvariant();
                var appLanguage = LanguageService.CurrentLanguage?.Trim().ToLowerInvariant();

                return cvLanguage switch
                {
                    "ro" => appLanguage == "ro" ? "Română" : "Romanian",
                    "en" => appLanguage == "ro" ? "Engleză" : "English",
                    _ => string.IsNullOrWhiteSpace(Language) ? "-" : Language
                };
            }
        }
    }
}