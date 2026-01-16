using System;

namespace Kabutar_WPF.Models.Chat
{
    public class ChatItem
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ProfilePicture { get; set; }
        public string LastMessage { get; set; } = string.Empty;
        public DateTime LastMessageTime { get; set; }
        public int UnreadCount { get; set; }
        public bool IsOnline { get; set; }
        public bool IsGroup { get; set; }
    }
}
