using System.Threading.Tasks;
using Kabutar_WPF.Models;

namespace Kabutar_WPF.Services
{
    public interface IAuthService
    {
        Task<string> LoginAsync(LoginRequest loginRequest);
        Task<bool> RegisterAsync(RegisterRequest registerRequest);
        Task<bool> VerifyEmailAsync(VerifyEmail verifyEmail);
        Task<bool> SendCodeAsync(SendCode sendCode);
    }
}
