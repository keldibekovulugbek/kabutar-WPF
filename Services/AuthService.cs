using System;
using System.Threading.Tasks;
using System.Windows;
using Kabutar_WPF.Models.Auth;

namespace Kabutar_WPF.Services
{
    public interface IAuthService
    {
        Task<LoginResponse?> LoginAsync(LoginRequest request);
        Task<bool> RegisterAsync(RegisterRequest request);
        Task<bool> VerifyEmailAsync(VerifyEmailRequest request);
        Task<bool> SendPasswordResetCodeAsync(SendCodeRequest request);
        Task<bool> ResetPasswordAsync(ResetPasswordRequest request);
        void SaveToken(string token);
        string? GetToken();
        void ClearToken();
        bool IsAuthenticated { get; }
    }

    public class AuthService : IAuthService
    {
        private readonly ApiClient _apiClient;
        private const string TokenKey = "AuthToken";

        public AuthService(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public bool IsAuthenticated => !string.IsNullOrEmpty(GetToken());

        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            try
            {
                var response = await _apiClient.PostAsync<LoginRequest, LoginResponse>("/account/login", request);

                if (response != null && !string.IsNullOrEmpty(response.Token))
                {
                    SaveToken(response.Token);
                    _apiClient.SetAuthToken(response.Token);
                }

                return response;
            }
            catch (Exception ex)
            {
                throw new Exception($"Login failed: {ex.Message}", ex);
            }
        }

        public async Task<bool> RegisterAsync(RegisterRequest request)
        {
            try
            {
                return await _apiClient.PostAsync("/account/register", request);
            }
            catch (Exception ex)
            {
                throw new Exception($"Registration failed: {ex.Message}", ex);
            }
        }

        public async Task<bool> VerifyEmailAsync(VerifyEmailRequest request)
        {
            try
            {
                return await _apiClient.PostAsync("/account/verify-email", request);
            }
            catch (Exception ex)
            {
                throw new Exception($"Email verification failed: {ex.Message}", ex);
            }
        }

        public async Task<bool> SendPasswordResetCodeAsync(SendCodeRequest request)
        {
            try
            {
                return await _apiClient.PostAsync("/account/send-code", request);
            }
            catch (Exception ex)
            {
                throw new Exception($"Send code failed: {ex.Message}", ex);
            }
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request)
        {
            try
            {
                return await _apiClient.PostAsync("/account/reset-password", request);
            }
            catch (Exception ex)
            {
                throw new Exception($"Password reset failed: {ex.Message}", ex);
            }
        }

        public void SaveToken(string token)
        {
            Application.Current.Properties[TokenKey] = token;
        }

        public string? GetToken()
        {
            return Application.Current.Properties.Contains(TokenKey)
                ? Application.Current.Properties[TokenKey] as string
                : null;
        }

        public void ClearToken()
        {
            if (Application.Current.Properties.Contains(TokenKey))
            {
                Application.Current.Properties.Remove(TokenKey);
            }
            _apiClient.ClearAuthToken();
        }
    }
}
