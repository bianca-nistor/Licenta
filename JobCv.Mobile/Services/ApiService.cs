using JobCv.Mobile.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace JobCv.Mobile.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

#if ANDROID
        private const string ApiBaseUrl = "https://10.0.2.2:7158/";
#else
        private const string ApiBaseUrl = "https://localhost:7158/";
#endif

        public ApiService()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(ApiBaseUrl)
            };
        }

        public async Task<UserDto?> LoginAsync(LoginRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Auth/login", request);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<UserDto>(_jsonOptions);
        }

        public async Task<UserDto?> RegisterAsync(RegisterRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Auth/register", request);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<UserDto>(_jsonOptions);
        }

        public async Task<List<CvDto>> GetUserCvsAsync(int userId)
        {
            var cvs = await _httpClient.GetFromJsonAsync<List<CvDto>>($"api/Cvs/user/{userId}", _jsonOptions);

            return cvs ?? new List<CvDto>();
        }

        public async Task<CvDto?> CreateCvAsync(CreateCvRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Cvs", request);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<CvDto>(_jsonOptions);
        }

        public async Task<bool> DeleteCvAsync(int cvId)
        {
            var response = await _httpClient.DeleteAsync($"api/Cvs/{cvId}");

            return response.IsSuccessStatusCode;
        }
        public async Task<List<CvTemplateDto>> GetCvTemplatesAsync()
        {
            var templates = await _httpClient.GetFromJsonAsync<List<CvTemplateDto>>(
                "api/Cvs/templates",
                _jsonOptions);

            return templates ?? new List<CvTemplateDto>();
        }

      
        public async Task<CvDto?> GetCvByIdAsync(int cvId)
        {
            var response = await _httpClient.GetAsync($"api/Cvs/{cvId}");

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<CvDto>(_jsonOptions);
        }
        public async Task<bool> UpdateCvAsync(int cvId, UpdateCvRequest request)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/Cvs/{cvId}", request);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateCvPersonalInfoAsync(int cvId, UpdateCvPersonalInfoRequest request)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/Cvs/{cvId}/personal-info", request);
            return response.IsSuccessStatusCode;
        }

        public async Task<CvSkillDto?> AddSkillAsync(int cvId, AddSkillRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync($"api/Cvs/{cvId}/skills", request);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<CvSkillDto>(_jsonOptions);
        }

        public async Task<bool> UpdateSkillAsync(int skillId, UpdateSkillRequest request)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/Cvs/skills/{skillId}", request);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteSkillAsync(int skillId)
        {
            var response = await _httpClient.DeleteAsync($"api/Cvs/skills/{skillId}");
            return response.IsSuccessStatusCode;
        }

        public async Task<CvLanguageDto?> AddLanguageAsync(int cvId, AddLanguageRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync($"api/Cvs/{cvId}/languages", request);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<CvLanguageDto>(_jsonOptions);
        }

        public async Task<bool> UpdateLanguageAsync(int languageId, UpdateLanguageRequest request)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/Cvs/languages/{languageId}", request);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteLanguageAsync(int languageId)
        {
            var response = await _httpClient.DeleteAsync($"api/Cvs/languages/{languageId}");
            return response.IsSuccessStatusCode;
        }

        public async Task<CvProjectDto?> AddProjectAsync(int cvId, AddProjectRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync($"api/Cvs/{cvId}/projects", request);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<CvProjectDto>(_jsonOptions);
        }

        public async Task<bool> UpdateProjectAsync(int projectId, UpdateProjectRequest request)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/Cvs/projects/{projectId}", request);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteProjectAsync(int projectId)
        {
            var response = await _httpClient.DeleteAsync($"api/Cvs/projects/{projectId}");
            return response.IsSuccessStatusCode;
        }

        public async Task<CvEducationDto?> AddEducationAsync(int cvId, AddEducationRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync($"api/Cvs/{cvId}/educations", request);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<CvEducationDto>(_jsonOptions);
        }

        public async Task<bool> UpdateEducationAsync(int educationId, UpdateEducationRequest request)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/Cvs/educations/{educationId}", request);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteEducationAsync(int educationId)
        {
            var response = await _httpClient.DeleteAsync($"api/Cvs/educations/{educationId}");
            return response.IsSuccessStatusCode;
        }

        public async Task<CvExperienceDto?> AddExperienceAsync(int cvId, AddExperienceRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync($"api/Cvs/{cvId}/experiences", request);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<CvExperienceDto>(_jsonOptions);
        }

        public async Task<bool> UpdateExperienceAsync(int experienceId, UpdateExperienceRequest request)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/Cvs/experiences/{experienceId}", request);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteExperienceAsync(int experienceId)
        {
            var response = await _httpClient.DeleteAsync($"api/Cvs/experiences/{experienceId}");
            return response.IsSuccessStatusCode;
        }

        public async Task<CvCertificationDto?> AddCertificationAsync(int cvId, AddCertificationRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync($"api/Cvs/{cvId}/certifications", request);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<CvCertificationDto>(_jsonOptions);
        }

        public async Task<bool> UpdateCertificationAsync(int certificationId, UpdateCertificationRequest request)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/Cvs/certifications/{certificationId}", request);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteCertificationAsync(int certificationId)
        {
            var response = await _httpClient.DeleteAsync($"api/Cvs/certifications/{certificationId}");
            return response.IsSuccessStatusCode;
        }
        public string GetCvPdfUrl(int cvId)
        {
#if ANDROID
            return $"{ApiBaseUrl}api/Cvs/{cvId}/export-pdf";
#else
    return $"{ApiBaseUrl}api/Cvs/{cvId}/export-pdf";
#endif
        }
        public async Task<UploadedCvFileDto?> UploadCvFileAsync(int userId, FileResult file)
        {
            await using var stream = await file.OpenReadAsync();

            using var content = new MultipartFormDataContent();

            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType ?? "application/octet-stream");

            content.Add(fileContent, "file", file.FileName);

            var response = await _httpClient.PostAsync($"api/UploadedCvs/user/{userId}", content);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<UploadedCvFileDto>(_jsonOptions);
        }

        public async Task<List<UploadedCvFileDto>> GetUploadedCvFilesAsync(int userId)
        {
            var files = await _httpClient.GetFromJsonAsync<List<UploadedCvFileDto>>(
                $"api/UploadedCvs/user/{userId}",
                _jsonOptions);

            return files ?? new List<UploadedCvFileDto>();
        }

        public async Task<bool> DeleteUploadedCvFileAsync(int fileId)
        {
            var response = await _httpClient.DeleteAsync($"api/UploadedCvs/{fileId}");

            return response.IsSuccessStatusCode;
        }

        public string GetUploadedCvDownloadUrl(int fileId)
        {
            return $"{ApiBaseUrl}api/UploadedCvs/{fileId}/download";
        }
        public string GetCvPdfPreviewUrl(int cvId)
        {
            return $"{ApiBaseUrl}api/Cvs/{cvId}/preview-pdf";
        }

        public string GetCvPdfDownloadUrl(int cvId)
        {
            return $"{ApiBaseUrl}api/Cvs/{cvId}/export-pdf";
        }

        public string GetUploadedCvPreviewUrl(int fileId)
        {
            return $"{ApiBaseUrl}api/UploadedCvs/{fileId}/preview";
        }
        public string GetCvPhotoUrl(int cvId)
        {
            return $"{ApiBaseUrl}api/Cvs/{cvId}/photo";
        }

        public async Task<CvDto?> UploadCvPhotoAsync(int cvId, FileResult file)
        {
            await using var stream = await file.OpenReadAsync();

            using var content = new MultipartFormDataContent();

            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType ?? "application/octet-stream");

            content.Add(fileContent, "file", file.FileName);

            var response = await _httpClient.PostAsync($"api/Cvs/{cvId}/photo", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Photo upload error: {response.StatusCode} - {error}");
            }

            return await response.Content.ReadFromJsonAsync<CvDto>(_jsonOptions);
        }
        public async Task<CvDto?> DuplicateCvAsync(int cvId, DuplicateCvRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync($"api/Cvs/{cvId}/duplicate", request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Duplicate CV API error: {response.StatusCode} - {error}");
            }

            return await response.Content.ReadFromJsonAsync<CvDto>(_jsonOptions);
        }
        public async Task<List<JobSearchResultDto>> SearchJobsAsync(string query, string location, int page = 1)
        {
            var encodedQuery = Uri.EscapeDataString(query ?? "");
            var encodedLocation = Uri.EscapeDataString(location ?? "");

            var jobs = await _httpClient.GetFromJsonAsync<List<JobSearchResultDto>>(
                $"api/Jobs/search?query={encodedQuery}&location={encodedLocation}&page={page}",
                _jsonOptions);

            return jobs ?? new List<JobSearchResultDto>();
        }
        public async Task<JobApplicationDto?> CreateApplicationAsync(CreateJobApplicationRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Applications", request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Application API error: {response.StatusCode} - {error}");
            }

            return await response.Content.ReadFromJsonAsync<JobApplicationDto>(_jsonOptions);
        }

        public async Task<List<JobApplicationDto>> GetUserApplicationsAsync(int userId)
        {
            var applications = await _httpClient.GetFromJsonAsync<List<JobApplicationDto>>(
                $"api/Applications/user/{userId}",
                _jsonOptions);

            return applications ?? new List<JobApplicationDto>();
        }

        public async Task<bool> DeleteApplicationAsync(int applicationId)
        {
            var response = await _httpClient.DeleteAsync($"api/Applications/{applicationId}");

            return response.IsSuccessStatusCode;
        }
    }
}