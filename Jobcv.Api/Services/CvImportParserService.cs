using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using JobCv.Api.Dtos;

namespace JobCv.Api.Services
{
    public interface ICvImportParserService
    {
        Task<ParsedCvImportDto> ParseAsync(string rawText, bool useAi);
    }

    public class CvImportParserService : ICvImportParserService
    {
        private const string ExportStartMarker = "JOBCV_EXPORT_V1_START";
        private const string ExportEndMarker = "JOBCV_EXPORT_V1_END";

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private static readonly string[] SummaryHeaders =
        {
            "professional summary", "career summary", "summary", "profile", "professional profile", "about", "about me", "objective", "career objective",
            "personal statement", "overview", "profil", "despre mine", "rezumat", "obiectiv"
        };

        private static readonly string[] SkillsHeaders =
        {
            "skills", "technical skills", "core skills", "key skills", "professional skills", "competencies", "areas of expertise", "expertise",
            "technologies", "technology", "tech stack", "tools", "programming languages", "software", "hard skills",
            "competente", "competențe", "abilitati", "abilități", "aptitudini", "tehnologii"
        };

        private static readonly string[] ExperienceHeaders =
        {
            "experience", "work experience", "professional experience", "employment", "employment history", "work history", "career history",
            "professional background", "relevant experience", "internship experience", "internships", "job experience",
            "experienta", "experiență", "experienta profesionala", "experiență profesională", "istoric profesional"
        };

        private static readonly string[] EducationHeaders =
        {
            "education", "studies", "academic", "academic background", "education and training", "qualifications", "training",
            "educatie", "educație", "studii", "formare", "calificari", "calificări"
        };

        private static readonly string[] ProjectHeaders =
        {
            "projects", "project experience", "personal projects", "academic projects", "selected projects", "portfolio projects", "software projects",
            "proiecte", "proiecte personale", "portofoliu", "portfolio"
        };

        private static readonly string[] LanguageHeaders =
        {
            "languages", "language skills", "foreign languages", "limbi", "limbi straine", "limbi străine", "competente lingvistice", "competențe lingvistice"
        };

        private static readonly string[] CertificationHeaders =
        {
            "certifications", "certificates", "licenses", "licences", "courses", "training courses", "awards", "achievements",
            "certificari", "certificări", "certificate", "cursuri", "diplome", "licente", "licențe"
        };

        private static readonly string[] ContactHeaders =
        {
            "contact", "contact information", "personal information", "details", "links", "social links", "online profiles"
        };

        private static readonly string[] AllHeaders = SummaryHeaders
            .Concat(SkillsHeaders)
            .Concat(ExperienceHeaders)
            .Concat(EducationHeaders)
            .Concat(ProjectHeaders)
            .Concat(LanguageHeaders)
            .Concat(CertificationHeaders)
            .Concat(ContactHeaders)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        public Task<ParsedCvImportDto> ParseAsync(string rawText, bool useAi)
        {
            var cleanText = NormalizeText(rawText);

            if (string.IsNullOrWhiteSpace(cleanText))
                return Task.FromResult(new ParsedCvImportDto());

            var embedded = TryParseEmbeddedExport(cleanText);
            if (embedded != null)
                return Task.FromResult(embedded);

            return Task.FromResult(ParseVisibleText(cleanText));
        }

        private static ParsedCvImportDto? TryParseEmbeddedExport(string text)
        {
            try
            {
                var compactText = Regex.Replace(text, @"\s+", "");

                var startIndex = compactText.IndexOf(ExportStartMarker, StringComparison.Ordinal);
                if (startIndex < 0)
                    return null;

                startIndex += ExportStartMarker.Length;

                var endIndex = compactText.IndexOf(ExportEndMarker, startIndex, StringComparison.Ordinal);
                if (endIndex <= startIndex)
                    return null;

                var base64 = compactText[startIndex..endIndex];
                var json = Encoding.UTF8.GetString(Convert.FromBase64String(base64));

                var parsed = JsonSerializer.Deserialize<ParsedCvImportDto>(json, JsonOptions);
                return parsed ?? null;
            }
            catch
            {
                return null;
            }
        }

        private static ParsedCvImportDto ParseVisibleText(string text)
        {
            var lines = SplitUsefulLines(text, 220);

            var result = new ParsedCvImportDto
            {
                Email = FindEmail(text),
                Phone = FindPhone(text),
                LinkedInUrl = FindUrl(text, "linkedin.com"),
                GitHubUrl = FindUrl(text, "github.com"),
                PortfolioUrl = FindPortfolioUrl(text),
                FullName = FindName(lines),
                Location = FindLocation(lines),
                Summary = Limit(ExtractSection(text, SummaryHeaders), 900)
            };

            result.Skills = ParseSkills(ExtractSection(text, SkillsHeaders));
            result.Experiences = ParseExperiences(ExtractSection(text, ExperienceHeaders));
            result.Educations = ParseEducations(ExtractSection(text, EducationHeaders));
            result.Projects = ParseProjects(ExtractSection(text, ProjectHeaders), result.GitHubUrl, result.PortfolioUrl);
            result.Languages = ParseLanguages(ExtractSection(text, LanguageHeaders));
            result.Certifications = ParseCertifications(ExtractSection(text, CertificationHeaders));

            return result;
        }

        private static string NormalizeText(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var normalized = value.Replace("\r\n", "\n").Replace("\r", "\n");
            normalized = Regex.Replace(normalized, "[\t]+", " ");
            normalized = Regex.Replace(normalized, @"[ ]{2,}", " ");
            normalized = Regex.Replace(normalized, "\n{4,}", "\n\n");
            return normalized.Trim();
        }

        private static List<string> SplitUsefulLines(string text, int take)
        {
            return text.Split('\n')
                .Select(x => CleanLine(x))
                .Where(x => x.Length > 0 && x.Length <= 180)
                .Take(take)
                .ToList();
        }

        private static string FindEmail(string text)
        {
            var match = Regex.Match(text, @"(?<![\w.%+-])([A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,})(?![A-Z])", RegexOptions.IgnoreCase);
            if (!match.Success)
                return string.Empty;

            return match.Groups[1].Value.Trim().Trim('.', ',', ';', ':', ')', ']');
        }

        private static string FindPhone(string text)
        {
            var textWithoutEmailAndUrls = RemoveEmailsAndUrls(text);
            var candidates = new List<(string Value, int Score)>();

            foreach (var line in textWithoutEmailAndUrls.Split('\n').Select(CleanLine).Where(x => x.Length > 0).Take(120))
            {
                foreach (Match match in Regex.Matches(line, @"(?<!\d)(?:\+|00)?\d[\d\s().\-/]{6,}\d(?!\d)"))
                {
                    var value = CleanPhone(match.Value);
                    if (!IsLikelyPhone(value))
                        continue;

                    var score = 0;
                    if (LineContainsAny(line, "phone", "mobile", "tel", "telephone", "contact", "telefon", "mobil")) score += 30;
                    if (value.TrimStart().StartsWith("+")) score += 15;
                    if (Regex.IsMatch(value, @"(^|\D)0\d")) score += 8;
                    if (value.Count(char.IsDigit) is >= 10 and <= 12) score += 6;

                    candidates.Add((value, score));
                }
            }

            return candidates
                .OrderByDescending(x => x.Score)
                .ThenBy(x => Math.Abs(x.Value.Count(char.IsDigit) - 10))
                .Select(x => x.Value)
                .FirstOrDefault() ?? string.Empty;
        }

        private static string RemoveEmailsAndUrls(string text)
        {
            var withoutEmails = Regex.Replace(text, @"[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}", " ", RegexOptions.IgnoreCase);
            return Regex.Replace(withoutEmails, @"(?:https?://|www\.)\S+", " ", RegexOptions.IgnoreCase);
        }

        private static string CleanPhone(string value)
        {
            return Regex.Replace(value.Trim().Trim(',', ';', '.', ':', ')', ']'), @"\s+", " ");
        }

        private static bool IsLikelyPhone(string value)
        {
            var digits = new string(value.Where(char.IsDigit).ToArray());
            if (digits.Length is < 8 or > 15)
                return false;

            if (Regex.IsMatch(value, @"\b\d{1,2}[./-]\d{1,2}[./-]\d{2,4}\b"))
                return false;

            if (Regex.IsMatch(value, @"\b(?:19|20)\d{2}\s*[-–—/]\s*(?:19|20)\d{2}\b"))
                return false;

            if (Regex.Matches(value, @"(?:19|20)\d{2}").Count >= 2)
                return false;

            if (Regex.IsMatch(value, @"\b(?:jan|feb|mar|apr|may|jun|jul|aug|sep|oct|nov|dec|ian|feb|mar|apr|mai|iun|iul|aug|sep|oct|noi|dec)", RegexOptions.IgnoreCase))
                return false;

            if (!value.Contains('+') && !Regex.IsMatch(value, @"(^|\D)0\d") && digits.Length <= 9)
                return false;

            return true;
        }

        private static string FindUrl(string text, string domain)
        {
            var pattern = $@"(?<![@\w.-])(?:https?://)?(?:www\.)?[^\s,;<>\)\]]*{Regex.Escape(domain)}[^\s,;<>\)\]]*";
            var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
            return match.Success ? NormalizeUrl(match.Value) : string.Empty;
        }

        private static string FindPortfolioUrl(string text)
        {
            var explicitPortfolio = Regex.Match(text,
                @"(?:portfolio|portofoliu|website|site)\s*[:\-]?\s*((?:https?://)?(?:www\.)?[a-z0-9.-]+\.[a-z]{2,}(?:/[^\s,;<>\)\]]*)?)",
                RegexOptions.IgnoreCase);

            if (explicitPortfolio.Success)
                return NormalizeUrl(explicitPortfolio.Groups[1].Value);

            var matches = Regex.Matches(text,
                @"(?<![@\w.-])(?:https?://|www\.)[a-z0-9.-]+\.[a-z]{2,}(?:/[^\s,;<>\)\]]*)?",
                RegexOptions.IgnoreCase);

            foreach (Match match in matches)
            {
                var value = match.Value.Trim();

                if (value.Contains("linkedin.com", StringComparison.OrdinalIgnoreCase) ||
                    value.Contains("github.com", StringComparison.OrdinalIgnoreCase) ||
                    value.Contains("facebook.com", StringComparison.OrdinalIgnoreCase) ||
                    value.Contains("twitter.com", StringComparison.OrdinalIgnoreCase) ||
                    value.Contains("x.com", StringComparison.OrdinalIgnoreCase))
                    continue;

                return NormalizeUrl(value);
            }

            return string.Empty;
        }

        private static string NormalizeUrl(string value)
        {
            var url = value.Trim().TrimEnd('.', ',', ';', ')', ']');
            return url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                   url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                ? url
                : $"https://{url}";
        }

        private static string FindName(List<string> lines)
        {
            foreach (var line in lines.Take(35))
            {
                if (line.Contains('@') || Regex.IsMatch(line, @"\d"))
                    continue;

                if (IsAnyKnownHeader(line) || LineContainsAny(line,
                        "resume", "curriculum", "vitae", "phone", "email", "contact", "linkedin", "github", "portfolio",
                        "address", "location", "skills", "languages", "profile", "experience", "education", "projects", "certifications"))
                    continue;

                var cleaned = Regex.Replace(line, @"\s+", " ").Trim('-', '–', '—', '|', ' ');
                var words = cleaned.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (words.Length is >= 2 and <= 5 && words.All(w => w.All(ch => char.IsLetter(ch) || ch == '-' || ch == '\'')))
                    return ToTitleCase(cleaned);
            }

            return string.Empty;
        }

        private static string FindLocation(List<string> lines)
        {
            foreach (var line in lines.Take(70))
            {
                var normalized = NormalizeKey(line);

                if (normalized.StartsWith("location ") || normalized.StartsWith("address ") || normalized.StartsWith("city "))
                    return RemoveLeadingLabel(line, "location", "address", "city", "locatie", "locație", "adresa", "adresă");

                if (LineContainsAny(line, "Bucharest", "Bucuresti", "București", "Cluj", "Iasi", "Iași", "Timisoara", "Timișoara", "Romania", "Remote"))
                    return line;
            }

            return string.Empty;
        }

        private static string ExtractSection(string text, params string[] sectionNames)
        {
            var lines = text.Split('\n').Select(x => x.Trim()).ToList();
            var sectionLines = new List<string>();
            var started = false;

            foreach (var rawLine in lines)
            {
                var line = rawLine.Trim();

                if (!started)
                {
                    if (IsHeaderLine(line, sectionNames))
                    {
                        started = true;

                        var inline = GetInlineTextAfterHeader(line, sectionNames);
                        if (!string.IsNullOrWhiteSpace(inline))
                            sectionLines.Add(inline);
                    }

                    continue;
                }

                if (!string.IsNullOrWhiteSpace(line) && IsAnyKnownHeader(line))
                    break;

                sectionLines.Add(line);
            }

            return CleanSectionText(sectionLines);
        }

        private static string CleanSectionText(IEnumerable<string> lines)
        {
            var cleaned = new List<string>();
            var lastWasEmpty = false;

            foreach (var rawLine in lines)
            {
                var line = CleanLine(rawLine);

                if (string.IsNullOrWhiteSpace(line))
                {
                    if (!lastWasEmpty && cleaned.Count > 0)
                    {
                        cleaned.Add(string.Empty);
                        lastWasEmpty = true;
                    }

                    continue;
                }

                if (line.Contains(ExportStartMarker) || line.Contains(ExportEndMarker))
                    continue;

                cleaned.Add(line);
                lastWasEmpty = false;
            }

            while (cleaned.Count > 0 && string.IsNullOrWhiteSpace(cleaned[^1]))
                cleaned.RemoveAt(cleaned.Count - 1);

            return string.Join("\n", cleaned).Trim();
        }

        private static bool IsAnyKnownHeader(string line)
        {
            return IsHeaderLine(line, AllHeaders);
        }

        private static bool IsHeaderLine(string line, IEnumerable<string> headers)
        {
            var key = NormalizeKey(RemoveBulletPrefix(line));

            if (string.IsNullOrWhiteSpace(key) || key.Length > 90)
                return false;

            foreach (var header in headers.Select(NormalizeKey).Where(x => x.Length > 0).OrderByDescending(x => x.Length))
            {
                if (key.Equals(header, StringComparison.OrdinalIgnoreCase))
                    return true;

                if ((key.StartsWith(header + " ", StringComparison.OrdinalIgnoreCase) ||
                     key.EndsWith(" " + header, StringComparison.OrdinalIgnoreCase) ||
                     key.Contains(" " + header + " ", StringComparison.OrdinalIgnoreCase)) &&
                    key.Length <= header.Length + 28)
                    return true;
            }

            return false;
        }

        private static string GetInlineTextAfterHeader(string line, IEnumerable<string> headers)
        {
            var cleaned = RemoveBulletPrefix(line).Trim();

            foreach (var header in headers.OrderByDescending(x => x.Length))
            {
                var normalizedHeader = NormalizeKey(header);
                var normalizedLine = NormalizeKey(cleaned);

                if (!normalizedLine.StartsWith(normalizedHeader, StringComparison.OrdinalIgnoreCase))
                    continue;

                var separatorIndex = cleaned.IndexOf(':');
                if (separatorIndex < 0)
                    separatorIndex = cleaned.IndexOf('-');

                if (separatorIndex >= 0 && separatorIndex < cleaned.Length - 1)
                    return cleaned[(separatorIndex + 1)..].Trim();
            }

            return string.Empty;
        }

        private static List<string> ParseSkills(string section)
        {
            if (string.IsNullOrWhiteSpace(section))
                return new List<string>();

            var values = new List<string>();

            foreach (var rawLine in section.Split('\n'))
            {
                var line = RemoveLeadingLabel(CleanLine(rawLine), SkillsHeaders);
                if (string.IsNullOrWhiteSpace(line) || IsAnyKnownHeader(line))
                    continue;

                var parts = Regex.Split(line, @"[,;|•·]+")
                    .SelectMany(x => Regex.Split(x, @"\s{2,}"))
                    .Select(x => x.Trim().Trim('-', '–', '—', '•', '·'));

                foreach (var part in parts)
                {
                    var item = Regex.Replace(part, @"\s+", " ").Trim();

                    if (item.Length is < 2 or > 55)
                        continue;

                    if (IsLikelyDateText(item) || item.Contains('@') || item.Contains("http", StringComparison.OrdinalIgnoreCase))
                        continue;

                    values.Add(item);
                }
            }

            return CleanList(values, 50);
        }

        private static List<ParsedExperienceDto> ParseExperiences(string section)
        {
            var blocks = SplitIntoBlocks(section).Take(10).ToList();
            var result = new List<ParsedExperienceDto>();

            foreach (var block in blocks)
            {
                var lines = block.Where(x => !string.IsNullOrWhiteSpace(x)).Select(CleanLine).ToList();
                if (lines.Count == 0)
                    continue;

                var dateLine = lines.FirstOrDefault(ContainsDateRange) ?? string.Empty;
                var dateRange = ParseDateRange(dateLine);
                var contentLines = lines.Where(x => !x.Equals(dateLine, StringComparison.OrdinalIgnoreCase)).ToList();

                var firstUseful = contentLines.FirstOrDefault(x => !LooksLikeLongDescription(x)) ?? contentLines.FirstOrDefault() ?? string.Empty;
                var secondUseful = contentLines.SkipWhile(x => x != firstUseful).Skip(1).FirstOrDefault(x => !LooksLikeLongDescription(x)) ?? string.Empty;

                var jobTitle = RemoveLeadingLabel(firstUseful, "role", "title", "position", "job title", "functie", "funcție", "pozitie", "poziție");
                var company = RemoveLeadingLabel(secondUseful, "company", "employer", "organization", "organisation", "firma", "companie", "angajator");

                SplitTitleAndCompany(ref jobTitle, ref company);

                var descriptionLines = contentLines
                    .Where(x => !x.Equals(firstUseful, StringComparison.OrdinalIgnoreCase) && !x.Equals(secondUseful, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (descriptionLines.Count == 0 && contentLines.Count > 0)
                    descriptionLines = contentLines.Skip(Math.Min(2, contentLines.Count)).ToList();

                var description = Limit(string.Join("\n", descriptionLines), 1800);

                if (string.IsNullOrWhiteSpace(jobTitle) && string.IsNullOrWhiteSpace(company))
                    jobTitle = "Work experience imported from PDF";

                if (string.IsNullOrWhiteSpace(description))
                    description = Limit(string.Join("\n", contentLines), 1800);

                if (string.IsNullOrWhiteSpace(description) && string.IsNullOrWhiteSpace(company))
                    continue;

                result.Add(new ParsedExperienceDto
                {
                    JobTitle = Limit(jobTitle, 120),
                    Company = Limit(company, 120),
                    Description = description,
                    StartDate = dateRange.Start,
                    EndDate = dateRange.End,
                    IsCurrent = dateRange.IsCurrent
                });
            }

            return result;
        }

        private static List<ParsedEducationDto> ParseEducations(string section)
        {
            var blocks = SplitIntoBlocks(section).Take(10).ToList();
            var result = new List<ParsedEducationDto>();

            foreach (var block in blocks)
            {
                var lines = block.Where(x => !string.IsNullOrWhiteSpace(x)).Select(CleanLine).ToList();
                if (lines.Count == 0)
                    continue;

                var dateLine = lines.FirstOrDefault(ContainsDateRange) ?? lines.FirstOrDefault(IsLikelyDateText) ?? string.Empty;
                var dateRange = ParseDateRange(dateLine);
                var contentLines = lines.Where(x => !x.Equals(dateLine, StringComparison.OrdinalIgnoreCase)).ToList();

                var institution = contentLines.FirstOrDefault() ?? string.Empty;
                var degree = contentLines.Skip(1).FirstOrDefault() ?? string.Empty;

                if (LooksLikeDegree(institution) && !LooksLikeDegree(degree) && !string.IsNullOrWhiteSpace(degree))
                    (institution, degree) = (degree, institution);

                var description = Limit(string.Join("\n", contentLines.Skip(2)), 1000);

                if (string.IsNullOrWhiteSpace(institution) && string.IsNullOrWhiteSpace(degree))
                    institution = "Education imported from PDF";

                result.Add(new ParsedEducationDto
                {
                    Institution = Limit(institution, 140),
                    Degree = Limit(degree, 140),
                    Description = description,
                    StartDate = dateRange.Start,
                    EndDate = dateRange.End
                });
            }

            return result;
        }

        private static List<ParsedProjectDto> ParseProjects(string section, string gitHubUrl, string portfolioUrl)
        {
            var blocks = SplitIntoBlocks(section).Take(12).ToList();
            var result = new List<ParsedProjectDto>();

            foreach (var block in blocks)
            {
                var lines = block.Where(x => !string.IsNullOrWhiteSpace(x)).Select(CleanLine).ToList();
                if (lines.Count == 0)
                    continue;

                var title = lines.FirstOrDefault(x => !LooksLikeLongDescription(x) && !ContainsUrl(x)) ?? lines.First();
                var projectUrl = FindFirstUrl(string.Join("\n", lines));
                var github = projectUrl.Contains("github.com", StringComparison.OrdinalIgnoreCase) ? projectUrl : string.Empty;

                var technologies = lines.FirstOrDefault(x => LineContainsAny(x, "technologies", "technology", "tech stack", "tools", "stack", "tehnologii")) ?? string.Empty;
                technologies = RemoveLeadingLabel(technologies, "technologies", "technology", "tech stack", "tools", "stack", "tehnologii");

                var description = Limit(string.Join("\n", lines.Where(x => !x.Equals(title, StringComparison.OrdinalIgnoreCase) && !x.Equals(technologies, StringComparison.OrdinalIgnoreCase))), 1200);

                result.Add(new ParsedProjectDto
                {
                    Title = Limit(title, 140),
                    Description = description,
                    Technologies = Limit(technologies, 300),
                    ProjectUrl = projectUrl.Contains("github.com", StringComparison.OrdinalIgnoreCase) ? string.Empty : projectUrl,
                    GitHubUrl = github
                });
            }

            if (result.Count == 0 && (!string.IsNullOrWhiteSpace(gitHubUrl) || !string.IsNullOrWhiteSpace(portfolioUrl)))
            {
                result.Add(new ParsedProjectDto
                {
                    Title = "Portfolio imported from PDF",
                    Description = string.Empty,
                    ProjectUrl = portfolioUrl,
                    GitHubUrl = gitHubUrl
                });
            }

            return result;
        }

        private static List<ParsedLanguageDto> ParseLanguages(string section)
        {
            if (string.IsNullOrWhiteSpace(section))
                return new List<ParsedLanguageDto>();

            var result = new List<ParsedLanguageDto>();

            foreach (var raw in Regex.Split(section, @"[\n;•·]+"))
            {
                var line = CleanLine(raw);
                if (string.IsNullOrWhiteSpace(line) || IsAnyKnownHeader(line))
                    continue;

                var parts = Regex.Split(line, @"[:\-–—,|()]")
                    .Select(x => x.Trim())
                    .Where(x => x.Length > 0)
                    .ToList();

                if (parts.Count == 0)
                    continue;

                result.Add(new ParsedLanguageDto
                {
                    Name = Limit(parts[0], 80),
                    Level = Limit(parts.Count > 1 ? parts[1] : string.Empty, 80)
                });
            }

            return result
                .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                .GroupBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .Select(x => x.First())
                .Take(12)
                .ToList();
        }

        private static List<ParsedCertificationDto> ParseCertifications(string section)
        {
            var blocks = SplitIntoBlocks(section).Take(20).ToList();
            var result = new List<ParsedCertificationDto>();

            foreach (var block in blocks)
            {
                var lines = block.Where(x => !string.IsNullOrWhiteSpace(x)).Select(CleanLine).ToList();
                if (lines.Count == 0)
                    continue;

                var dateLine = lines.FirstOrDefault(IsLikelyDateText) ?? string.Empty;
                var name = lines.FirstOrDefault(x => !x.Equals(dateLine, StringComparison.OrdinalIgnoreCase)) ?? lines.First();
                var issuer = lines.SkipWhile(x => x != name).Skip(1).FirstOrDefault(x => !x.Equals(dateLine, StringComparison.OrdinalIgnoreCase)) ?? string.Empty;
                var url = FindFirstUrl(string.Join("\n", lines));

                result.Add(new ParsedCertificationDto
                {
                    Name = Limit(name, 160),
                    Issuer = Limit(issuer, 120),
                    Date = ParseLooseDate(dateLine),
                    Url = url
                });
            }

            return result;
        }

        private static List<List<string>> SplitIntoBlocks(string section)
        {
            var blocks = new List<List<string>>();
            var current = new List<string>();

            foreach (var raw in section.Split('\n'))
            {
                var line = CleanLine(raw);

                if (string.IsNullOrWhiteSpace(line))
                {
                    AddBlockIfUseful(blocks, current);
                    current = new List<string>();
                    continue;
                }

                if (current.Count >= 4 && LooksLikeNewBlockStart(line))
                {
                    AddBlockIfUseful(blocks, current);
                    current = new List<string>();
                }

                current.Add(line);
            }

            AddBlockIfUseful(blocks, current);

            if (blocks.Count == 0 && !string.IsNullOrWhiteSpace(section))
            {
                var lines = section.Split('\n').Select(CleanLine).Where(x => x.Length > 0).ToList();
                AddBlockIfUseful(blocks, lines);
            }

            return blocks;
        }

        private static void AddBlockIfUseful(List<List<string>> blocks, List<string> current)
        {
            var cleaned = current.Select(CleanLine).Where(x => x.Length > 0).ToList();
            if (cleaned.Count > 0)
                blocks.Add(cleaned);
        }

        private static bool LooksLikeNewBlockStart(string line)
        {
            if (line.Length > 95 || ContainsUrl(line) || line.Contains('@') || line.EndsWith('.') || IsLikelyDateText(line))
                return false;

            if (LineContainsAny(line, "responsible", "developed", "managed", "created", "implemented", "worked", "collaborated", "using", "with"))
                return false;

            return true;
        }

        private static bool LooksLikeLongDescription(string line)
        {
            return line.Length > 100 || line.StartsWith("-") || line.StartsWith("•") || line.EndsWith(".");
        }

        private static void SplitTitleAndCompany(ref string title, ref string company)
        {
            if (string.IsNullOrWhiteSpace(title))
                return;

            var atMatch = Regex.Match(title, @"^(.+?)\s+(?:at|@)\s+(.+)$", RegexOptions.IgnoreCase);
            if (atMatch.Success)
            {
                title = atMatch.Groups[1].Value.Trim();
                if (string.IsNullOrWhiteSpace(company))
                    company = atMatch.Groups[2].Value.Trim();
                return;
            }

            var dashMatch = Regex.Match(title, @"^(.+?)\s+[-–—]\s+(.+)$");
            if (dashMatch.Success && string.IsNullOrWhiteSpace(company))
            {
                title = dashMatch.Groups[1].Value.Trim();
                company = dashMatch.Groups[2].Value.Trim();
            }
        }

        private static bool LooksLikeDegree(string value)
        {
            return LineContainsAny(value, "bachelor", "master", "phd", "degree", "diploma", "licence", "license", "engineer", "computer science", "high school", "university", "msc", "bsc", "ba", "ma");
        }

        private static bool ContainsDateRange(string value)
        {
            return Regex.IsMatch(value, @"(?i)(?:\b(?:jan|feb|mar|apr|may|jun|jul|aug|sep|sept|oct|nov|dec|ian|feb|mar|apr|mai|iun|iul|aug|sep|oct|noi|dec)[a-zăâîșț]*\s+)?(?:19|20)\d{2}\s*(?:-|–|—|to|until|present|current|now)\s*(?:(?:\b(?:jan|feb|mar|apr|may|jun|jul|aug|sep|sept|oct|nov|dec|ian|feb|mar|apr|mai|iun|iul|aug|sep|oct|noi|dec)[a-zăâîșț]*\s+)?(?:19|20)\d{2}|present|current|now)") ||
                   Regex.IsMatch(value, @"\b\d{1,2}/(?:19|20)\d{2}\s*(?:-|–|—|to)\s*(?:\d{1,2}/(?:19|20)\d{2}|present|current|now)\b", RegexOptions.IgnoreCase);
        }

        private static bool IsLikelyDateText(string value)
        {
            return ContainsDateRange(value) ||
                   Regex.IsMatch(value, @"\b(?:19|20)\d{2}\b") ||
                   Regex.IsMatch(value, @"(?i)\b(?:jan|feb|mar|apr|may|jun|jul|aug|sep|sept|oct|nov|dec|ian|feb|mar|apr|mai|iun|iul|aug|sep|oct|noi|dec)[a-zăâîșț]*\s+(?:19|20)\d{2}\b");
        }

        private static (DateTime? Start, DateTime? End, bool IsCurrent) ParseDateRange(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return (null, null, false);

            var match = Regex.Match(value,
                @"(?i)(?<start>(?:\b(?:jan|feb|mar|apr|may|jun|jul|aug|sep|sept|oct|nov|dec|ian|feb|mar|apr|mai|iun|iul|aug|sep|oct|noi|dec)[a-zăâîșț]*\s+)?(?:19|20)\d{2}|\d{1,2}/(?:19|20)\d{2})\s*(?:-|–|—|to|until)\s*(?<end>(?:\b(?:jan|feb|mar|apr|may|jun|jul|aug|sep|sept|oct|nov|dec|ian|feb|mar|apr|mai|iun|iul|aug|sep|oct|noi|dec)[a-zăâîșț]*\s+)?(?:19|20)\d{2}|\d{1,2}/(?:19|20)\d{2}|present|current|now)");

            if (!match.Success)
            {
                var single = ParseLooseDate(value);
                return (single, null, false);
            }

            var start = ParseLooseDate(match.Groups["start"].Value);
            var isCurrent = Regex.IsMatch(match.Groups["end"].Value, @"(?i)present|current|now");
            var end = isCurrent ? null : ParseLooseDate(match.Groups["end"].Value);

            return (start, end, isCurrent);
        }

        private static DateTime? ParseLooseDate(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var text = NormalizeKey(value);
            var yearMatch = Regex.Match(text, @"\b(19|20)\d{2}\b");
            if (!yearMatch.Success)
                return null;

            var year = int.Parse(yearMatch.Value, CultureInfo.InvariantCulture);
            var month = 1;

            var monthMap = new Dictionary<string, int>
            {
                ["jan"] = 1,
                ["january"] = 1,
                ["ian"] = 1,
                ["ianuarie"] = 1,
                ["feb"] = 2,
                ["february"] = 2,
                ["februarie"] = 2,
                ["mar"] = 3,
                ["march"] = 3,
                ["martie"] = 3,
                ["apr"] = 4,
                ["april"] = 4,
                ["aprilie"] = 4,
                ["may"] = 5,
                ["mai"] = 5,
                ["jun"] = 6,
                ["june"] = 6,
                ["iun"] = 6,
                ["iunie"] = 6,
                ["jul"] = 7,
                ["july"] = 7,
                ["iul"] = 7,
                ["iulie"] = 7,
                ["aug"] = 8,
                ["august"] = 8,
                ["sep"] = 9,
                ["sept"] = 9,
                ["september"] = 9,
                ["septembrie"] = 9,
                ["oct"] = 10,
                ["october"] = 10,
                ["octombrie"] = 10,
                ["nov"] = 11,
                ["november"] = 11,
                ["noi"] = 11,
                ["noiembrie"] = 11,
                ["dec"] = 12,
                ["december"] = 12,
                ["decembrie"] = 12
            };

            foreach (var item in monthMap)
            {
                if (Regex.IsMatch(text, $@"\b{Regex.Escape(item.Key)}\w*\b", RegexOptions.IgnoreCase))
                {
                    month = item.Value;
                    break;
                }
            }

            var numericMonth = Regex.Match(value, @"\b(?<month>\d{1,2})/(?<year>(?:19|20)\d{2})\b");
            if (numericMonth.Success && int.TryParse(numericMonth.Groups["month"].Value, out var parsedMonth) && parsedMonth is >= 1 and <= 12)
                month = parsedMonth;

            return new DateTime(year, month, 1);
        }

        private static string FindFirstUrl(string text)
        {
            var match = Regex.Match(text, @"(?<![@\w.-])(?:https?://|www\.)[a-z0-9.-]+\.[a-z]{2,}(?:/[^\s,;<>\)\]]*)?", RegexOptions.IgnoreCase);
            return match.Success ? NormalizeUrl(match.Value) : string.Empty;
        }

        private static bool ContainsUrl(string value)
        {
            return Regex.IsMatch(value, @"(?:https?://|www\.|\.com|\.net|\.org|\.io)", RegexOptions.IgnoreCase);
        }

        private static string RemoveLeadingLabel(string value, params string[] labels)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var result = value.Trim();

            foreach (var label in labels.OrderByDescending(x => x.Length))
            {
                result = Regex.Replace(result, $@"^\s*{Regex.Escape(label)}\s*[:\-–—|]?\s*", string.Empty, RegexOptions.IgnoreCase).Trim();
            }

            return result;
        }

        private static string CleanLine(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var cleaned = Regex.Replace(value.Trim(), @"\s+", " ");
            return RemoveBulletPrefix(cleaned).Trim();
        }

        private static string RemoveBulletPrefix(string value)
        {
            return value.Trim().TrimStart('•', '·', '-', '–', '—', '*', '▪', '◦').Trim();
        }

        private static string NormalizeKey(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var formD = value.Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder();

            foreach (var ch in formD)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                    builder.Append(ch);
            }

            var noDiacritics = builder.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
            noDiacritics = Regex.Replace(noDiacritics, @"[^\p{L}\p{N}]+", " ");
            return Regex.Replace(noDiacritics, @"\s+", " ").Trim();
        }

        private static List<string> CleanList(IEnumerable<string> values, int maxCount)
        {
            return values
                .Select(x => Regex.Replace(x.Trim(), @"\s+", " "))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(maxCount)
                .ToList();
        }

        private static bool LineContainsAny(string line, params string[] values)
        {
            return values.Any(value => line.Contains(value, StringComparison.OrdinalIgnoreCase));
        }

        private static string Limit(string? value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var trimmed = value.Trim();
            return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
        }

        private static string ToTitleCase(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            return string.Join(' ', value.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(word => word.Length <= 1
                    ? word.ToUpperInvariant()
                    : char.ToUpperInvariant(word[0]) + word[1..].ToLowerInvariant()));
        }
    }
}
