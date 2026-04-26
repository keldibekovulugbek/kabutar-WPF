using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Kabutar_WPF.Core;
using Kabutar_WPF.Helpers;
using Kabutar_WPF.Models.Auth;
using Kabutar_WPF.Services;

namespace Kabutar_WPF.ViewModels.Auth
{
    public class ForgotPasswordViewModel : ObservableObject
    {
        private readonly IAuthService _authService;
        private readonly IWindowNavigationService _navigation;
        private string _email = string.Empty;
        private string _code = string.Empty;
        private string _newPassword = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isLoading;
        private bool _isCodeSent;

        public event Action? RequestClose;

        public ForgotPasswordViewModel(IAuthService authService, IWindowNavigationService navigation)
        {
            _authService = authService;
            _navigation = navigation;

            SendCodeCommand = new RelayCommand(async _ => await SendCodeAsync(), _ => CanSendCode());
            ResetPasswordCommand = new RelayCommand(async _ => await ResetPasswordAsync(), _ => CanResetPassword());
            NavigateBackCommand = new RelayCommand(_ => NavigateBack());
        }

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string Code
        {
            get => _code;
            set => SetProperty(ref _code, value);
        }

        public string NewPassword
        {
            get => _newPassword;
            set => SetProperty(ref _newPassword, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                if (SetProperty(ref _errorMessage, value))
                    OnPropertyChanged(nameof(HasError));
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public bool IsCodeSent
        {
            get => _isCodeSent;
            set => SetProperty(ref _isCodeSent, value);
        }

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        public ICommand SendCodeCommand { get; }
        public ICommand ResetPasswordCommand { get; }
        public ICommand NavigateBackCommand { get; }

        private bool CanSendCode() => !string.IsNullOrWhiteSpace(Email) && !IsLoading;

        private bool CanResetPassword() =>
            !string.IsNullOrWhiteSpace(Email) &&
            !string.IsNullOrWhiteSpace(Code) &&
            !string.IsNullOrWhiteSpace(NewPassword) &&
            !IsLoading;

        private async Task SendCodeAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var success = await _authService.SendPasswordResetCodeAsync(new SendCodeRequest { Email = Email.Trim() });

                if (success)
                    IsCodeSent = true;
                else
                    ErrorMessage = "Failed to send code. Please check your email and try again.";
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ResetPasswordAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var request = new ResetPasswordRequest
                {
                    Email = Email.Trim(),
                    Code = int.TryParse(Code.Trim(), out int code) ? code : 0,
                    Password = NewPassword
                };

                var success = await _authService.ResetPasswordAsync(request);

                if (success)
                {
                    NotificationService.Show("Parol muvaffaqiyatli o'zgartirildi! Yangi parol bilan tizimga kirishingiz mumkin.", NotificationType.Success);
                    await System.Threading.Tasks.Task.Delay(1500);
                    _navigation.ShowLoginView();
                    RequestClose?.Invoke();
                }
                else
                {
                    ErrorMessage = "Password reset failed. Please check your code and try again.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void NavigateBack()
        {
            if (IsCodeSent)
            {
                IsCodeSent = false;
                Code = string.Empty;
                NewPassword = string.Empty;
                ErrorMessage = string.Empty;
            }
            else
            {
                _navigation.ShowLoginView();
                RequestClose?.Invoke();
            }
        }
    }
}
