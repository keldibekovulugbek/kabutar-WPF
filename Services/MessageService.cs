using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kabutar_WPF.Models.Messages;

namespace Kabutar_WPF.Services
{
    public interface IMessageService
    {
        Task<bool> SendMessageAsync(SendMessageRequest request);
        Task<List<MessageDTO>> GetConversationAsync(long userId, int page = 1, int pageSize = 50);
        Task<bool> MarkAsReadAsync(long messageId);
        Task<bool> DeleteMessageAsync(long messageId, bool deleteForBoth = false);
        Task<bool> ClearChatAsync(long otherUserId, bool clearForBoth = false);
        Task<bool> SendImageAsync(long receiverId, string imagePath);
        Task<bool> EditMessageAsync(long messageId, string newContent);
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

        public async Task<List<MessageDTO>> GetConversationAsync(long userId, int page = 1, int pageSize = 50)
        {
            try
            {
                var url = page > 1
                    ? $"messages/conversation/{userId}?page={page}&pageSize={pageSize}"
                    : $"messages/conversation/{userId}?pageSize={pageSize}";
                var result = await _apiClient.GetAsync<List<MessageDTO>>(url);
                return result ?? new List<MessageDTO>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Xabarlarni yuklashda xatolik: {ex.Message}", ex);
            }
        }

        public async Task<bool> EditMessageAsync(long messageId, string newContent)
        {
            try
            {
                return await _apiClient.PutAsync($"messages/{messageId}", new { content = newContent });
            }
            catch (Exception ex)
            {
                throw new Exception($"Xabarni tahrirlashda xatolik: {ex.Message}", ex);
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
        public long? ReplyToMessageId { get; set; }
        public string? ReplyToContent { get; set; }
        public string? ReplyToSenderName { get; set; }
    }
}
