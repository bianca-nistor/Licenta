using JobCv.Api.Dtos;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace JobCv.Api.Services
{
    public class AdzunaJobSearchService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public AdzunaJobSearchService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<List<JobSearchResultDto>> SearchJobsAsync(
            string? query,
            string? location,
            int page = 1,
            int pageSize = 20)
        {
            var appId = _configuration["Adzuna:AppId"];
            var appKey = _configuration["Adzuna:AppKey"];
            var country = _configuration["Adzuna:Country"] ?? "gb";

            if (string.IsNullOrWhiteSpace(appId))
                throw new Exception("Adzuna AppId is missing.");

            if (string.IsNullOrWhiteSpace(appKey))
                throw new Exception("Adzuna AppKey is missing.");

            if (appId.Contains("PASTE") || appKey.Contains("PASTE"))
                throw new Exception("Adzuna AppId or AppKey still contains placeholder text.");
            try
            {
                if (page < 1)
                    page = 1;

                if (pageSize < 1)
                    pageSize = 20;

                var encodedQuery = Uri.EscapeDataString(query ?? "");
                var encodedLocation = Uri.EscapeDataString(location ?? "");

                var url =
                    $"https://api.adzuna.com/v1/api/jobs/{country}/search/{page}" +
                    $"?app_id={Uri.EscapeDataString(appId)}" +
                    $"&app_key={Uri.EscapeDataString(appKey)}" +
                    $"&results_per_page={pageSize}" +
                    $"&what={encodedQuery}" +
                    $"&where={encodedLocation}" +
                    $"&content-type=application/json";

                var httpResponse = await _httpClient.GetAsync(url);

                var bytes = await httpResponse.Content.ReadAsByteArrayAsync();
                var json = Encoding.UTF8.GetString(bytes);

                if (!httpResponse.IsSuccessStatusCode)
                {
                    throw new Exception($"Adzuna returned status {(int)httpResponse.StatusCode}: {json}");
                }
                var response = JsonSerializer.Deserialize<AdzunaSearchResponse>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (response?.Results == null)
                    return new List<JobSearchResultDto>();

                if (response.Results.Count == 0)
                    return new List<JobSearchResultDto>();

                return response.Results.Select(job => new JobSearchResultDto
                {
                    ExternalId = job.Id ?? string.Empty,
                    Title = job.Title ?? "Untitled job",
                    Company = job.Company?.DisplayName ?? "Unknown company",
                    Location = job.Location?.DisplayName ?? "Location not specified",
                    Description = CleanDescription(job.Description),
                    ApplyUrl = job.RedirectUrl ?? string.Empty,
                    Source = "Adzuna",
                    Salary = FormatSalary(job.SalaryMin, job.SalaryMax),
                    PostedAt = job.Created
                }).ToList();
            }
            catch
            {
                return GetMockJobs(query, location, page, pageSize);
            }
        }

        private static string FormatSalary(decimal? salaryMin, decimal? salaryMax)
        {
            if (salaryMin.HasValue && salaryMax.HasValue)
                return $"{salaryMin.Value:0} - {salaryMax.Value:0}";

            if (salaryMin.HasValue)
                return $"From {salaryMin.Value:0}";

            if (salaryMax.HasValue)
                return $"Up to {salaryMax.Value:0}";

            return "Not specified";
        }

        private static string CleanDescription(string? description)
        {
            if (string.IsNullOrWhiteSpace(description))
                return "No description available.";

            var withoutHtml = Regex.Replace(description, "<.*?>", string.Empty);
            var decoded = WebUtility.HtmlDecode(withoutHtml);

            decoded = Regex.Replace(decoded, @"\s+", " ").Trim();

            if (decoded.Length > 350)
                decoded = decoded[..350] + "...";

            return decoded;
        }

        private static List<JobSearchResultDto> GetMockJobs(
            string? query,
            string? location,
            int page,
            int pageSize)
        {
            var mockJobs = new List<JobSearchResultDto>
            {
                new JobSearchResultDto
                {
                    ExternalId = "mock-1",
                    Title = "Junior .NET Developer",
                    Company = "Tech Solutions",
                    Location = "Bucharest / Remote",
                    Description = "Work on web and mobile applications using C#, .NET, REST APIs and SQL. Suitable for junior candidates with basic backend knowledge.",
                    ApplyUrl = "https://www.linkedin.com/jobs/",
                    Source = "Mock",
                    Salary = "Not specified",
                    PostedAt = DateTime.UtcNow.AddDays(-2)
                },
                new JobSearchResultDto
                {
                    ExternalId = "mock-2",
                    Title = "Frontend Developer",
                    Company = "Digital Studio",
                    Location = "Cluj-Napoca",
                    Description = "Build responsive user interfaces using HTML, CSS, JavaScript and modern frontend frameworks.",
                    ApplyUrl = "https://www.linkedin.com/jobs/",
                    Source = "Mock",
                    Salary = "Not specified",
                    PostedAt = DateTime.UtcNow.AddDays(-4)
                },
                new JobSearchResultDto
                {
                    ExternalId = "mock-3",
                    Title = "Mobile App Developer",
                    Company = "AppWorks",
                    Location = "Remote",
                    Description = "Develop mobile applications and collaborate with backend developers, designers and QA engineers.",
                    ApplyUrl = "https://www.linkedin.com/jobs/",
                    Source = "Mock",
                    Salary = "Not specified",
                    PostedAt = DateTime.UtcNow.AddDays(-1)
                },
                new JobSearchResultDto
                {
                    ExternalId = "mock-4",
                    Title = "Data Analyst",
                    Company = "Insight Analytics",
                    Location = "Bucharest",
                    Description = "Analyze business data, create reports and dashboards, and support decision making with clear insights.",
                    ApplyUrl = "https://www.linkedin.com/jobs/",
                    Source = "Mock",
                    Salary = "Not specified",
                    PostedAt = DateTime.UtcNow.AddDays(-7)
                },
                new JobSearchResultDto
                {
                    ExternalId = "mock-5",
                    Title = "QA Tester",
                    Company = "QualitySoft",
                    Location = "Iasi / Remote",
                    Description = "Test web and mobile applications, write test cases, report bugs and collaborate with developers.",
                    ApplyUrl = "https://www.linkedin.com/jobs/",
                    Source = "Mock",
                    Salary = "Not specified",
                    PostedAt = DateTime.UtcNow.AddDays(-3)
                },
                new JobSearchResultDto
                {
                    ExternalId = "mock-6",
                    Title = "Backend Developer",
                    Company = "Cloud Systems",
                    Location = "Timisoara",
                    Description = "Develop REST APIs, work with databases, authentication and application architecture using .NET.",
                    ApplyUrl = "https://www.linkedin.com/jobs/",
                    Source = "Mock",
                    Salary = "Not specified",
                    PostedAt = DateTime.UtcNow.AddDays(-5)
                }
            };

            var results = mockJobs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                var normalizedQuery = query.Trim().ToLower();

                results = results.Where(job =>
                    job.Title.ToLower().Contains(normalizedQuery) ||
                    job.Company.ToLower().Contains(normalizedQuery) ||
                    job.Description.ToLower().Contains(normalizedQuery));
            }

            if (!string.IsNullOrWhiteSpace(location))
            {
                var normalizedLocation = location.Trim().ToLower();

                results = results.Where(job =>
                    job.Location.ToLower().Contains(normalizedLocation));
            }

            return results
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        private class AdzunaSearchResponse
        {
            [JsonPropertyName("results")]
            public List<AdzunaJobResult> Results { get; set; } = new();
        }

        private class AdzunaJobResult
        {
            [JsonPropertyName("id")]
            public string? Id { get; set; }

            [JsonPropertyName("title")]
            public string? Title { get; set; }

            [JsonPropertyName("description")]
            public string? Description { get; set; }

            [JsonPropertyName("redirect_url")]
            public string? RedirectUrl { get; set; }

            [JsonPropertyName("created")]
            public DateTime? Created { get; set; }

            [JsonPropertyName("salary_min")]
            public decimal? SalaryMin { get; set; }

            [JsonPropertyName("salary_max")]
            public decimal? SalaryMax { get; set; }

            [JsonPropertyName("company")]
            public AdzunaCompany? Company { get; set; }

            [JsonPropertyName("location")]
            public AdzunaLocation? Location { get; set; }
        }

        private class AdzunaCompany
        {
            [JsonPropertyName("display_name")]
            public string? DisplayName { get; set; }
        }

        private class AdzunaLocation
        {
            [JsonPropertyName("display_name")]
            public string? DisplayName { get; set; }
        }
    }
}