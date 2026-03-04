using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kabutar_WPF.Models.Chat;
using Newtonsoft.Json;

namespace Kabutar_WPF.Services
{
    public interface IChatService
    {
        Task<List<ChatItem>> GetRecentChatsAsync();
    }

    public class ChatService : IChatService
    {
        private readonly ApiClient _apiClient;

        public ChatService(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<List<ChatItem>> GetRecentChatsAsync()
        {
            try
            {
                var response = await _apiClient.GetAsync<List<ChatUserDTO>>("messages/recent");

                if (response == null)
                {
                    return new List<ChatItem>();
                }

                return response.Select(dto => new ChatItem
                {
                    Id = dto.UserId,
                    Name = $"{dto.FirstName} {dto.LastName}".Trim(),
                    ProfilePicture = dto.ProfilePicture,
                    LastMessage = dto.LastMessage,
                    LastMessageTime = dto.Timestamp.ToLocalTime(),
                    UnreadCount = dto.UnreadCount,
                    IsOnline = dto.IsOnline,
                    LastActive = dto.LastActive?.ToLocalTime(),
                    IsGroup = false
                }).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Chatlarni yuklashda xatolik: {ex.Message}", ex);
            }
        }
    }

    internal class ChatUserDTO
    {
        public long UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? ProfilePicture { get; set; }
        public string LastMessage { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public int UnreadCount { get; set; }
        public bool IsOnline { get; set; }
        public DateTime? LastActive { get; set; }
    }
}
