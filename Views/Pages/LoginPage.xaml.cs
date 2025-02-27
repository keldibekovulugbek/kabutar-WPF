using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Kabutar_WPF.Services;
using Kabutar_WPF.ViewModels;
using Kabutar_WPF.Views.Pages;

namespace Kabutar_WPF.Views.Pages
{
    public partial class LoginPage : Page
    {
        private readonly MainWindow _mainWindow;
        private readonly IAuthService _authService;

        public LoginPage(MainWindow mainWindow, IAuthService authService)
        {
            InitializeComponent();
            _authService = authService;
            DataContext = new LoginViewModel(_authService);
            _mainWindow = mainWindow;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel viewModel && sender is PasswordBox passwordBox)
            {
                viewModel.Password = passwordBox.Password;
            }
        }

        private void RegisterButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _mainWindow.NavigateTo(new RegisterPage(_mainWindow,_authService));
        }
    }
}
