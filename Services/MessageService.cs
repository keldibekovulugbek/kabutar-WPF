using System;
using System.Threading.Tasks;
using Kabutar_WPF.Models.Messages;

namespace Kabutar_WPF.Services
{
    public interface IMessageService
    {
        Task<bool> SendMessageAsync(SendMessageRequest request);
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
                return await _apiClient.PostAsync("/message/send", request);
            }
            catch (Exception ex)
            {
                throw new Exception($"Xabar yuborishda xatolik: {ex.Message}", ex);
            }
        }
    }
}
