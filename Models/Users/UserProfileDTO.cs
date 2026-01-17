namespace Kabutar_WPF.Models.Users
{
    public class UserProfileDTO
    {
        public long Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string? About { get; set; }
        public string? ProfilePicture { get; set; }
        public string Email { get; set; } = string.Empty;
    }
}
