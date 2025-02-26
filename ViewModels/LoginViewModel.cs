

using Kabutar_WPF.Helpers;
using Kabutar_WPF.Models;
using Kabutar_WPF.Services;
using Kabutar_WPF.ViewModels.Common;
using Kabutar_WPF.Views.Pages;
using System.Net.Http;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;

namespace Kabutar_WPF.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private string _username;
        private string _password;
        private string _passwordError;
        private bool _isLoading = false;
        private readonly IAuthService _authService;

        public LoginViewModel()
        {
            _authService = new AuthService(new HttpClient());

            LoginCommand = new RelayCommand(async () => await LoginAsync(), CanExecuteLogin);
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
            return !string.IsNullOrWhiteSpace(Username) &&
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

        private async Task LoginAsync()
        {

            if (!CanExecuteLogin())
            {
                MessageBox.Show("Iltimos, hamma maydonlarni to‘g‘ri to‘ldiring!", "Xatolik", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsLoading = true;

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
                    MessageBox.Show("Tizimga muvaffaqiyatli kirdingiz!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Login xato! Foydalanuvchi nomi yoki parol noto‘g‘ri!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
