using JobCv.Mobile.Models;
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

        public ApiService()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://localhost:7158/")
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
    }
}