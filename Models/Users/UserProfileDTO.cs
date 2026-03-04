using System;
using Newtonsoft.Json;

namespace Kabutar_WPF.Models.Users
{
    public class UserProfileDTO
    {
        public long Id { get; set; }

        [JsonProperty("fullname")]
        public string Fullname { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;
        public string? About { get; set; }

        [JsonProperty("imagePath")]
        public string? ImagePath { get; set; }

        public string CreatedAt { get; set; } = string.Empty;

        public bool IsOnline { get; set; }
        public DateTime? LastActive { get; set; }

        public string FirstName => GetFirstName();
        public string LastName => GetLastName();
        public string? ProfilePicture => ImagePath;

        private string GetFirstName()
        {
            if (string.IsNullOrEmpty(Fullname)) return string.Empty;
            var parts = Fullname.Split(' ', 2);
            return parts.Length > 0 ? parts[0] : string.Empty;
        }

        private string GetLastName()
        {
            if (string.IsNullOrEmpty(Fullname)) return string.Empty;
            var parts = Fullname.Split(' ', 2);
            return parts.Length > 1 ? parts[1] : string.Empty;
        }
    }
}
