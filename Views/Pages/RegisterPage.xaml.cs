using System.Windows.Controls;
using System.Windows;
using Kabutar_WPF.Views.Pages;
using Kabutar_WPF.ViewModels;
using Kabutar_WPF.Services;

namespace Kabutar_WPF.Views.Pages
{
    public partial class RegisterPage : Page
    {
        private readonly MainWindow _mainWindow;
        private readonly IAuthService _authService;

        public RegisterPage(MainWindow mainWindow, IAuthService authService)
        {
            InitializeComponent();
            _authService = authService;
            _mainWindow = mainWindow;
            DataContext = new RegisterViewModel(mainWindow,_authService);
        }

        private void BackToLoginButton_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.NavigateTo(new LoginPage(_mainWindow,_authService));
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is RegisterViewModel viewModel && sender is PasswordBox passwordBox)
            {
                viewModel.Password = passwordBox.Password;
            }
        }
        private void PasswordBox_ConfirmPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is RegisterViewModel viewModel && sender is PasswordBox passwordBox)
            {
                viewModel.ConfirmPassword = passwordBox.Password;
            }
        }
    }
}
