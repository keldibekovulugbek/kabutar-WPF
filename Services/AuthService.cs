using Kabutar_WPF.Models;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Kabutar_WPF.Services
{
    public class AuthService : IAuthService
    {
        private const string _baseUrl = "http://localhost:5237/api/accounts";
        private readonly HttpClient _httpClient;

        public AuthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> LoginAsync(LoginRequest loginRequest)
        {
            var response = await _httpClient.PostAsJsonAsync(_baseUrl + "/login", loginRequest);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
                return result.Token;
            }

            return null;
        }

        public async Task<bool> RegisterAsync(RegisterRequest registerRequest)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(_baseUrl + "/register", registerRequest);
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> SendCodeAsync(SendCode sendCode)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/send-code", sendCode);
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> VerifyEmailAsync(VerifyEmail verifyEmail)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/verify-email", verifyEmail);
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}
