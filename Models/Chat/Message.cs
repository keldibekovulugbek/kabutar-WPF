using System;

namespace Kabutar_WPF.Models.Chat
{
    public class Message
    {
        public long Id { get; set; }
        public long ChatId { get; set; }
        public long SenderId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }
        public bool IsSent { get; set; }
        public bool IsFromMe { get; set; }
        public string? AttachmentUrl { get; set; }
        public bool HasImage => !string.IsNullOrEmpty(AttachmentUrl);
    }
}
