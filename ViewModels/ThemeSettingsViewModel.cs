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
    public class ThemeSettingsViewModel : ObservableObject
    {
        private readonly IUserService _userService;
        private string _selectedTheme = "light";
        private string _selectedFontSize = "medium";
        private string? _selectedBackgroundPath;
        private bool _hasCustomBackground;
        private bool _clearBackground;
        private BitmapSource? _backgroundPreviewImage;

        public event Action? CloseRequested;
        public event Action? SettingsSaved;

        public ThemeSettingsViewModel(IUserService userService)
        {
            _userService = userService;

            SelectLightThemeCommand = new RelayCommand(_ => SelectedTheme = "light");
            SelectDarkThemeCommand = new RelayCommand(_ => SelectedTheme = "dark");
            SelectSmallFontCommand = new RelayCommand(_ => SelectedFontSize = "small");
            SelectMediumFontCommand = new RelayCommand(_ => SelectedFontSize = "medium");
            SelectLargeFontCommand = new RelayCommand(_ => SelectedFontSize = "large");
            ClearBackgroundCommand = new RelayCommand(_ => ClearBackground());
            SaveCommand = new RelayCommand(async _ => await SaveAsync());
            CloseCommand = new RelayCommand(_ => CloseRequested?.Invoke());
        }

        public string SelectedTheme
        {
            get => _selectedTheme;
            set
            {
                SetProperty(ref _selectedTheme, value);
                OnPropertyChanged(nameof(IsLightTheme));
                OnPropertyChanged(nameof(IsDarkTheme));
            }
        }

        public string SelectedFontSize
        {
            get => _selectedFontSize;
            set
            {
                SetProperty(ref _selectedFontSize, value);
                OnPropertyChanged(nameof(IsSmallFont));
                OnPropertyChanged(nameof(IsMediumFont));
                OnPropertyChanged(nameof(IsLargeFont));
                OnPropertyChanged(nameof(PreviewFontSize));
            }
        }

        public bool IsLightTheme => _selectedTheme == "light";
        public bool IsDarkTheme => _selectedTheme == "dark";
        public bool IsSmallFont => _selectedFontSize == "small";
        public bool IsMediumFont => _selectedFontSize == "medium";
        public bool IsLargeFont => _selectedFontSize == "large";

        public double PreviewFontSize => _selectedFontSize switch
        {
            "small" => 12,
            "large" => 18,
            _ => 14
        };

        public bool HasCustomBackground
        {
            get => _hasCustomBackground;
            set => SetProperty(ref _hasCustomBackground, value);
        }

        public BitmapSource? BackgroundPreviewImage
        {
            get => _backgroundPreviewImage;
            set => SetProperty(ref _backgroundPreviewImage, value);
        }

        public ICommand SelectLightThemeCommand { get; }
        public ICommand SelectDarkThemeCommand { get; }
        public ICommand SelectSmallFontCommand { get; }
        public ICommand SelectMediumFontCommand { get; }
        public ICommand SelectLargeFontCommand { get; }
        public ICommand ClearBackgroundCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CloseCommand { get; }

        public async Task LoadAsync()
        {
            try
            {
                var settings = await _userService.GetSettingsAsync();
                if (settings == null) return;

                SelectedTheme = settings.Theme ?? "light";
                SelectedFontSize = settings.FontSize ?? "medium";

                if (!string.IsNullOrEmpty(settings.ChatBackgroundImage))
                {
                    var bitmap = await LoadBitmapFromUrlAsync(GetFullImageUrl(settings.ChatBackgroundImage));
                    if (bitmap != null)
                    {
                        BackgroundPreviewImage = bitmap;
                        HasCustomBackground = true;
                    }
                }
            }
            catch (Exception ex)
            {
                NotificationService.Show($"Sozlamalarni yuklashda xatolik: {ex.Message}", NotificationType.Error);
            }
        }

        public void SetSelectedBackground(string path, BitmapSource preview)
        {
            _selectedBackgroundPath = path;
            _clearBackground = false;
            BackgroundPreviewImage = preview;
            HasCustomBackground = true;
        }

        private void ClearBackground()
        {
            _selectedBackgroundPath = null;
            _clearBackground = true;
            BackgroundPreviewImage = null;
            HasCustomBackground = false;
        }

        private async Task SaveAsync()
        {
            try
            {
                if (!string.IsNullOrEmpty(_selectedBackgroundPath))
                    await _userService.UploadChatBackgroundAsync(_selectedBackgroundPath);

                var updateRequest = new UserSettingsUpdateRequest
                {
                    Theme = SelectedTheme,
                    FontSize = SelectedFontSize,
                    ChatBackgroundImage = _clearBackground ? "" : null
                };

                await _userService.UpdateSettingsAsync(updateRequest);

                MainViewModel.ApplyTheme(SelectedTheme);
                MainViewModel.ApplyFontSize(SelectedFontSize);

                NotificationService.Show("Sozlamalar saqlandi!", NotificationType.Success);
                SettingsSaved?.Invoke();
                await Task.Delay(1000);
                CloseRequested?.Invoke();
            }
            catch (Exception ex)
            {
                NotificationService.Show($"Xatolik: {ex.Message}", NotificationType.Error);
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
