using Kabutar_WPF.Models;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;

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

                if (response.IsSuccessStatusCode)
                {
                    return true; // ✅ Ro‘yxatdan o‘tish muvaffaqiyatli bo‘ldi
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Registration failed: {errorMessage}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Xatolik yuz berdi: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
    }
}
