using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Kabutar_WPF.Core;
using Kabutar_WPF.Helpers;
using Kabutar_WPF.Models.Users;
using Kabutar_WPF.Services;

namespace Kabutar_WPF.ViewModels
{
    public class ProfileSettingsViewModel : ObservableObject
    {
        private readonly IUserService _userService;
        private readonly IAuthService _authService;

        private string _firstName = string.Empty;
        private string _lastName = string.Empty;
        private string _username = string.Empty;
        private string _about = string.Empty;
        private string _userInitials = string.Empty;
        private string _selectedFileName = string.Empty;
        private string _usernameErrorMessage = string.Empty;
        private bool _hasProfileImage;
        private bool _showUsernameError;
        private BitmapSource? _profileImage;
        private string? _selectedImagePath;
        private string? _selectedThumbnailPath;

        public event Action? CloseRequested;

        public ProfileSettingsViewModel(IUserService userService, IAuthService authService)
        {
            _userService = userService;
            _authService = authService;

            SaveCommand = new RelayCommand(async _ => await SaveAsync());
            CloseCommand = new RelayCommand(_ => CloseRequested?.Invoke());
        }

        public string FirstName
        {
            get => _firstName;
            set => SetProperty(ref _firstName, value);
        }

        public string LastName
        {
            get => _lastName;
            set => SetProperty(ref _lastName, value);
        }

        public string Username
        {
            get => _username;
            set
            {
                SetProperty(ref _username, value);
                ShowUsernameError = false;
            }
        }

        public string About
        {
            get => _about;
            set => SetProperty(ref _about, value);
        }

        public string UserInitials
        {
            get => _userInitials;
            set => SetProperty(ref _userInitials, value);
        }

        public string SelectedFileName
        {
            get => _selectedFileName;
            set => SetProperty(ref _selectedFileName, value);
        }

        public string UsernameErrorMessage
        {
            get => _usernameErrorMessage;
            set => SetProperty(ref _usernameErrorMessage, value);
        }

        public bool HasProfileImage
        {
            get => _hasProfileImage;
            set => SetProperty(ref _hasProfileImage, value);
        }

        public bool ShowUsernameError
        {
            get => _showUsernameError;
            set => SetProperty(ref _showUsernameError, value);
        }

        public BitmapSource? ProfileImage
        {
            get => _profileImage;
            set => SetProperty(ref _profileImage, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand CloseCommand { get; }

        public async Task LoadAsync()
        {
            try
            {
                var profile = await _userService.GetCurrentUserAsync();
                if (profile == null) return;

                FirstName = profile.FirstName ?? string.Empty;
                LastName = profile.LastName ?? string.Empty;
                Username = profile.Username ?? string.Empty;
                About = profile.About ?? string.Empty;

                var fi = string.IsNullOrEmpty(profile.FirstName) ? "" : profile.FirstName[0].ToString();
                var li = string.IsNullOrEmpty(profile.LastName) ? "" : profile.LastName[0].ToString();
                var initials = (fi + li).ToUpper();
                if (string.IsNullOrEmpty(initials) && !string.IsNullOrEmpty(profile.Username))
                    initials = profile.Username[0].ToString().ToUpper();
                UserInitials = initials;

                if (!string.IsNullOrEmpty(profile.ProfilePicture))
                {
                    var bitmap = await LoadBitmapFromUrlAsync(GetFullImageUrl(profile.ProfilePicture));
                    if (bitmap != null)
                    {
                        ProfileImage = bitmap;
                        HasProfileImage = true;
                    }
                }
            }
            catch (Exception ex)
            {
                NotificationService.Show($"Ma'lumotlarni yuklashda xatolik: {ex.Message}", NotificationType.Error);
            }
        }

        public void SetSelectedImage(string filePath, string? thumbnailPath, BitmapSource preview)
        {
            _selectedImagePath = filePath;
            _selectedThumbnailPath = thumbnailPath;
            SelectedFileName = System.IO.Path.GetFileName(filePath);
            ProfileImage = preview;
            HasProfileImage = true;
        }

        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(FirstName))
            {
                NotificationService.Show("Ism kiritilishi shart.", NotificationType.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(Username))
            {
                NotificationService.Show("Username kiritilishi shart.", NotificationType.Warning);
                return;
            }

            try
            {
                ShowUsernameError = false;
                var updateRequest = new UserUpdateRequest
                {
                    Firstname = FirstName.Trim(),
                    Lastname = string.IsNullOrWhiteSpace(LastName) ? string.Empty : LastName.Trim(),
                    Username = Username.Trim(),
                    About = string.IsNullOrWhiteSpace(About) ? null : About.Trim()
                };

                var success = await _userService.UpdateProfileAsync(updateRequest);
                if (!success)
                {
                    NotificationService.Show("Profilni yangilashda xatolik.", NotificationType.Error);
                    return;
                }

                if (!string.IsNullOrEmpty(_selectedImagePath))
                {
                    try
                    {
                        await _userService.UploadProfileImageAsync(_selectedImagePath, _selectedThumbnailPath);
                    }
                    catch (Exception imgEx)
                    {
                        NotificationService.Show($"Rasmni yuklashda xatolik: {imgEx.Message}. Lekin boshqa ma'lumotlar saqlandi.", NotificationType.Warning, 5000);
                    }
                }

                NotificationService.Show("Profil muvaffaqiyatli yangilandi!", NotificationType.Success);
                await Task.Delay(1500);
                CloseRequested?.Invoke();
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                if (msg.Contains("already exists") || msg.Contains("already taken") || msg.Contains("mavjud"))
                {
                    UsernameErrorMessage = "Bu username allaqachon band. Boshqa nom tanlang.";
                    ShowUsernameError = true;
                }
                else
                {
                    NotificationService.Show($"Xatolik yuz berdi: {msg}", NotificationType.Error);
                }
            }
        }

        private static async Task<BitmapSource?> LoadBitmapFromUrlAsync(string url)
        {
            try
            {
                using var client = new System.Net.Http.HttpClient();
                var bytes = await client.GetByteArrayAsync(url);
                return await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    var bitmap = new BitmapImage();
                    using var stream = new System.IO.MemoryStream(bytes);
                    bitmap.BeginInit();
                    bitmap.StreamSource = stream;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    return (BitmapSource)bitmap;
                });
            }
            catch
            {
                return null;
            }
        }

        private static string GetFullImageUrl(string path)
        {
            if (path.StartsWith("http://") || path.StartsWith("https://")) return path;
            return $"http://localhost:5237/{path.TrimStart('/')}";
        }
    }
}
