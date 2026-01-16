namespace Kabutar_WPF.Models.Auth
{
    public class SendCodeRequest
    {
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordRequest
    {
        public string Email { get; set; } = string.Empty;
        public int Code { get; set; }
        public string Password { get; set; } = string.Empty;
    }
}
