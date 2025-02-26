using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using Kabutar_WPF.Services;
using Kabutar_WPF.Views.Pages;

namespace Kabutar_WPF
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var mainFrame = new Frame();
            var authService = new AuthService(new HttpClient());

            var loginPage = new LoginPage();
            var mainWindow = new MainWindow { Content = mainFrame };

            mainFrame.Navigate(loginPage);
            mainWindow.Show();
        }
    }
}
