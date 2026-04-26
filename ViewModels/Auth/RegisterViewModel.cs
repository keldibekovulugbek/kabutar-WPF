using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Kabutar_WPF.Core;
using Kabutar_WPF.Models.Auth;
using Kabutar_WPF.Services;

namespace Kabutar_WPF.ViewModels.Auth
{
    public class RegisterViewModel : ObservableObject
    {
        private readonly IAuthService _authService;
        private readonly IWindowNavigationService _navigation;
        private string _firstname = string.Empty;
        private string _lastname = string.Empty;
        private string _email = string.Empty;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isLoading;

        public event Action? RequestClose;

        public RegisterViewModel(IAuthService authService, IWindowNavigationService navigation)
        {
            _authService = authService;
            _navigation = navigation;

            RegisterCommand = new RelayCommand(async _ => await RegisterAsync(), _ => CanRegister());
            NavigateToLoginCommand = new RelayCommand(_ => NavigateToLogin());
        }

        public string Firstname
        {
            get => _firstname;
            set => SetProperty(ref _firstname, value);
        }

        public string Lastname
        {
            get => _lastname;
            set => SetProperty(ref _lastname, value);
        }

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
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

        public ICommand RegisterCommand { get; }
        public ICommand NavigateToLoginCommand { get; }

        private bool CanRegister() =>
            !string.IsNullOrWhiteSpace(Firstname) &&
            !string.IsNullOrWhiteSpace(Lastname) &&
            !string.IsNullOrWhiteSpace(Email) &&
            !string.IsNullOrWhiteSpace(Username) &&
            !string.IsNullOrWhiteSpace(Password) &&
            !IsLoading;

        private async Task RegisterAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var request = new RegisterRequest
                {
                    Firstname = Firstname.Trim(),
                    Lastname = Lastname.Trim(),
                    Email = Email.Trim(),
                    Username = Username.Trim(),
                    Password = Password
                };

                var success = await _authService.RegisterAsync(request);

                if (success)
                {
                    _navigation.ShowVerifyEmailView(Email);
                    RequestClose?.Invoke();
                }
                else
                {
                    ErrorMessage = "Registration failed. Please try again.";
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

        private void NavigateToLogin()
        {
            _navigation.ShowLoginView();
            RequestClose?.Invoke();
        }
    }
}
