namespace JobCv.Api.Dtos
{
    public class ParsedCvImportDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string LinkedInUrl { get; set; } = string.Empty;
        public string GitHubUrl { get; set; } = string.Empty;
        public string PortfolioUrl { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;

        public List<string> Skills { get; set; } = new();
        public List<ParsedExperienceDto> Experiences { get; set; } = new();
        public List<ParsedEducationDto> Educations { get; set; } = new();
        public List<ParsedProjectDto> Projects { get; set; } = new();
        public List<ParsedLanguageDto> Languages { get; set; } = new();
        public List<ParsedCertificationDto> Certifications { get; set; } = new();
    }

    public class ParsedExperienceDto
    {
        public string JobTitle { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsCurrent { get; set; }
    }

    public class ParsedEducationDto
    {
        public string Institution { get; set; } = string.Empty;
        public string Degree { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class ParsedProjectDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Technologies { get; set; } = string.Empty;
        public string ProjectUrl { get; set; } = string.Empty;
        public string GitHubUrl { get; set; } = string.Empty;
    }

    public class ParsedLanguageDto
    {
        public string Name { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
    }

    public class ParsedCertificationDto
    {
        public string Name { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public DateTime? Date { get; set; }
        public string Url { get; set; } = string.Empty;
    }
}