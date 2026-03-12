using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Kabutar_WPF.Models.Users;
using Kabutar_WPF.Services;
using Kabutar_WPF.Helpers;

namespace Kabutar_WPF.Views
{
    public partial class ProfileSettingsView : UserControl
    {
        private readonly IUserService _userService;
        private readonly IAuthService _authService;
        private string? _selectedImagePath;
        private string? _selectedThumbnailPath;

        public event Action? CloseRequested;

        public ProfileSettingsView(IUserService userService, IAuthService authService)
        {
            InitializeComponent();
            _userService = userService;
            _authService = authService;

            Loaded += async (s, e) => await LoadCurrentUserData();
            UsernameTextBox.TextChanged += (s, e) =>
            {
                UsernameError.Visibility = Visibility.Collapsed;
                UsernameTextBox.ClearValue(System.Windows.Controls.TextBox.BorderBrushProperty);
            };
        }

        private async System.Threading.Tasks.Task LoadCurrentUserData()
        {
            try
            {
                var userId = _authService.GetUserId();

                if (userId == null)
                {
                    ShowNotification("Foydalanuvchi ma'lumotlari topilmadi.", NotificationType.Error);
                    CloseRequested?.Invoke();
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

        private static BitmapSource ApplyExifRotation(BitmapSource source, string sourcePath)
        {
            try
            {
                using var stream = System.IO.File.OpenRead(sourcePath);
                var decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                if (decoder.Frames[0].Metadata is BitmapMetadata meta
                    && meta.ContainsQuery("/app1/ifd/{ushort=274}"))
                {
                    int orientation = (int)(ushort)meta.GetQuery("/app1/ifd/{ushort=274}");
                    double angle = orientation switch { 3 => 180, 6 => 90, 8 => -90, _ => 0 };
                    if (angle != 0)
                        return new System.Windows.Media.Imaging.TransformedBitmap(
                            source, new System.Windows.Media.RotateTransform(angle));
                }
            }
            catch { }
            return source;
        }

        private static BitmapSource RenderToSize(BitmapSource source, int width, int height)
        {
            var visual = new System.Windows.Media.DrawingVisual();
            using (var ctx = visual.RenderOpen())
                ctx.DrawImage(source, new System.Windows.Rect(0, 0, width, height));
            var rt = new System.Windows.Media.Imaging.RenderTargetBitmap(
                width, height, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);
            rt.Render(visual);
            rt.Freeze();
            return rt;
        }

        private static string SaveAsJpeg(BitmapSource source, int quality, string prefix)
        {
            var encoder = new System.Windows.Media.Imaging.JpegBitmapEncoder { QualityLevel = quality };
            encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(source));
            var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"{prefix}_{Guid.NewGuid()}.jpg");
            using var fs = System.IO.File.OpenWrite(path);
            encoder.Save(fs);
            return path;
        }

        private (string fullPath, string thumbPath) CropAndResizeImage(string sourcePath)
        {

            var originalBitmap = new BitmapImage();
            originalBitmap.BeginInit();
            originalBitmap.UriSource = new Uri(sourcePath);
            originalBitmap.CacheOption = BitmapCacheOption.OnLoad;
            originalBitmap.EndInit();
            originalBitmap.Freeze();


            BitmapSource source = ApplyExifRotation(originalBitmap, sourcePath);

            int origWidth = source.PixelWidth;
            int origHeight = source.PixelHeight;


            int size = Math.Min(origWidth, origHeight);
            int x = (origWidth - size) / 2;
            int y = (origHeight - size) / 2;
            var cropped = new System.Windows.Media.Imaging.CroppedBitmap(
                source, new System.Windows.Int32Rect(x, y, size, size));


            string fullPath;
            long originalFileSize = new System.IO.FileInfo(sourcePath).Length;
            if (originalFileSize > 5 * 1024 * 1024)
            {

                var mainSize = Math.Min(size, 1200);
                var mainResized = RenderToSize(cropped, mainSize, mainSize);
                fullPath = SaveAsJpeg(mainResized, 80, "kabutar_profile");
            }
            else
            {

                fullPath = SaveAsJpeg(cropped, 92, "kabutar_profile");
            }


            var thumb = RenderToSize(cropped, 80, 80);
            var thumbPath = SaveAsJpeg(thumb, 50, "kabutar_thumb");

            return (fullPath, thumbPath);
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
                try
                {
                    var (fullPath, thumbPath) = CropAndResizeImage(openFileDialog.FileName);
                    _selectedImagePath = fullPath;
                    _selectedThumbnailPath = thumbPath;
                    SelectedImagePath.Text = System.IO.Path.GetFileName(openFileDialog.FileName);

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

                UsernameError.Visibility = Visibility.Collapsed;

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
                        await _userService.UploadProfileImageAsync(_selectedImagePath, _selectedThumbnailPath);
                    }
                    catch (Exception imgEx)
                    {
                        ShowNotification($"Rasmni yuklashda xatolik: {imgEx.Message}. Lekin boshqa ma'lumotlar saqlandi.", NotificationType.Warning, 5000);
                    }
                }

                ShowNotification("Profil muvaffaqiyatli yangilandi!", NotificationType.Success);

                await System.Threading.Tasks.Task.Delay(1500);
                CloseRequested?.Invoke();
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                if (msg.Contains("already exists") || msg.Contains("already taken") || msg.Contains("mavjud"))
                {
                    UsernameError.Text = "Bu username allaqachon band. Boshqa nom tanlang.";
                    UsernameError.Visibility = Visibility.Visible;
                    UsernameTextBox.BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(229, 57, 53));
                    UsernameTextBox.Focus();
                }
                else
                {
                    ShowNotification($"Xatolik yuz berdi: {msg}", NotificationType.Error);
                }

                var saveButton = sender as System.Windows.Controls.Button;
                if (saveButton != null)
                    saveButton.IsEnabled = true;
            }
        }

        private void ShowNotification(string message, NotificationType type, int durationMs = 3000)
        {
            NotificationService.Show(message, type, durationMs);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            CloseRequested?.Invoke();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            CloseRequested?.Invoke();
        }
    }
}
