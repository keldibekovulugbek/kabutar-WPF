using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Kabutar_WPF.Models.Chat;
using Kabutar_WPF.Services;
using Kabutar_WPF.ViewModels;
using Kabutar_WPF.Helpers;

namespace Kabutar_WPF.Views
{
    public partial class MainView : Window
    {
        private bool _isMenuOpen = false;
        private readonly SignalRService _signalRService = new SignalRService();
        private MainViewModel _viewModel;

        public MainView(MainViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;

            viewModel.ShowClearChatDialogFunc = chat =>
            {
                var dialog = new ClearChatDialog(chat, this);
                dialog.ShowDialog();
                return (dialog.Confirmed, dialog.DeleteForBoth);
            };

            viewModel.RequestClose += Close;
            DataContext = viewModel;

            NotificationService.Initialize(RootGrid);

            Loaded += async (s, e) =>
            {
                await viewModel.InitializeAsync();
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
                var token = new AuthService(ApiClient.Instance).GetToken();

                if (!string.IsNullOrEmpty(token))
                {
                    _signalRService.MessageReceived += OnMessageReceived;
                    await _signalRService.ConnectAsync(token);
                    _viewModel.SetSignalRService(_signalRService);
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
                _viewModel.ReceiveIncomingMessage(msg.SenderId, msg.Content, msg.SentAt, msg.AttachmentUrl);
            });
        }

        public async System.Threading.Tasks.Task RefreshSettingsAsync()
        {
            await _viewModel.LoadUserSettingsAsync();
        }

        private void HamburgerButton_Click(object sender, RoutedEventArgs e) => ToggleMenu();

        private void ToggleMenu()
        {
            if (_isMenuOpen) CloseMenu(); else OpenMenu();
        }

        private void OpenMenu()
        {
            _isMenuOpen = true;
            MenuOverlay.Visibility = Visibility.Visible;

            var slideIn = new DoubleAnimation
            {
                From = -280, To = 0,
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
                From = 0, To = -280,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };
            slideOut.Completed += (s, e) => MenuOverlay.Visibility = Visibility.Collapsed;
            MenuTransform.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, slideOut);
        }

        private void MenuOverlay_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => CloseMenu();

        private void ProfileSettings_Click(object sender, RoutedEventArgs e)
        {
            CloseMenu();
            var apiClient = ApiClient.Instance;
            var vm = new ProfileSettingsViewModel(new UserService(apiClient), new AuthService(apiClient));
            var profilePanel = new ProfileSettingsView(vm);
            profilePanel.CloseRequested += HideRightPanel;
            ShowRightPanel(profilePanel);
        }

        private void ChangeTheme_Click(object sender, RoutedEventArgs e)
        {
            CloseMenu();
            var vm = new ThemeSettingsViewModel(new UserService(ApiClient.Instance));
            vm.SettingsSaved += async () => await RefreshSettingsAsync();
            var themePanel = new ThemeSettingsView(vm);
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

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            CloseMenu();
            var settingsPanel = new SettingsView();
            settingsPanel.CloseRequested += HideRightPanel;
            ShowRightPanel(settingsPanel);
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            CloseMenu();
            var aboutPanel = new AboutView();
            aboutPanel.CloseRequested += HideRightPanel;
            ShowRightPanel(aboutPanel);
        }

        private async void Logout_Click(object sender, RoutedEventArgs e)
        {
            CloseMenu();
            await _signalRService.DisconnectAsync();
            _viewModel.LogoutCommand.Execute(null);
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

                double angle = orientation switch { 3 => 180, 6 => 90, 8 => -90, _ => 0 };
                if (angle == 0) return filePath;

                BitmapSource rotated = new TransformedBitmap(frame, new System.Windows.Media.RotateTransform(angle));
                var encoder = new JpegBitmapEncoder { QualityLevel = 90 };
                encoder.Frames.Add(BitmapFrame.Create(rotated));
                var tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"kabutar_img_{Guid.NewGuid()}.jpg");
                using var outStream = System.IO.File.OpenWrite(tempPath);
                encoder.Save(outStream);
                return tempPath;
            }
            catch { return filePath; }
        }

        private void SendImage_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Rasm tanlang",
                Filter = "Rasm fayllari (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png"
            };
            if (openFileDialog.ShowDialog() != true) return;
            if (_viewModel.SelectedChat == null) return;

            var fixedPath = FixExifRotation(openFileDialog.FileName);
            _viewModel.SendImageCommand.Execute(fixedPath);
        }

        private async void ChatUserInfo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_viewModel.SelectedChat == null) return;

                var userProfile = await _viewModel.GetChatUserProfileAsync(_viewModel.SelectedChat.Id);
                if (userProfile != null)
                {
                    var lastActive = userProfile.LastActive?.ToLocalTime() ?? _viewModel.SelectedChat.LastActive;
                    var isOnline = _viewModel.SelectedChat.IsOnline;
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

        // ── Drag & Drop ────────────────────────────────────────────────
        private void ChatArea_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
            e.Handled = true;
        }

        private void ChatArea_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            if (_viewModel.SelectedChat == null) return;
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            var imageExts = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
            foreach (var file in files.Where(f => imageExts.Contains(System.IO.Path.GetExtension(f).ToLower())))
            {
                var fixedPath = FixExifRotation(file);
                _viewModel.SendImageCommand.Execute(fixedPath);
            }
        }

        // ── Emoji Picker ───────────────────────────────────────────────
        private static readonly string[] Emojis =
        {
            "😊","😂","😍","🥰","😎","😢","😭","😡","🤔","😴",
            "👍","👎","❤️","🔥","✨","🎉","💯","😮","🙏","👏",
            "😁","😅","🤣","😇","😉","😋","😘","🥺","😤","🤩",
            "💪","🌟","🎊","🍕","☕","🎵","📷","🌈","⚡","🤝",
            "😆","😃","😄","😀","🙂","😏","😒","😞","😔","😟"
        };

        private bool _emojiPopupBuilt;

        private void EmojiButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_emojiPopupBuilt)
            {
                foreach (var emoji in Emojis)
                {
                    var btn = new Button
                    {
                        Content = emoji,
                        FontSize = 20,
                        Width = 36, Height = 36,
                        Background = System.Windows.Media.Brushes.Transparent,
                        BorderThickness = new Thickness(0),
                        Cursor = System.Windows.Input.Cursors.Hand,
                        ToolTip = emoji
                    };
                    btn.Click += (_, __) =>
                    {
                        InsertEmoji(emoji);
                        EmojiPopup.IsOpen = false;
                    };
                    EmojiPanel.Children.Add(btn);
                }
                _emojiPopupBuilt = true;
            }
            EmojiPopup.IsOpen = !EmojiPopup.IsOpen;
        }

        private void InsertEmoji(string emoji)
        {
            if (MessageTextBox == null) return;
            var pos = MessageTextBox.CaretIndex;
            var text = MessageTextBox.Text ?? string.Empty;
            MessageTextBox.Text = text.Insert(pos, emoji);
            MessageTextBox.CaretIndex = pos + emoji.Length;
            MessageTextBox.Focus();
        }

        // ── Message Edit ───────────────────────────────────────────────
        private void ConfirmEdit_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is MessageItem item)
                _viewModel.ConfirmEditCommand.Execute(item);
        }

        private void CancelEdit_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is MessageItem item)
                _viewModel.CancelEditCommand.Execute(item);
        }
    }
}
