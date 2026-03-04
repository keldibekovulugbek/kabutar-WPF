using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Kabutar_WPF.Services;
using Kabutar_WPF.Models.Users;
using Kabutar_WPF.Helpers;

namespace Kabutar_WPF.Views
{
    public partial class ThemeSettingsView : Window
    {
        private readonly IUserService _userService;
        private string _selectedTheme = "light";
        private string _selectedFontSize = "medium";
        private string? _selectedBackgroundPath;
        private string? _currentBackgroundUrl;
        private bool _clearBackground = false;

        public ThemeSettingsView(IUserService userService)
        {
            InitializeComponent();
            _userService = userService;

            Loaded += async (s, e) => await LoadCurrentSettings();
        }

        private async System.Threading.Tasks.Task LoadCurrentSettings()
        {
            try
            {
                var settings = await _userService.GetSettingsAsync();

                if (settings != null)
                {
                    _selectedTheme = settings.Theme;
                    _selectedFontSize = settings.FontSize;
                    _currentBackgroundUrl = settings.ChatBackgroundImage;

                    UpdateThemeSelection();
                    UpdateFontSizeSelection();
                    UpdateFontPreview();

                    if (!string.IsNullOrEmpty(settings.ChatBackgroundImage))
                    {
                        try
                        {
                            var imageUrl = GetFullImageUrl(settings.ChatBackgroundImage);
                            var bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.UriSource = new Uri(imageUrl);
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.EndInit();

                            BackgroundPreviewImage.ImageSource = bitmap;
                            CustomBackgroundBorder.Visibility = Visibility.Visible;
                            DefaultBackgroundBorder.Visibility = Visibility.Collapsed;
                            ClearBackgroundButton.Visibility = Visibility.Visible;
                        }
                        catch
                        {
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NotificationService.Show($"Sozlamalarni yuklashda xatolik: {ex.Message}", NotificationType.Error);
            }
        }

        private string GetFullImageUrl(string imagePath)
        {
            if (imagePath.StartsWith("http://") || imagePath.StartsWith("https://"))
                return imagePath;

            return $"http://localhost:5237/{imagePath.TrimStart('/')}";
        }

        private void UpdateThemeSelection()
        {
            var primaryBrush = (SolidColorBrush)FindResource("PrimaryBrush");
            var borderBrush = (SolidColorBrush)FindResource("BorderBrush");

            LightThemeButton.BorderBrush = _selectedTheme == "light" ? primaryBrush : borderBrush;
            DarkThemeButton.BorderBrush = _selectedTheme == "dark" ? primaryBrush : borderBrush;
        }

        private void UpdateFontSizeSelection()
        {
            var primaryBrush = (SolidColorBrush)FindResource("PrimaryBrush");
            var borderBrush = (SolidColorBrush)FindResource("BorderBrush");

            SmallFontButton.BorderBrush = _selectedFontSize == "small" ? primaryBrush : borderBrush;
            MediumFontButton.BorderBrush = _selectedFontSize == "medium" ? primaryBrush : borderBrush;
            LargeFontButton.BorderBrush = _selectedFontSize == "large" ? primaryBrush : borderBrush;
        }

        private void UpdateFontPreview()
        {
            FontPreviewText.FontSize = _selectedFontSize switch
            {
                "small" => 12,
                "large" => 18,
                _ => 14
            };
        }

        private void LightTheme_Click(object sender, MouseButtonEventArgs e)
        {
            _selectedTheme = "light";
            UpdateThemeSelection();
        }

        private void DarkTheme_Click(object sender, MouseButtonEventArgs e)
        {
            _selectedTheme = "dark";
            UpdateThemeSelection();
        }

        private void SmallFont_Click(object sender, MouseButtonEventArgs e)
        {
            _selectedFontSize = "small";
            UpdateFontSizeSelection();
            UpdateFontPreview();
        }

        private void MediumFont_Click(object sender, MouseButtonEventArgs e)
        {
            _selectedFontSize = "medium";
            UpdateFontSizeSelection();
            UpdateFontPreview();
        }

        private void LargeFont_Click(object sender, MouseButtonEventArgs e)
        {
            _selectedFontSize = "large";
            UpdateFontSizeSelection();
            UpdateFontPreview();
        }

        private void SelectBackground_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Chat orqa fonini tanlang",
                Filter = "Rasm fayllari (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png",
                FilterIndex = 1
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _selectedBackgroundPath = openFileDialog.FileName;
                _clearBackground = false;

                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(_selectedBackgroundPath);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    BackgroundPreviewImage.ImageSource = bitmap;
                    CustomBackgroundBorder.Visibility = Visibility.Visible;
                    DefaultBackgroundBorder.Visibility = Visibility.Collapsed;
                    ClearBackgroundButton.Visibility = Visibility.Visible;
                }
                catch
                {
                    NotificationService.Show("Rasmni yuklashda xatolik", NotificationType.Error);
                }
            }
        }

        private void ClearBackground_Click(object sender, RoutedEventArgs e)
        {
            _selectedBackgroundPath = null;
            _clearBackground = true;

            CustomBackgroundBorder.Visibility = Visibility.Collapsed;
            DefaultBackgroundBorder.Visibility = Visibility.Visible;
            ClearBackgroundButton.Visibility = Visibility.Collapsed;
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveButton = sender as System.Windows.Controls.Button;
                if (saveButton != null)
                    saveButton.IsEnabled = false;

                if (!string.IsNullOrEmpty(_selectedBackgroundPath))
                {
                    await _userService.UploadChatBackgroundAsync(_selectedBackgroundPath);
                }

                var updateRequest = new UserSettingsUpdateRequest
                {
                    Theme = _selectedTheme,
                    FontSize = _selectedFontSize,
                    ChatBackgroundImage = _clearBackground ? "" : null
                };

                await _userService.UpdateSettingsAsync(updateRequest);

                ApplyTheme(_selectedTheme);

                ApplyFontSize(_selectedFontSize);

                if (Owner is MainView mainView)
                {
                    await mainView.RefreshSettingsAsync();
                }

                NotificationService.Show("Sozlamalar saqlandi!", NotificationType.Success);

                await System.Threading.Tasks.Task.Delay(1000);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                NotificationService.Show($"Xatolik: {ex.Message}", NotificationType.Error);

                var saveButton = sender as System.Windows.Controls.Button;
                if (saveButton != null)
                    saveButton.IsEnabled = true;
            }
        }

        private void ApplyTheme(string theme)
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

        private void ApplyFontSize(string fontSize)
        {
            double fontSizeValue = fontSize switch
            {
                "small" => 12.0,
                "large" => 18.0,
                _ => 14.0
            };

            Application.Current.Resources["ChatFontSize"] = fontSizeValue;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
