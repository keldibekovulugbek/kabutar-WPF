using System;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Kabutar_WPF.Models.Users;
using Kabutar_WPF.Services;
using Kabutar_WPF.Helpers;

namespace Kabutar_WPF.Views
{
    public partial class ProfileSettingsView : Window
    {
        private readonly IUserService _userService;
        private readonly IAuthService _authService;
        private string? _selectedImagePath;
        private Window? _parentWindow;

        public ProfileSettingsView(IUserService userService, IAuthService authService)
        {
            InitializeComponent();
            _userService = userService;
            _authService = authService;

            Loaded += async (s, e) => await LoadCurrentUserData();
        }

        private async System.Threading.Tasks.Task LoadCurrentUserData()
        {
            try
            {
                var userId = _authService.GetUserId();

                if (userId == null)
                {
                    ShowNotification("Foydalanuvchi ma'lumotlari topilmadi.", NotificationType.Error);
                    Close();
                    return;
                }

                var userProfile = await _userService.GetCurrentUserAsync();

                if (userProfile != null)
                {
                    FirstNameTextBox.Text = userProfile.FirstName ?? string.Empty;
                    LastNameTextBox.Text = userProfile.LastName ?? string.Empty;
                    UsernameTextBox.Text = userProfile.Username ?? string.Empty;
                    AboutTextBox.Text = userProfile.About ?? string.Empty;

                    var firstInitial = string.IsNullOrEmpty(userProfile.FirstName) ? "" : userProfile.FirstName[0].ToString();
                    var lastInitial = string.IsNullOrEmpty(userProfile.LastName) ? "" : userProfile.LastName[0].ToString();
                    var initials = (firstInitial + lastInitial).ToUpper();

                    if (string.IsNullOrEmpty(initials) && !string.IsNullOrEmpty(userProfile.Username))
                        initials = userProfile.Username[0].ToString().ToUpper();

                    InitialsText.Text = initials;

                    if (!string.IsNullOrEmpty(userProfile.ProfilePicture))
                    {
                        await LoadProfileImageAsync(userProfile.ProfilePicture);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowNotification($"Ma'lumotlarni yuklashda xatolik: {ex.Message}", NotificationType.Error);
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

                    ProfileImage.ImageSource = bitmap;
                    ProfileImageBorder.Visibility = Visibility.Visible;
                });
            }
            catch
            {
                ProfileImageBorder.Visibility = Visibility.Collapsed;
            }
        }

        private string GetFullImageUrl(string imagePath)
        {
            if (imagePath.StartsWith("http://") || imagePath.StartsWith("https://"))
                return imagePath;

            return $"http://localhost:5237/{imagePath.TrimStart('/')}";
        }

        private void SelectImage_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Profil rasmini tanlang",
                Filter = "Rasm fayllari (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png",
                FilterIndex = 1
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _selectedImagePath = openFileDialog.FileName;
                SelectedImagePath.Text = System.IO.Path.GetFileName(_selectedImagePath);

                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(_selectedImagePath);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    ProfileImage.ImageSource = bitmap;
                    ProfileImageBorder.Visibility = Visibility.Visible;
                }
                catch
                {
                    ProfileImageBorder.Visibility = Visibility.Collapsed;
                }
            }
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(FirstNameTextBox.Text))
                {
                    ShowNotification("Ism kiritilishi shart.", NotificationType.Warning);
                    FirstNameTextBox.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(UsernameTextBox.Text))
                {
                    ShowNotification("Username kiritilishi shart.", NotificationType.Warning);
                    UsernameTextBox.Focus();
                    return;
                }

                var saveButton = sender as System.Windows.Controls.Button;
                if (saveButton != null)
                    saveButton.IsEnabled = false;

                var updateRequest = new UserUpdateRequest
                {
                    Firstname = FirstNameTextBox.Text.Trim(),
                    Lastname = string.IsNullOrWhiteSpace(LastNameTextBox.Text) ? null : LastNameTextBox.Text.Trim(),
                    Username = UsernameTextBox.Text.Trim(),
                    About = string.IsNullOrWhiteSpace(AboutTextBox.Text) ? null : AboutTextBox.Text.Trim()
                };

                var success = await _userService.UpdateProfileAsync(updateRequest);

                if (!success)
                {
                    ShowNotification("Profilni yangilashda xatolik.", NotificationType.Error);
                    if (saveButton != null)
                        saveButton.IsEnabled = true;
                    return;
                }

                if (!string.IsNullOrEmpty(_selectedImagePath))
                {
                    try
                    {
                        await _userService.UploadProfileImageAsync(_selectedImagePath);
                    }
                    catch (Exception imgEx)
                    {
                        ShowNotification($"Rasmni yuklashda xatolik: {imgEx.Message}. Lekin boshqa ma'lumotlar saqlandi.", NotificationType.Warning, 5000);
                    }
                }

                ShowNotification("Profil muvaffaqiyatli yangilandi!", NotificationType.Success);

                await System.Threading.Tasks.Task.Delay(1500);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                ShowNotification($"Xatolik yuz berdi: {ex.Message}", NotificationType.Error);

                var saveButton = sender as System.Windows.Controls.Button;
                if (saveButton != null)
                    saveButton.IsEnabled = true;
            }
        }

        private void ShowNotification(string message, NotificationType type, int durationMs = 3000)
        {
            if (Owner is MainView mainView)
            {
                NotificationService.Show(message, type, durationMs);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
