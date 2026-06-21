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
            int pageSize = 20,
            string? countryOverride = null)
        {
            var appId = _configuration["Adzuna:AppId"];
            var appKey = _configuration["Adzuna:AppKey"];

            var country = string.IsNullOrWhiteSpace(countryOverride)
                ? _configuration["Adzuna:Country"] ?? "gb"
                : countryOverride.Trim().ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(appId))
                throw new Exception("Adzuna AppId is missing.");

            if (string.IsNullOrWhiteSpace(appKey))
                throw new Exception("Adzuna AppKey is missing.");

            if (appId.Contains("PASTE", StringComparison.OrdinalIgnoreCase) ||
                appKey.Contains("PASTE", StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("Adzuna AppId or AppKey still contains placeholder text.");
            }

            if (string.IsNullOrWhiteSpace(country))
                country = "gb";

            if (page < 1)
                page = 1;

            if (pageSize < 1)
                pageSize = 20;

            if (pageSize > 50)
                pageSize = 50;

            var encodedQuery = Uri.EscapeDataString(query ?? string.Empty);
            var encodedLocation = Uri.EscapeDataString(location ?? string.Empty);

            var url =
                $"https://api.adzuna.com/v1/api/jobs/{country}/search/{page}" +
                $"?app_id={Uri.EscapeDataString(appId)}" +
                $"&app_key={Uri.EscapeDataString(appKey)}" +
                $"&results_per_page={pageSize}" +
                $"&what={encodedQuery}" +
                $"&where={encodedLocation}" +
                $"&content-type=application/json";

            HttpResponseMessage httpResponse;

            try
            {
                httpResponse = await _httpClient.GetAsync(url);
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not reach Adzuna API: {ex.Message}", ex);
            }

            var bytes = await httpResponse.Content.ReadAsByteArrayAsync();
            var json = Encoding.UTF8.GetString(bytes);

            if (!httpResponse.IsSuccessStatusCode)
            {
                throw new Exception(
                    $"Adzuna returned status {(int)httpResponse.StatusCode} {httpResponse.ReasonPhrase}: {json}");
            }

            AdzunaSearchResponse? response;

            try
            {
                response = JsonSerializer.Deserialize<AdzunaSearchResponse>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not parse Adzuna response: {ex.Message}. Raw response: {json}", ex);
            }

            if (response?.Results == null || response.Results.Count == 0)
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
            decoded = Regex.Replace(decoded, @"\.{2,}\s*$", string.Empty).Trim();

            var lastSentenceEnd = decoded.LastIndexOfAny(new[] { '.', '!', '?' });

            if (lastSentenceEnd > 80 && lastSentenceEnd < decoded.Length - 1)
            {
                decoded = decoded[..(lastSentenceEnd + 1)].Trim();
            }
            else
            {
                decoded = decoded.TrimEnd(',', ';', ':', '-', ' ');
            }

            if (!decoded.EndsWith(".") &&
                !decoded.EndsWith("!") &&
                !decoded.EndsWith("?"))
            {
                decoded += ".";
            }

            return decoded;
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