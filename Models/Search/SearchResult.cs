using System;
using System.Collections.Generic;

namespace Kabutar_WPF.Models.Search
{
    public class SearchResult
    {
        public List<UserSearchResult> Users { get; set; } = new();
        public List<MessageSearchResult> Messages { get; set; } = new();
    }

    public class UserSearchResult
    {
        public long Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Firstname { get; set; } = string.Empty;
        public string Lastname { get; set; } = string.Empty;
        public string? ProfilePicture { get; set; }
        public bool IsOnline { get; set; }

        public string FullName => $"{Firstname} {Lastname}";
    }

    public class MessageSearchResult
    {
        public long MessageId { get; set; }
        public long ChatId { get; set; }
        public long UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Firstname { get; set; } = string.Empty;
        public string Lastname { get; set; } = string.Empty;
        public string? ProfilePicture { get; set; }
        public string MessageContent { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }

        public string FullName => $"{Firstname} {Lastname}";
    }
}
