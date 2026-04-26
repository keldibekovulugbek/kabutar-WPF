using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Kabutar_WPF.Core;
using Kabutar_WPF.Helpers;
using Kabutar_WPF.Models.Auth;
using Kabutar_WPF.Services;

namespace Kabutar_WPF.ViewModels.Auth
{
    public class VerifyEmailViewModel : ObservableObject
    {
        private readonly IAuthService _authService;
        private readonly IWindowNavigationService _navigation;
        private string _email = string.Empty;
        private string _code = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isLoading;

        public event Action? RequestClose;

        public VerifyEmailViewModel(IAuthService authService, IWindowNavigationService navigation)
        {
            _authService = authService;
            _navigation = navigation;

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
                    OnPropertyChanged(nameof(HasError));
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

        private bool CanVerify() =>
            !string.IsNullOrWhiteSpace(Email) &&
            !string.IsNullOrWhiteSpace(Code) &&
            !IsLoading;

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
                    NotificationService.Show("Email tasdiqlandi! Tizimga kirishingiz mumkin.", NotificationType.Success);
                    await System.Threading.Tasks.Task.Delay(1500);
                    _navigation.ShowLoginView();
                    RequestClose?.Invoke();
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
            _navigation.ShowRegisterView();
            RequestClose?.Invoke();
        }
    }
}
