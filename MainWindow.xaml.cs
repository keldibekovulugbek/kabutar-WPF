using System.Windows;
using Kabutar_WPF.Views.Pages;
using Kabutar_WPF.Services;
using System.Net.Http;

namespace Kabutar_WPF
{
    public partial class MainWindow : Window
    {
        private readonly IAuthService _authService;

        public MainWindow()
        {
            InitializeComponent();
            _authService = new AuthService(new HttpClient());

            ShowLoginPage(); // Dastur boshlanganda Login sahifasini ko‘rsatish
        }

        public void ShowLoginPage()
        {
            Content = new LoginPage();
        }

        public void ShowRegisterPage()
        {
            Content = new RegisterPage();
        }
    }
}
