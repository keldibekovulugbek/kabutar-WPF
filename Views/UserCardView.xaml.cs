using System;
using System.Windows;
using System.Windows.Media.Imaging;
using Kabutar_WPF.Models.Users;

namespace Kabutar_WPF.Views
{
    public partial class UserCardView : Window
    {
        private readonly UserProfileDTO? _userProfile;
        private readonly bool _isOnline;

        public UserCardView(UserProfileDTO? userProfile, bool isOnline = false)
        {
            InitializeComponent();
            _userProfile = userProfile;
            _isOnline = isOnline;

            Loaded += (s, e) => LoadUserInfo();
        }

        private async void LoadUserInfo()
        {
            if (_userProfile == null)
            {
                Close();
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
                OnlineStatus.Text = "Online";
            }
            else
            {
                OnlineStatus.Text = "Offline";
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

        private string GetFullImageUrl(string imagePath)
        {
            if (imagePath.StartsWith("http://") || imagePath.StartsWith("https://"))
                return imagePath;

            return $"http://localhost:5237/{imagePath.TrimStart('/')}";
        }

        private void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
