using System.Text;
using System.Text.Json;
using UserManagementService.Common.DTOs;

namespace UserManagementService.UI.Services
{
    public class UserServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public UserServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        public async Task<List<UserDto>> GetUsersAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<UserDto>>("Users") ?? new List<UserDto>();
        }

        public async Task<UserDto?> GetUserAsync(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<UserDto>($"Users/{id}", _jsonOptions);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task<bool> CreateUserAsync(CreateUserDto createUserDto)
        {
            var response = await _httpClient.PostAsJsonAsync("Users", createUserDto);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateUserAsync(UpdateUserDto updateUserDto)
        {
            var response = await _httpClient.PatchAsJsonAsync("Users", updateUserDto);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"Users/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ValidatePasswordAsync(ValidatePasswordDto validatePasswordDto)
        {
            var response = await _httpClient.PostAsJsonAsync("Users/validate", validatePasswordDto);
            return response.IsSuccessStatusCode;
        }
    }
}

