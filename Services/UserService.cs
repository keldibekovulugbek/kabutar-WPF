using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Kabutar_WPF.Models.Users;

namespace Kabutar_WPF.Services
{
    public interface IUserService
    {
        Task<UserProfileDTO?> GetCurrentUserAsync();
        Task<bool> UpdateProfileAsync(UserUpdateRequest request);
        Task<bool> UploadProfileImageAsync(string imagePath);
    }

    public class UserService : IUserService
    {
        private readonly ApiClient _apiClient;

        public UserService(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<UserProfileDTO?> GetCurrentUserAsync()
        {
            try
            {
                // Assuming there's an endpoint to get current user info
                // For now, return null as we might need to add this endpoint to backend
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get user profile: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdateProfileAsync(UserUpdateRequest request)
        {
            try
            {
                return await _apiClient.PutAsync("users", request);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update profile: {ex.Message}", ex);
            }
        }

        public async Task<bool> UploadProfileImageAsync(string imagePath)
        {
            try
            {
                if (!File.Exists(imagePath))
                    throw new FileNotFoundException("Image file not found", imagePath);

                using var fileStream = File.OpenRead(imagePath);
                using var content = new MultipartFormDataContent();
                using var fileContent = new StreamContent(fileStream);

                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
                content.Add(fileContent, "Image", Path.GetFileName(imagePath));

                var response = await _apiClient.PostFormDataAsync("users/image", content);
                return response;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to upload profile image: {ex.Message}", ex);
            }
        }
    }
}
