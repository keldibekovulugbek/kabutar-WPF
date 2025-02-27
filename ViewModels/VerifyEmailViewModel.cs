using System;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Timers;
using Kabutar_WPF.ViewModels.Common;
using Kabutar_WPF.Services;
using Kabutar_WPF.Helpers;
using Kabutar_WPF.Models;
using Kabutar_WPF.Views.Pages;
using Timer = System.Timers.Timer;

namespace Kabutar_WPF.ViewModels
{
    public class VerifyEmailViewModel : BaseViewModel
    {
        private readonly MainWindow _mainWindow;
        private readonly IAuthService _authService;
        private readonly string _email;
        private string _errorMessage;
        private bool _isLoading;
        private bool _canResendCode = true; // 🔥 1 daqiqada faqat 1 marta jo‘natish
        private int _resendCooldown = 0; // 🔥 Qancha soniya qoldi

        private string _digit1;
        private string _digit2;
        private string _digit3;
        private string _digit4;
        private string _digit5;

        public VerifyEmailViewModel(MainWindow mainWindow, IAuthService authService, string email)
        {
            _mainWindow = mainWindow;
            _authService = authService;
            _email = email;
            VerifyCodeCommand = new RelayCommand(async () => await VerifyCodeAsync(), CanExecuteVerify);
            ResendCodeCommand = new RelayCommand(async () => await ResendCodeAsync(), () => _canResendCode);
            NavigateBackCommand = new RelayCommand(NavigateBack);
        }

        // 🔢 5 ta raqam maydoni alohida
        public string Digit1 { get => _digit1; set { SetProperty(ref _digit1, value); OnPropertyChanged(nameof(VerificationCode)); } }
        public string Digit2 { get => _digit2; set { SetProperty(ref _digit2, value); OnPropertyChanged(nameof(VerificationCode)); } }
        public string Digit3 { get => _digit3; set { SetProperty(ref _digit3, value); OnPropertyChanged(nameof(VerificationCode)); } }
        public string Digit4 { get => _digit4; set { SetProperty(ref _digit4, value); OnPropertyChanged(nameof(VerificationCode)); } }
        public string Digit5 { get => _digit5; set { SetProperty(ref _digit5, value); OnPropertyChanged(nameof(VerificationCode)); } }

        public string VerificationCode => $"{Digit1}{Digit2}{Digit3}{Digit4}{Digit5}";

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

        public bool CanResendCode
        {
            get => _canResendCode;
            set
            {
                SetProperty(ref _canResendCode, value);
                (ResendCodeCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
        public string ResendButtonText
        {
            get => _canResendCode ? "Resend Code" : $"Wait {_resendCooldown}s...";
        }

        public ICommand VerifyCodeCommand { get; }
        public ICommand ResendCodeCommand { get; }
        public ICommand NavigateBackCommand { get; }

        private bool CanExecuteVerify()
        {
            return VerificationCode.Length == 5 && !IsLoading;
        }

        private async Task VerifyCodeAsync()
        {
            if (!CanExecuteVerify()) return;

            IsLoading = true;
            ErrorMessage = ""; // ⬅ Xatoliklarni tozalash

            try
            {
                var verifyEmail = new VerifyEmail
                {
                    Email = _email,
                    Code = VerificationCode
                };

                bool isValid = await _authService.VerifyEmailAsync(verifyEmail);

                if (isValid)
                {
                    _mainWindow.NavigateTo(new LoginPage(_mainWindow, _authService)); // ✅ Login sahifasiga o‘tish
                }
                else
                {
                    ErrorMessage = "Invalid verification code. Please try again.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ResendCodeAsync()
        {
            if (!_canResendCode) return;

            try
            {
                var sendCode = new SendCode { Email = _email };
                bool isSent = await _authService.SendCodeAsync(sendCode);

                if (isSent)
                {
                    ErrorMessage = "A new verification code has been sent!";
                    StartResendCooldown();
                }
                else
                {
                    ErrorMessage = "Failed to resend verification code.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
            }
        }

        private void StartResendCooldown()
        {
            CanResendCode = false;
            _resendCooldown = 60;
            OnPropertyChanged(nameof(ResendButtonText)); 

            Timer timer = new Timer(1000); 
            timer.Elapsed += (sender, e) =>
            {
                _resendCooldown--;
                OnPropertyChanged(nameof(ResendButtonText));

                if (_resendCooldown <= 0)
                {
                    CanResendCode = true;
                    OnPropertyChanged(nameof(ResendButtonText)); 
                    timer.Stop();
                }
            };
            timer.Start();
        }


        private void NavigateBack()
        {
            _mainWindow.NavigateTo(new RegisterPage(_mainWindow, _authService));
        }
    }
}
