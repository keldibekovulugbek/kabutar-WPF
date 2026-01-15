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
    public class LoginViewModel : ObservableObject
    {
        private readonly IAuthService _authService;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isLoading;

        public LoginViewModel()
        {
            var apiClient = new ApiClient();
            _authService = new AuthService(apiClient);

            LoginCommand = new RelayCommand(async _ => await LoginAsync(), _ => CanLogin());
            NavigateToRegisterCommand = new RelayCommand(_ => NavigateToRegister());
            NavigateToForgotPasswordCommand = new RelayCommand(_ => NavigateToForgotPassword());
        }

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
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

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        public ICommand LoginCommand { get; }
        public ICommand NavigateToRegisterCommand { get; }
        public ICommand NavigateToForgotPasswordCommand { get; }

        private bool CanLogin()
        {
            return !string.IsNullOrWhiteSpace(Username) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   !IsLoading;
        }

        private async Task LoginAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var request = new LoginRequest
                {
                    UsernameOrEmail = Username.Trim(),
                    Password = Password
                };

                var response = await _authService.LoginAsync(request);

                if (response != null && !string.IsNullOrEmpty(response.Token))
                {
                    // Login successful - navigate to main view
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        // TODO: Navigate to MainView when created
                        MessageBox.Show("Login successful! Main view coming soon.", "Success",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    });
                }
                else
                {
                    ErrorMessage = "Login failed. Please check your credentials.";
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

        private void NavigateToRegister()
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

        private void NavigateToForgotPassword()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var forgotPasswordView = new ForgotPasswordView();
                forgotPasswordView.Show();

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
