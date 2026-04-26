using System.Windows;
using Kabutar_WPF.Core;
using Kabutar_WPF.Services;

namespace Kabutar_WPF
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var apiClient = ApiClient.Instance;
            var authService = new AuthService(apiClient);
            var userService = new UserService(apiClient);
            var searchService = new SearchService(apiClient);
            var messageService = new MessageService(apiClient);
            var chatService = new ChatService(apiClient);

            var navigation = new WindowNavigationService(
                authService, searchService, messageService, chatService, userService);

            if (authService.TryRestoreSession())
                navigation.ShowMainView();
            else
                navigation.ShowLoginView();
        }
    }
}
