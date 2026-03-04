namespace Kabutar_WPF.Models.Users
{
    public class UserSettingsDTO
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string Theme { get; set; } = "light";
        public string? ChatBackgroundImage { get; set; }
        public string FontSize { get; set; } = "medium";
    }

    public class UserSettingsUpdateRequest
    {
        public string? Theme { get; set; }
        public string? ChatBackgroundImage { get; set; }
        public string? FontSize { get; set; }
    }
}
