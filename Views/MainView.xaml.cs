using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Kabutar_WPF.Services;
using Kabutar_WPF.ViewModels;
using Kabutar_WPF.Helpers;

namespace Kabutar_WPF.Views
{
    public partial class MainView : Window
    {
        private bool _isMenuOpen = false;

        public MainView()
        {
            InitializeComponent();

            NotificationService.Initialize(RootGrid);

            var apiClient = ApiClient.Instance;
            var authService = new AuthService(apiClient);
            var searchService = new SearchService(apiClient);
            var messageService = new MessageService(apiClient);
            var chatService = new ChatService(apiClient);

            DataContext = new MainViewModel(authService, searchService, messageService, chatService);

            Loaded += async (s, e) =>
            {
                await LoadCurrentUserInfoAsync();
                await LoadUserSettingsAsync();
            };
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
                    var fullName = userProfile.Fullname;
                    if (string.IsNullOrWhiteSpace(fullName))
                        fullName = $"{userProfile.FirstName} {userProfile.LastName}".Trim();

                    MenuUserName.Text = string.IsNullOrWhiteSpace(fullName) ? userProfile.Username : fullName;

                    var firstInitial = string.IsNullOrEmpty(userProfile.FirstName) ? "" : userProfile.FirstName[0].ToString();
                    var lastInitial = string.IsNullOrEmpty(userProfile.LastName) ? "" : userProfile.LastName[0].ToString();
                    var initials = (firstInitial + lastInitial).ToUpper();

                    if (string.IsNullOrEmpty(initials) && !string.IsNullOrEmpty(userProfile.Username))
                        initials = userProfile.Username[0].ToString().ToUpper();

                    MenuUserInitials.Text = initials;

                    if (!string.IsNullOrEmpty(userProfile.ProfilePicture) && !userProfile.ProfilePicture.Contains("default"))
                    {
                        await LoadProfileImageAsync(userProfile.ProfilePicture);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading user profile: {ex.Message}");
            }
        }

        private async System.Threading.Tasks.Task LoadProfileImageAsync(string imagePath)
        {
            try
            {
                var imageUrl = GetFullImageUrl(imagePath);
                System.Diagnostics.Debug.WriteLine($"Loading profile image from: {imageUrl}");

                using var httpClient = new System.Net.Http.HttpClient();
                var imageBytes = await httpClient.GetByteArrayAsync(imageUrl);

                await Dispatcher.InvokeAsync(() =>
                {
                    var bitmap = new BitmapImage();
                    using (var stream = new System.IO.MemoryStream(imageBytes))
                    {
                        bitmap.BeginInit();
                        bitmap.StreamSource = stream;
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        bitmap.Freeze();
                    }

                    MenuUserImage.ImageSource = bitmap;
                    MenuUserImageBorder.Visibility = Visibility.Visible;
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load profile image: {ex.Message}");
                MenuUserImageBorder.Visibility = Visibility.Collapsed;
            }
        }

        private string GetFullImageUrl(string imagePath)
        {
            if (imagePath.StartsWith("http://") || imagePath.StartsWith("https://"))
                return imagePath;

            return $"http://localhost:5237/{imagePath.TrimStart('/')}";
        }

        private async System.Threading.Tasks.Task LoadUserSettingsAsync()
        {
            try
            {
                var apiClient = ApiClient.Instance;
                var userService = new UserService(apiClient);

                var settings = await userService.GetSettingsAsync();

                if (settings != null)
                {
                    ApplyTheme(settings.Theme);

                    ApplyFontSize(settings.FontSize);

                    if (!string.IsNullOrEmpty(settings.ChatBackgroundImage))
                    {
                        await ApplyChatBackgroundAsync(settings.ChatBackgroundImage);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading user settings: {ex.Message}");
            }
        }

        private void ApplyTheme(string theme)
        {
            try
            {
                var app = Application.Current;
                var mergedDicts = app.Resources.MergedDictionaries;

                ResourceDictionary? themeToRemove = null;
                foreach (var dict in mergedDicts)
                {
                    if (dict.Source != null &&
                        (dict.Source.OriginalString.Contains("LightTheme") ||
                         dict.Source.OriginalString.Contains("DarkTheme")))
                    {
                        themeToRemove = dict;
                        break;
                    }
                }

                if (themeToRemove != null)
                    mergedDicts.Remove(themeToRemove);

                var themeUri = theme == "dark"
                    ? new Uri("Resources/Themes/DarkTheme.xaml", UriKind.Relative)
                    : new Uri("Resources/Themes/LightTheme.xaml", UriKind.Relative);

                mergedDicts.Add(new ResourceDictionary { Source = themeUri });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying theme: {ex.Message}");
            }
        }

        private void ApplyFontSize(string fontSize)
        {
            try
            {
                double fontSizeValue = fontSize switch
                {
                    "small" => 12.0,
                    "large" => 18.0,
                    _ => 14.0
                };

                Application.Current.Resources["ChatFontSize"] = fontSizeValue;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying font size: {ex.Message}");
            }
        }

        private async System.Threading.Tasks.Task ApplyChatBackgroundAsync(string imagePath)
        {
            try
            {
                var imageUrl = GetFullImageUrl(imagePath);

                using var httpClient = new System.Net.Http.HttpClient();
                var imageBytes = await httpClient.GetByteArrayAsync(imageUrl);

                await Dispatcher.InvokeAsync(() =>
                {
                    var bitmap = new BitmapImage();
                    using (var stream = new System.IO.MemoryStream(imageBytes))
                    {
                        bitmap.BeginInit();
                        bitmap.StreamSource = stream;
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        bitmap.Freeze();
                    }

                    var imageBrush = new ImageBrush(bitmap)
                    {
                        Stretch = Stretch.UniformToFill,
                        Opacity = 0.3
                    };

                    ChatBackgroundBorder.Background = imageBrush;
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying chat background: {ex.Message}");
            }
        }

        public async System.Threading.Tasks.Task RefreshSettingsAsync()
        {
            await LoadUserSettingsAsync();
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

            var apiClient = ApiClient.Instance;
            var userService = new UserService(apiClient);

            var themeWindow = new ThemeSettingsView(userService)
            {
                Owner = this
            };

            themeWindow.ShowDialog();
        }

        private void Settings_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            CloseMenu();
            NotificationService.Show("Sozlamalar sahifasi hali ishlab chiqilmagan", NotificationType.Info);
        }

        private void Logout_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            CloseMenu();

            var apiClient = ApiClient.Instance;
            var authService = new AuthService(apiClient);
            authService.ClearToken();

            var loginView = new Auth.LoginView();
            loginView.Show();

            Close();
        }

        private async void ChatUserInfo_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                var viewModel = DataContext as MainViewModel;
                if (viewModel?.SelectedChat == null)
                    return;

                var apiClient = ApiClient.Instance;
                var userService = new UserService(apiClient);

                var userProfile = await apiClient.GetAsync<Models.Users.UserProfileDTO>($"users/{viewModel.SelectedChat.Id}");

                if (userProfile != null)
                {
                    var userCard = new UserCardView(userProfile, viewModel.SelectedChat.IsOnline)
                    {
                        Owner = this
                    };

                    userCard.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                NotificationService.Show($"Foydalanuvchi ma'lumotlarini olishda xatolik: {ex.Message}", NotificationType.Error);
            }
        }
    }
}
