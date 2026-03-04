using System.Windows;
using Kabutar_WPF.Services;
using Kabutar_WPF.Views;
using Kabutar_WPF.Views.Auth;

namespace Kabutar_WPF
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var apiClient = ApiClient.Instance;
            var authService = new AuthService(apiClient);

            if (authService.TryRestoreSession())
            {
                var mainView = new MainView();
                mainView.Show();
            }
            else
            {
                var loginView = new LoginView();
                loginView.Show();
            }
        }
    }
}
