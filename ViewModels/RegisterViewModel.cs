using System.Threading.Tasks;
using System.Windows.Input;
using Kabutar_WPF.Services;
using Kabutar_WPF.ViewModels.Common;
using Kabutar_WPF.Models;
using Kabutar_WPF.Helpers;
using Kabutar_WPF.Views.Pages;

namespace Kabutar_WPF.ViewModels
{
    public class RegisterViewModel : BaseViewModel
    {
        private string _firstname;
        private string _lastname;
        private string _email;
        private string _username;
        private string _password;
        private string _confirmPassword;
        private string _passwordError;
        private string _confirmPasswordError;
        private string _errorMessage; // ⬅ Xatolar uchun yangi property
        private bool _isLoading;
        private readonly IAuthService _authService;
        private readonly MainWindow _mainWindow;

        public RegisterViewModel(MainWindow mainWindow,IAuthService authService)
        {
            _authService = authService;
            _mainWindow = mainWindow;
            RegisterCommand = new RelayCommand(async () => await RegisterAsync(), CanExecuteRegister);
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
            set
            {
                SetProperty(ref _password, value);
                ValidatePassword();
            }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                SetProperty(ref _confirmPassword, value);
                ValidateConfirmPassword();
            }
        }

        public string PasswordError
        {
            get => _passwordError;
            set => SetProperty(ref _passwordError, value);
        }

        public string ConfirmPasswordError
        {
            get => _confirmPasswordError;
            set => SetProperty(ref _confirmPasswordError, value);
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

        public ICommand RegisterCommand { get; }

        private bool CanExecuteRegister()
        {
            return !string.IsNullOrWhiteSpace(Firstname) &&
                   !string.IsNullOrWhiteSpace(Lastname) &&
                   !string.IsNullOrWhiteSpace(Email) &&
                   !string.IsNullOrWhiteSpace(Username) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   !string.IsNullOrWhiteSpace(ConfirmPassword) &&
                   string.IsNullOrEmpty(PasswordError) &&
                   string.IsNullOrEmpty(ConfirmPasswordError) &&
                   !IsLoading;
        }

        private void ValidatePassword()
        {
            PasswordError = Password.Length < 8 ? "Password must be at least 8 characters" : "";
            (RegisterCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }

        private void ValidateConfirmPassword()
        {
            ConfirmPasswordError = Password != ConfirmPassword ? "Passwords do not match" : "";
            (RegisterCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }

        private async Task RegisterAsync()
        {
            if (!CanExecuteRegister()) return;

            IsLoading = true;
            ErrorMessage = ""; 

            try
            {
                var registerRequest = new RegisterRequest
                {
                    Firstname = Firstname,
                    Lastname = Lastname,
                    Email = Email,
                    Username = Username,
                    Password = Password
                };

                bool isRegistered = await _authService.RegisterAsync(registerRequest);

                if (isRegistered)
                {
                    _mainWindow.NavigateTo(new VerifyEmailPage(_mainWindow, _authService, Email));
                }
                else
                {
                    ErrorMessage = "Registration failed. Please try again.";
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
