using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Kabutar_WPF.Helpers;
using Kabutar_WPF.Models;
using Kabutar_WPF.Services;
using Kabutar_WPF.ViewModels.Common;

namespace Kabutar_WPF.ViewModels
{
    public class RegisterViewModel : BaseViewModel
    {
        private string _firstname;
        private string _lastname;
        private string _email;
        private string _username;
        private string _password;
        private string _passwordError;
        private bool _isLoading = false;
        private readonly IAuthService _authService;

        public RegisterViewModel()
        {
            _authService = new AuthService(new System.Net.Http.HttpClient());
            RegisterCommand = new RelayCommand(async () => await RegisterAsync(), CanExecuteRegister);
        }

        public string Firstname
        {
            get => _firstname;
            set
            {
                SetProperty(ref _firstname, value);
                OnPropertyChanged(nameof(CanExecuteRegister));
                (RegisterCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public string Lastname
        {
            get => _lastname;
            set
            {
                SetProperty(ref _lastname, value);
                OnPropertyChanged(nameof(CanExecuteRegister));
                (RegisterCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                SetProperty(ref _email, value);
                OnPropertyChanged(nameof(CanExecuteRegister));
                (RegisterCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public string Username
        {
            get => _username;
            set
            {
                SetProperty(ref _username, value);
                OnPropertyChanged(nameof(CanExecuteRegister));
                (RegisterCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                SetProperty(ref _password, value);
                ValidatePassword();
                OnPropertyChanged(nameof(CanExecuteRegister));
                (RegisterCommand as RelayCommand)?.RaiseCanExecuteChanged();
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

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                SetProperty(ref _isLoading, value);
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        public ICommand RegisterCommand { get; }

        private bool CanExecuteRegister()
        {
            return !string.IsNullOrWhiteSpace(Firstname) &&
                   !string.IsNullOrWhiteSpace(Lastname) &&
                   !string.IsNullOrWhiteSpace(Email) &&
                   !string.IsNullOrWhiteSpace(Username) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   string.IsNullOrEmpty(PasswordError);
        }

        private void ValidatePassword()
        {
            if (string.IsNullOrWhiteSpace(Password) || Password.Length < 8)
            {
                PasswordError = "Password must be at least 8 characters";
            }
            else
            {
                PasswordError = "";
            }
        }

        private async Task RegisterAsync()
        {
            if (!CanExecuteRegister())
            {
                MessageBox.Show("Iltimos, barcha maydonlarni to‘ldiring!", "Xatolik", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsLoading = true;

            try
            {
                var registerRequest = new RegisterRequest
                {
                    Firstname = Firstname,
                    Lastname = Lastname,
                    Email = Email,
                    Username = Username,
                    Password = Password,
                };

                var result = await _authService.RegisterAsync(registerRequest);
                if (result)
                {
                    MessageBox.Show("Ro‘yxatdan o‘tish muvaffaqiyatli tugadi!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Ro‘yxatdan o‘tishda xatolik yuz berdi!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Xatolik yuz berdi: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
