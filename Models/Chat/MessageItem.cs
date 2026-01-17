using System;

namespace Kabutar_WPF.Models.Chat
{
    // Wrapper class to support both messages and date separators
    public class MessageItem
    {
        public bool IsDateSeparator { get; set; }
        public string? DateText { get; set; }
        public Message? Message { get; set; }
    }
}
