using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Kabutar_WPF.Core;
using Kabutar_WPF.Models.Auth;
using Kabutar_WPF.Services;
using Kabutar_WPF.Views.Auth;

namespace Kabutar_WPF.ViewModels.Auth
{
    public class ForgotPasswordViewModel : ObservableObject
    {
        private readonly IAuthService _authService;
        private string _email = string.Empty;
        private string _code = string.Empty;
        private string _newPassword = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isLoading;
        private bool _isCodeSent;

        public ForgotPasswordViewModel()
        {
            var apiClient = new ApiClient();
            _authService = new AuthService(apiClient);

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
            set => SetProperty(ref _errorMessage, value);
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

        private bool CanSendCode()
        {
            return !string.IsNullOrWhiteSpace(Email) && !IsLoading;
        }

        private bool CanResetPassword()
        {
            return !string.IsNullOrWhiteSpace(Email) &&
                   !string.IsNullOrWhiteSpace(Code) &&
                   !string.IsNullOrWhiteSpace(NewPassword) &&
                   !IsLoading;
        }

        private async Task SendCodeAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var request = new SendCodeRequest
                {
                    Email = Email.Trim()
                };

                var success = await _authService.SendPasswordResetCodeAsync(request);

                if (success)
                {
                    IsCodeSent = true;
                }
                else
                {
                    ErrorMessage = "Failed to send code. Please check your email and try again.";
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

        private async Task ResetPasswordAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var request = new ResetPasswordRequest
                {
                    Email = Email.Trim(),
                    Code = Code.Trim(),
                    NewPassword = NewPassword
                };

                var success = await _authService.ResetPasswordAsync(request);

                if (success)
                {
                    // Password reset successful - navigate to login view
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show("Password reset successfully! You can now log in with your new password.",
                            "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                        var loginView = new LoginView();
                        loginView.Show();

                        // Close current window
                        foreach (Window window in Application.Current.Windows)
                        {
                            if (window.DataContext == this)
                            {
                                window.Close();
                                break;
                            }
                        }
                    });
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
                // Go back to email entry step
                IsCodeSent = false;
                Code = string.Empty;
                NewPassword = string.Empty;
                ErrorMessage = string.Empty;
            }
            else
            {
                // Navigate back to login
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var loginView = new LoginView();
                    loginView.Show();

                    // Close current window
                    foreach (Window window in Application.Current.Windows)
                    {
                        if (window.DataContext == this)
                        {
                            window.Close();
                            break;
                        }
                    }
                });
            }
        }
    }
}
