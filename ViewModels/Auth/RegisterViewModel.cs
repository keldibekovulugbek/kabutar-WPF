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
    public class RegisterViewModel : ObservableObject
    {
        private readonly IAuthService _authService;
        private string _firstname = string.Empty;
        private string _lastname = string.Empty;
        private string _email = string.Empty;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isLoading;

        public RegisterViewModel()
        {
            var apiClient = new ApiClient();
            _authService = new AuthService(apiClient);

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

        public ICommand RegisterCommand { get; }
        public ICommand NavigateToLoginCommand { get; }

        private bool CanRegister()
        {
            return !string.IsNullOrWhiteSpace(Firstname) &&
                   !string.IsNullOrWhiteSpace(Lastname) &&
                   !string.IsNullOrWhiteSpace(Email) &&
                   !string.IsNullOrWhiteSpace(Username) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   !IsLoading;
        }

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
                    // Registration successful - navigate to verify email view
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        var verifyEmailView = new VerifyEmailView();

                        // Pass email to VerifyEmailViewModel if needed
                        if (verifyEmailView.DataContext is VerifyEmailViewModel viewModel)
                        {
                            viewModel.Email = Email;
                        }

                        verifyEmailView.Show();

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
