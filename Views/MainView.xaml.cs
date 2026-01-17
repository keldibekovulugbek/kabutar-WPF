using System;
using System.Windows;
using System.Windows.Media.Animation;
using Kabutar_WPF.Services;
using Kabutar_WPF.ViewModels;

namespace Kabutar_WPF.Views
{
    public partial class MainView : Window
    {
        private bool _isMenuOpen = false;

        public MainView()
        {
            InitializeComponent();

            var apiClient = ApiClient.Instance;
            var authService = new AuthService(apiClient);
            var searchService = new SearchService(apiClient);
            var messageService = new MessageService(apiClient);
            var chatService = new ChatService(apiClient);

            DataContext = new MainViewModel(authService, searchService, messageService, chatService);

            // Load current user info for menu
            _ = LoadCurrentUserInfoAsync();
        }

        private async System.Threading.Tasks.Task LoadCurrentUserInfoAsync()
        {
            try
            {
                var apiClient = ApiClient.Instance;
                var userService = new UserService(apiClient);
                var userProfile = await userService.GetCurrentUserAsync();

                if (userProfile != null)
                {
                    MenuUserName.Text = $"{userProfile.FirstName} {userProfile.LastName}";

                    // Set initials (first letter of firstname + first letter of lastname)
                    var firstInitial = string.IsNullOrEmpty(userProfile.FirstName) ? "" : userProfile.FirstName[0].ToString();
                    var lastInitial = string.IsNullOrEmpty(userProfile.LastName) ? "" : userProfile.LastName[0].ToString();
                    MenuUserInitials.Text = (firstInitial + lastInitial).ToUpper();
                }
            }
            catch (Exception ex)
            {
                // Silently fail, keep default values
                Console.WriteLine($"Failed to load user info: {ex.Message}");
            }
        }

        private void HamburgerButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ToggleMenu();
        }

        private void ToggleMenu()
        {
            if (_isMenuOpen)
            {
                CloseMenu();
            }
            else
            {
                OpenMenu();
            }
        }

        private void OpenMenu()
        {
            _isMenuOpen = true;
            MenuOverlay.Visibility = Visibility.Visible;

            // Animate menu sliding in
            var slideIn = new DoubleAnimation
            {
                From = -280,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(250),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            MenuTransform.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, slideIn);
        }

        private void CloseMenu()
        {
            _isMenuOpen = false;

            // Animate menu sliding out
            var slideOut = new DoubleAnimation
            {
                From = 0,
                To = -280,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };

            slideOut.Completed += (s, e) =>
            {
                MenuOverlay.Visibility = Visibility.Collapsed;
            };

            MenuTransform.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, slideOut);
        }

        private void MenuOverlay_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            CloseMenu();
        }

        private void ProfileSettings_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            CloseMenu();

            var apiClient = ApiClient.Instance;
            var authService = new AuthService(apiClient);
            var userService = new UserService(apiClient);

            var profileWindow = new ProfileSettingsView(userService, authService)
            {
                Owner = this
            };

            profileWindow.ShowDialog();
        }

        private void ChangeTheme_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            CloseMenu();
            // TODO: Implement theme switching
            MessageBox.Show("Mavzuni almashtirish hali ishlab chiqilmagan", "Xabar", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Settings_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            CloseMenu();
            // TODO: Open settings
            MessageBox.Show("Sozlamalar sahifasi hali ishlab chiqilmagan", "Xabar", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Logout_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            CloseMenu();
        }
    }
}
