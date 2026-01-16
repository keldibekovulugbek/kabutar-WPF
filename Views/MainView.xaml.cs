using System.Windows;
using Kabutar_WPF.Services;
using Kabutar_WPF.ViewModels;

namespace Kabutar_WPF.Views
{
    public partial class MainView : Window
    {
        public MainView()
        {
            InitializeComponent();

            var apiClient = new ApiClient();
            var authService = new AuthService(apiClient);
            var searchService = new SearchService(apiClient);
            var messageService = new MessageService(apiClient);

            DataContext = new MainViewModel(authService, searchService, messageService);
        }
    }
}
