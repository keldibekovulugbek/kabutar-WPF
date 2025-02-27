using System.Threading.Tasks;
using System.Windows.Input;
using Kabutar_WPF.Services;
using Kabutar_WPF.ViewModels.Common;
using Kabutar_WPF.Models;
using Kabutar_WPF.Helpers;

namespace Kabutar_WPF.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private string _username;
        private string _password;
        private string _passwordError;
        private string _errorMessage; // ⬅ Xatolar uchun yangi property
        private bool _isLoading = false;
        private readonly IAuthService _authService;

        public LoginViewModel(IAuthService authService)
        {
            _authService = authService;
            LoginCommand = new RelayCommand(async () => await LoginAsync());
        }

        public string Username
        {
            get => _username;
            set
            {
                SetProperty(ref _username, value);
                OnPropertyChanged(nameof(Username));
                OnPropertyChanged(nameof(CanExecuteLogin));
                (LoginCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                SetProperty(ref _password, value);
                ValidatePassword();
                OnPropertyChanged(nameof(Password));
                (LoginCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public string PasswordError
        {
            get => _passwordError;
            set
            {
                SetProperty(ref _passwordError, value);
                OnPropertyChanged(nameof(PasswordError));
            }
        }

        public string ErrorMessage // ⬅ UI'da xatolikni chiqarish uchun
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                SetProperty(ref _isLoading, value);
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        public ICommand LoginCommand { get; }

        private bool CanExecuteLogin()
        {
            if (string.IsNullOrWhiteSpace(Username) &&
                   string.IsNullOrWhiteSpace(Password))
                PasswordError = "Username and password will not be null!";
            else if (string.IsNullOrWhiteSpace(Username))
                PasswordError = "Username will not be null!";
            else if (string.IsNullOrWhiteSpace(Password))
                PasswordError = "Password will not be null!";
            else
                PasswordError = Password.Length < 8 ? "Password must be at least 8 characters" : "";

            return !string.IsNullOrWhiteSpace(Username) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   string.IsNullOrEmpty(PasswordError);
        }

        private void ValidatePassword()
        {
            PasswordError = Password.Length < 8 ? "Password must be at least 8 characters" : "";
            (LoginCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }

        private async Task LoginAsync()
        {
            if (!CanExecuteLogin()) return;

            IsLoading = true;
            ErrorMessage = ""; // ⬅ Xatoliklarni tozalash

            try
            {
                var loginRequest = new LoginRequest
                {
                    UsernameOrEmail = Username,
                    Password = Password
                };

                var token = await _authService.LoginAsync(loginRequest);
                if (!string.IsNullOrEmpty(token))
                {
                    Settings.Default.AuthToken = token;
                    Settings.Default.Save();
                    ErrorMessage = "Login successful!";
                }
                else
                {
                    ErrorMessage = "Login failed! Incorrect username or password.";
                }
            }
            catch (System.Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
