using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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
        private readonly SignalRService _signalRService = new SignalRService();

        public MainView()
        {
            InitializeComponent();

            NotificationService.Initialize(RootGrid);

            var apiClient = ApiClient.Instance;
            var authService = new AuthService(apiClient);
            var searchService = new SearchService(apiClient);
            var messageService = new MessageService(apiClient);
            var chatService = new ChatService(apiClient);

            var vm = new MainViewModel(authService, searchService, messageService, chatService);
            vm.ShowClearChatDialogFunc = chat =>
            {
                var dialog = new ClearChatDialog(chat, this);
                dialog.ShowDialog();
                return (dialog.Confirmed, dialog.DeleteForBoth);
            };
            DataContext = vm;

            Loaded += async (s, e) =>
            {
                await LoadCurrentUserInfoAsync();
                await LoadUserSettingsAsync();
                await ConnectSignalRAsync();
            };

            Closing += async (s, e) =>
            {
                await _signalRService.DisconnectAsync();
            };
        }

        private async System.Threading.Tasks.Task ConnectSignalRAsync()
        {
            try
            {
                var apiClient = ApiClient.Instance;
                var authService = new AuthService(apiClient);
                var token = authService.GetToken();

                if (!string.IsNullOrEmpty(token))
                {
                    _signalRService.UserConnected += OnUserConnected;
                    _signalRService.UserDisconnected += OnUserDisconnected;
                    _signalRService.MessageReceived += OnMessageReceived;
                    await _signalRService.ConnectAsync(token);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SignalR connect error: {ex.Message}");
            }
        }

        private void OnMessageReceived(IncomingMessage msg)
        {
            Dispatcher.Invoke(() =>
            {
                if (DataContext is MainViewModel vm)
                {
                    vm.ReceiveIncomingMessage(msg.SenderId, msg.Content, msg.SentAt, msg.AttachmentUrl);
                }
            });
        }

        private void OnUserConnected(long userId)
        {
            Dispatcher.Invoke(() =>
            {
                if (DataContext is MainViewModel vm)
                {
                    var chat = vm.Chats.FirstOrDefault(c => c.Id == userId);
                    if (chat != null)
                    {
                        chat.IsOnline = true;
                        chat.LastActive = null;
                    }
                    if (vm.SelectedChat?.Id == userId)
                    {
                        vm.SelectedChat.IsOnline = true;
                        vm.SelectedChat.LastActive = null;
                        vm.NotifySelectedChatChanged();
                    }
                }
            });
        }

        private void OnUserDisconnected(long userId)
        {
            Dispatcher.Invoke(() =>
            {
                if (DataContext is MainViewModel vm)
                {
                    var chat = vm.Chats.FirstOrDefault(c => c.Id == userId);
                    if (chat != null)
                    {
                        chat.IsOnline = false;
                        chat.LastActive = DateTime.Now;
                    }
                    if (vm.SelectedChat?.Id == userId)
                    {
                        vm.SelectedChat.IsOnline = false;
                        vm.SelectedChat.LastActive = DateTime.Now;
                        vm.NotifySelectedChatChanged();
                    }
                }
            });
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
                    ApplyThemeGlobal(settings.Theme);

                    ApplyFontSize(settings.FontSize);

                    if (!string.IsNullOrEmpty(settings.ChatBackgroundImage))
                    {
                        await ApplyChatBackgroundAsync(settings.ChatBackgroundImage);
                    }
                    else
                    {
                        Dispatcher.Invoke(() =>
                        {
                            ChatBackgroundBorder.Background = null;
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading user settings: {ex.Message}");
            }
        }

        public static void ApplyThemeGlobal(string theme)
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
                    ? new Uri("pack://application:,,,/Resources/Themes/DarkTheme.xaml")
                    : new Uri("pack://application:,,,/Resources/Themes/LightTheme.xaml");

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

            var profilePanel = new ProfileSettingsView(userService, authService);
            profilePanel.CloseRequested += HideRightPanel;
            ShowRightPanel(profilePanel);
        }

        private void ChangeTheme_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            CloseMenu();

            var apiClient = ApiClient.Instance;
            var userService = new UserService(apiClient);

            var themePanel = new ThemeSettingsView(userService)
            {
                ParentMainView = this
            };
            themePanel.CloseRequested += HideRightPanel;
            ShowRightPanel(themePanel);
        }

        private void ShowRightPanel(System.Windows.Controls.UserControl content)
        {
            RightPanelContent.Content = content;
            RightPanelOverlay.Visibility = Visibility.Visible;
        }

        private void HideRightPanel()
        {
            RightPanelOverlay.Visibility = Visibility.Collapsed;
            RightPanelContent.Content = null;
        }

        private void Settings_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            CloseMenu();
            var settingsPanel = new SettingsView();
            settingsPanel.CloseRequested += HideRightPanel;
            ShowRightPanel(settingsPanel);
        }

        private void About_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            CloseMenu();
            var aboutPanel = new AboutView();
            aboutPanel.CloseRequested += HideRightPanel;
            ShowRightPanel(aboutPanel);
        }

        private async void Logout_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            CloseMenu();

            await _signalRService.DisconnectAsync();

            var apiClient = ApiClient.Instance;
            var authService = new AuthService(apiClient);
            authService.ClearToken();

            var loginView = new Auth.LoginView();
            loginView.Show();

            Close();
        }

        private static string FixExifRotation(string filePath)
        {
            try
            {
                using var stream = System.IO.File.OpenRead(filePath);
                var decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                var frame = decoder.Frames[0];

                int orientation = 1;
                if (frame.Metadata is BitmapMetadata meta && meta.ContainsQuery("/app1/ifd/{ushort=274}"))
                    orientation = (int)(ushort)meta.GetQuery("/app1/ifd/{ushort=274}");

                double angle = orientation switch
                {
                    3 => 180,
                    6 => 90,
                    8 => -90,
                    _ => 0
                };

                if (angle == 0) return filePath;

                BitmapSource rotated = new TransformedBitmap(frame, new RotateTransform(angle));

                var encoder = new JpegBitmapEncoder { QualityLevel = 90 };
                encoder.Frames.Add(BitmapFrame.Create(rotated));

                var tempPath = System.IO.Path.Combine(
                    System.IO.Path.GetTempPath(),
                    $"kabutar_img_{Guid.NewGuid()}.jpg");

                using var outStream = System.IO.File.OpenWrite(tempPath);
                encoder.Save(outStream);
                return tempPath;
            }
            catch
            {
                return filePath;
            }
        }

        private async void SendImage_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Rasm tanlang",
                Filter = "Rasm fayllari (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png",
                FilterIndex = 1
            };

            if (openFileDialog.ShowDialog() != true) return;

            var viewModel = DataContext as MainViewModel;
            if (viewModel?.SelectedChat == null) return;

            try
            {
                var fixedPath = FixExifRotation(openFileDialog.FileName);
                var messageService = new MessageService(ApiClient.Instance);
                var success = await messageService.SendImageAsync(viewModel.SelectedChat.Id, fixedPath);

                if (success)
                {
                    var now = DateTime.Now;
                    var newMessage = new Models.Chat.Message
                    {
                        Id = now.Ticks,
                        ChatId = viewModel.SelectedChat.Id,
                        Content = "📷 Rasm",
                        AttachmentUrl = fixedPath,
                        SentAt = now,
                        IsFromMe = true,
                        IsSent = true
                    };
                    viewModel.Messages.Add(new Models.Chat.MessageItem { Message = newMessage });

                    viewModel.SelectedChat.LastMessage = "📷 Rasm";
                    viewModel.SelectedChat.LastMessageTime = now;
                    var index = viewModel.Chats.IndexOf(viewModel.SelectedChat);
                    if (index > 0) viewModel.Chats.Move(index, 0);

                    NotificationService.Show("Rasm yuborildi", NotificationType.Success);
                }
            }
            catch (Exception ex)
            {
                NotificationService.Show($"Rasm yuborishda xatolik: {ex.Message}", NotificationType.Error);
            }
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
                    var lastActive = userProfile.LastActive?.ToLocalTime() ?? viewModel.SelectedChat.LastActive;
                    var isOnline = viewModel.SelectedChat.IsOnline;
                    var userCard = new UserCardView(userProfile, isOnline, lastActive);
                    userCard.CloseRequested += HideRightPanel;
                    ShowRightPanel(userCard);
                }
            }
            catch (Exception ex)
            {
                NotificationService.Show($"Foydalanuvchi ma'lumotlarini olishda xatolik: {ex.Message}", NotificationType.Error);
            }
        }


        private void ChatImage_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is System.Windows.Controls.Image img && img.Tag is string attachmentUrl && !string.IsNullOrEmpty(attachmentUrl))
            {
                var fullUrl = attachmentUrl;
                if (!fullUrl.StartsWith("http://") && !fullUrl.StartsWith("https://") && !(fullUrl.Length >= 2 && fullUrl[1] == ':'))
                    fullUrl = $"http://localhost:5237/{attachmentUrl.TrimStart('/')}";

                var viewer = new ImageViewerWindow(fullUrl);
                viewer.Owner = this;
                viewer.ShowDialog();
            }
        }
    }
}
