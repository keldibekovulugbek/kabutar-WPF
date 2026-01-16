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
            var chatService = new ChatService(apiClient);

            DataContext = new MainViewModel(authService, searchService, messageService, chatService);
        }

        private void HamburgerButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            HamburgerMenuPopup.PlacementTarget = sender as System.Windows.UIElement;
            HamburgerMenuPopup.IsOpen = true;
        }

        private void ProfileSettings_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            HamburgerMenuPopup.IsOpen = false;
            // TODO: Open profile settings
            MessageBox.Show("Profilni sozlash sahifasi hali ishlab chiqilmagan", "Xabar", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ChangeTheme_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            HamburgerMenuPopup.IsOpen = false;
            // TODO: Implement theme switching
            MessageBox.Show("Mavzuni almashtirish hali ishlab chiqilmagan", "Xabar", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Settings_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            HamburgerMenuPopup.IsOpen = false;
            // TODO: Open settings
            MessageBox.Show("Sozlamalar sahifasi hali ishlab chiqilmagan", "Xabar", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Logout_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            HamburgerMenuPopup.IsOpen = false;
        }
    }
}
