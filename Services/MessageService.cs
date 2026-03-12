using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kabutar_WPF.Models.Messages;

namespace Kabutar_WPF.Services
{
    public interface IMessageService
    {
        Task<bool> SendMessageAsync(SendMessageRequest request);
        Task<List<MessageDTO>> GetConversationAsync(long userId);
        Task<bool> MarkAsReadAsync(long messageId);
        Task<bool> DeleteMessageAsync(long messageId, bool deleteForBoth = false);
        Task<bool> ClearChatAsync(long otherUserId, bool clearForBoth = false);
        Task<bool> SendImageAsync(long receiverId, string imagePath);
    }

    public class MessageService : IMessageService
    {
        private readonly ApiClient _apiClient;

        public MessageService(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<bool> SendMessageAsync(SendMessageRequest request)
        {
            try
            {
                return await _apiClient.PostAsync("messages/text", request);
            }
            catch (Exception ex)
            {
                throw new Exception($"Xabar yuborishda xatolik: {ex.Message}", ex);
            }
        }

        public async Task<List<MessageDTO>> GetConversationAsync(long userId)
        {
            try
            {
                var result = await _apiClient.GetAsync<List<MessageDTO>>($"messages/conversation/{userId}");
                return result ?? new List<MessageDTO>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Xabarlarni yuklashda xatolik: {ex.Message}", ex);
            }
        }

        public async Task<bool> MarkAsReadAsync(long messageId)
        {
            try
            {
                return await _apiClient.PutAsync($"messages/read/{messageId}", new { });
            }
            catch (Exception ex)
            {
                throw new Exception($"Xabarni o'qilgan deb belgilashda xatolik: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteMessageAsync(long messageId, bool deleteForBoth = false)
        {
            try
            {
                var url = deleteForBoth
                    ? $"messages/{messageId}?deleteForBoth=true"
                    : $"messages/{messageId}";
                return await _apiClient.DeleteAsync(url);
            }
            catch (Exception ex)
            {
                throw new Exception($"Xabarni o'chirishda xatolik: {ex.Message}", ex);
            }
        }

        public async Task<bool> ClearChatAsync(long otherUserId, bool clearForBoth = false)
        {
            try
            {
                var url = clearForBoth
                    ? $"messages/chat/{otherUserId}?clearForBoth=true"
                    : $"messages/chat/{otherUserId}";
                return await _apiClient.DeleteAsync(url);
            }
            catch (Exception ex)
            {
                throw new Exception($"Chatni tozalashda xatolik: {ex.Message}", ex);
            }
        }

        public async Task<bool> SendImageAsync(long receiverId, string imagePath)
        {
            try
            {
                var fields = new Dictionary<string, string>
                {
                    ["receiverId"] = receiverId.ToString(),
                    ["content"] = "📷 Rasm"
                };
                return await _apiClient.PostMultipartAsync("messages", fields, imagePath, "attachment");
            }
            catch (Exception ex)
            {
                throw new Exception($"Rasm yuborishda xatolik: {ex.Message}", ex);
            }
        }
    }

    public class MessageDTO
    {
        public long Id { get; set; }
        public long SenderId { get; set; }
        public long ReceiverId { get; set; }
        public string Content { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public bool HasAttachment { get; set; }
        public string? AttachmentUrl { get; set; }
        public DateTime Created { get; set; }
    }
}
