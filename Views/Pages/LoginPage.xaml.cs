using System.Windows;
using System.Windows.Controls;
using Kabutar_WPF.ViewModels;
using Kabutar_WPF.Views.Pages;

namespace Kabutar_WPF.Views.Pages
{
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
            DataContext = new LoginViewModel();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            RegisterPage registerPage = new RegisterPage();

            this.Visibility = Visibility.Hidden;

            registerPage.Visibility = Visibility.Visible;
        }
    }
}
