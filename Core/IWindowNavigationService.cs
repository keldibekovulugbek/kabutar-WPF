namespace Kabutar_WPF.Core
{
    public interface IWindowNavigationService
    {
        void ShowMainView();
        void ShowLoginView();
        void ShowRegisterView();
        void ShowVerifyEmailView(string email);
        void ShowForgotPasswordView();
    }
}
