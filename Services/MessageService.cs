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
    }

    public class MessageDTO
    {
        public long Id { get; set; }
        public long SenderId { get; set; }
        public long ReceiverId { get; set; }
        public string Content { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public bool HasAttachment { get; set; }
        public DateTime Created { get; set; }
    }
}
