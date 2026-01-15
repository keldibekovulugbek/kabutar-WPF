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
    public class VerifyEmailViewModel : ObservableObject
    {
        private readonly IAuthService _authService;
        private string _email = string.Empty;
        private string _code = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isLoading;

        public VerifyEmailViewModel()
        {
            var apiClient = new ApiClient();
            _authService = new AuthService(apiClient);

            VerifyCommand = new RelayCommand(async _ => await VerifyEmailAsync(), _ => CanVerify());
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

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                if (SetProperty(ref _errorMessage, value))
                {
                    OnPropertyChanged(nameof(HasError));
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        public ICommand VerifyCommand { get; }
        public ICommand NavigateBackCommand { get; }

        private bool CanVerify()
        {
            return !string.IsNullOrWhiteSpace(Email) &&
                   !string.IsNullOrWhiteSpace(Code) &&
                   !IsLoading;
        }

        private async Task VerifyEmailAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var request = new VerifyEmailRequest
                {
                    Email = Email.Trim(),
                    Code = Code.Trim()
                };

                var success = await _authService.VerifyEmailAsync(request);

                if (success)
                {
                    // Verification successful - navigate to login view
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show("Email verified successfully! You can now log in.",
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
                    ErrorMessage = "Verification failed. Please check your code and try again.";
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
            Application.Current.Dispatcher.Invoke(() =>
            {
                var registerView = new RegisterView();
                registerView.Show();

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
