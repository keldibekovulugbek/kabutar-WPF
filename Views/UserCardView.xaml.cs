using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Kabutar_WPF.Models.Users;

namespace Kabutar_WPF.Views
{
    public partial class UserCardView : UserControl
    {
        private readonly UserProfileDTO? _userProfile;
        private readonly bool _isOnline;
        private readonly DateTime? _lastActive;

        public event Action? CloseRequested;

        public UserCardView(UserProfileDTO? userProfile, bool isOnline = false, DateTime? lastActive = null)
        {
            InitializeComponent();
            _userProfile = userProfile;
            _isOnline = isOnline;
            _lastActive = lastActive;

            Loaded += (s, e) => LoadUserInfo();
        }

        private async void LoadUserInfo()
        {
            if (_userProfile == null)
            {
                CloseRequested?.Invoke();
                return;
            }

            var fullName = _userProfile.Fullname;
            if (string.IsNullOrWhiteSpace(fullName))
                fullName = $"{_userProfile.FirstName} {_userProfile.LastName}".Trim();
            if (string.IsNullOrWhiteSpace(fullName))
                fullName = _userProfile.Username;

            UserFullName.Text = fullName;

            var firstInitial = string.IsNullOrEmpty(_userProfile.FirstName) ? "" : _userProfile.FirstName[0].ToString();
            var lastInitial = string.IsNullOrEmpty(_userProfile.LastName) ? "" : _userProfile.LastName[0].ToString();
            var initials = (firstInitial + lastInitial).ToUpper();

            if (string.IsNullOrEmpty(initials) && !string.IsNullOrEmpty(_userProfile.Username))
                initials = _userProfile.Username[0].ToString().ToUpper();

            UserInitials.Text = initials;

            UserUsername.Text = $"@{_userProfile.Username}";

            if (!string.IsNullOrEmpty(_userProfile.About))
            {
                UserAbout.Text = _userProfile.About;
                AboutSection.Visibility = Visibility.Visible;
            }

            if (_isOnline)
            {
                OnlineIndicator.Visibility = Visibility.Visible;
                OnlineStatus.Text = "Onlayn";
            }
            else
            {
                OnlineStatus.Text = _lastActive.HasValue ? FormatLastSeen(_lastActive.Value) : "so'nggi faollik noma'lum";
            }

            if (!string.IsNullOrEmpty(_userProfile.ProfilePicture))
            {
                await LoadProfileImageAsync(_userProfile.ProfilePicture);
            }
        }

        private async System.Threading.Tasks.Task LoadProfileImageAsync(string imagePath)
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

                    UserImage.ImageSource = bitmap;
                    UserImageBorder.Visibility = Visibility.Visible;
                });
            }
            catch
            {
                UserImageBorder.Visibility = Visibility.Collapsed;
            }
        }

        private string FormatLastSeen(DateTime lastActive)
        {
            var diff = DateTime.Now - lastActive;
            if (diff.TotalMinutes < 1) return "hozirgina chiqdi";
            if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes} daqiqa oldin";
            if (diff.TotalHours < 24) return $"{(int)diff.TotalHours} soat oldin";
            if (diff.TotalDays < 7) return $"{(int)diff.TotalDays} kun oldin";
            return lastActive.ToString("d-MMMM");
        }

        private string GetFullImageUrl(string imagePath)
        {
            if (imagePath.StartsWith("http://") || imagePath.StartsWith("https://"))
                return imagePath;

            return $"http://localhost:5237/{imagePath.TrimStart('/')}";
        }

        private void UserImageBorder_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (UserImage.ImageSource is System.Windows.Media.ImageSource imageSource)
            {
                var window = new Window
                {
                    Title = "Rasm",
                    WindowStyle = WindowStyle.None,
                    WindowState = WindowState.Maximized,
                    Background = System.Windows.Media.Brushes.Black,
                    Content = new System.Windows.Controls.Image
                    {
                        Source = imageSource,
                        Stretch = System.Windows.Media.Stretch.Uniform
                    }
                };
                window.MouseLeftButtonUp += (s, _) => window.Close();
                window.KeyDown += (s, ke) => { if (ke.Key == System.Windows.Input.Key.Escape) window.Close(); };
                window.ShowDialog();
            }
        }

        private void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            CloseRequested?.Invoke();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            CloseRequested?.Invoke();
        }
    }
}
