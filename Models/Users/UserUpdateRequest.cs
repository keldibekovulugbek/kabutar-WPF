namespace Kabutar_WPF.Models.Users
{
    public class UserUpdateRequest
    {
        public string Firstname { get; set; } = string.Empty;
        public string Lastname { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string? About { get; set; }
    }
}
