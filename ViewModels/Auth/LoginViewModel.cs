using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Kabutar_WPF.Core;
using Kabutar_WPF.Models.Auth;
using Kabutar_WPF.Services;

namespace Kabutar_WPF.ViewModels.Auth
{
    public class LoginViewModel : ObservableObject
    {
        private readonly IAuthService _authService;
        private readonly IWindowNavigationService _navigation;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isLoading;

        public event Action? RequestClose;

        public LoginViewModel(IAuthService authService, IWindowNavigationService navigation)
        {
            _authService = authService;
            _navigation = navigation;

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

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        public ICommand LoginCommand { get; }
        public ICommand NavigateToRegisterCommand { get; }
        public ICommand NavigateToForgotPasswordCommand { get; }

        private bool CanLogin() =>
            !string.IsNullOrWhiteSpace(Username) &&
            !string.IsNullOrWhiteSpace(Password) &&
            !IsLoading;

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
                    _navigation.ShowMainView();
                    RequestClose?.Invoke();
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
            _navigation.ShowRegisterView();
            RequestClose?.Invoke();
        }

        private void NavigateToForgotPassword()
        {
            _navigation.ShowForgotPasswordView();
            RequestClose?.Invoke();
        }
    }
}
