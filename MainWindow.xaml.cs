using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using Kabutar_WPF.Services;
using Kabutar_WPF.Views.Pages;

namespace Kabutar_WPF
{
    public partial class MainWindow : Window
    {
        private readonly IAuthService _authService;

        public MainWindow()
        {
            InitializeComponent();
            _authService = new AuthService(new HttpClient());

            MainFrame.Navigate(new LoginPage(this, _authService));
        }

        public void NavigateTo(Page page)
        {
            MainFrame.Navigate(page);
        }
    }
}
