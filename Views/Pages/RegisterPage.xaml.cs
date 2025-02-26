using System.Windows;
using System.Windows.Controls;
using Kabutar_WPF.Views.Pages;

namespace Kabutar_WPF.Views.Pages
{
    public partial class RegisterPage : Page
    {
        public RegisterPage()
        {
            InitializeComponent();
        }

        private void BackToLoginButton_Click(object sender, RoutedEventArgs e)
        {
            LoginPage loginPage = new LoginPage();
            Window window = new Window
            {
                Content = loginPage,
                Title = "Login",
                Height = 500,
                Width = 400
            };
            window.Show();
        }
    }
}
