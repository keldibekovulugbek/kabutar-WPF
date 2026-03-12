using System;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace Kabutar_WPF.Views
{
    public partial class ImageViewerWindow : Window
    {
        private readonly string _imageUrl;
        private BitmapImage? _loadedImage;
        private double _currentScale = 1.0;
        private const double MinScale = 0.5;
        private const double MaxScale = 5.0;
        private const double ZoomStep = 0.25;

        public bool IsZoomed => _currentScale > 1.01;

        public ImageViewerWindow(string imageUrl, string? title = null, string? subtitle = null)
        {
            InitializeComponent();
            _imageUrl = imageUrl;

            if (title != null) TitleText.Text = title;
            if (subtitle != null) SubtitleText.Text = subtitle;

            Loaded += async (s, e) => await LoadImageAsync();
        }

        private async System.Threading.Tasks.Task LoadImageAsync()
        {
            try
            {
                LoadingPanel.Visibility = Visibility.Visible;
                ErrorPanel.Visibility = Visibility.Collapsed;
                MainImage.Visibility = Visibility.Collapsed;

                BitmapImage bitmap;


                if (_imageUrl.Length >= 2 && _imageUrl[1] == ':')
                {
                    bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(_imageUrl);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();
                }
                else
                {
                    using var httpClient = new HttpClient();
                    var bytes = await httpClient.GetByteArrayAsync(_imageUrl);

                    bitmap = new BitmapImage();
                    using var stream = new MemoryStream(bytes);
                    bitmap.BeginInit();
                    bitmap.StreamSource = stream;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();
                }

                _loadedImage = bitmap;
                MainImage.Source = bitmap;
                MainImage.Visibility = Visibility.Visible;
                LoadingPanel.Visibility = Visibility.Collapsed;


                if (string.IsNullOrEmpty(SubtitleText.Text))
                    SubtitleText.Text = $"{bitmap.PixelWidth} × {bitmap.PixelHeight}";


                MainImage.Opacity = 0;
                var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
                MainImage.BeginAnimation(OpacityProperty, fadeIn);
            }
            catch (Exception ex)
            {
                LoadingPanel.Visibility = Visibility.Collapsed;
                ErrorPanel.Visibility = Visibility.Visible;
                ErrorText.Text = $"Rasmni yuklashda xatolik:\n{ex.Message}";
            }
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Title = "Rasmni saqlash",
                    Filter = "JPEG rasm (*.jpg)|*.jpg|PNG rasm (*.png)|*.png|Barcha fayllar (*.*)|*.*",
                    FilterIndex = 1,
                    FileName = $"kabutar_image_{DateTime.Now:yyyyMMdd_HHmmss}"
                };

                if (saveDialog.ShowDialog() != true) return;

                SaveButton.IsEnabled = false;


                if (_imageUrl.Length >= 2 && _imageUrl[1] == ':' && File.Exists(_imageUrl))
                {
                    File.Copy(_imageUrl, saveDialog.FileName, overwrite: true);
                }
                else
                {
                    using var httpClient = new HttpClient();
                    var bytes = await httpClient.GetByteArrayAsync(_imageUrl);
                    await File.WriteAllBytesAsync(saveDialog.FileName, bytes);
                }


                ShowSaveSuccess();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Saqlashda xatolik: {ex.Message}", "Xatolik",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                SaveButton.IsEnabled = true;
            }
        }

        private async void ShowSaveSuccess()
        {
            SaveButton.Background = new SolidColorBrush(Color.FromRgb(0x43, 0xA0, 0x47));
            SaveIcon.Text = "✓";
            SaveLabel.Text = "Saqlandi!";

            await System.Threading.Tasks.Task.Delay(1800);

            SaveButton.Background = new SolidColorBrush(Color.FromRgb(0x21, 0x96, 0xF3));
            SaveIcon.Text = "💾";
            SaveLabel.Text = "Saqlash";
        }


        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {

            double delta = e.Delta > 0 ? ZoomStep : -ZoomStep;
            SetZoom(_currentScale + delta);
            e.Handled = true;
        }

        private void ImageScroll_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            double delta = e.Delta > 0 ? ZoomStep : -ZoomStep;
            SetZoom(_currentScale + delta);
            e.Handled = true;
        }

        private void SetZoom(double scale)
        {
            _currentScale = Math.Clamp(scale, MinScale, MaxScale);

            ImageScale.ScaleX = _currentScale;
            ImageScale.ScaleY = _currentScale;


            if (_currentScale <= 1.0)
            {
                ImageScroll.HorizontalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Disabled;
                ImageScroll.VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Disabled;
            }
            else
            {
                ImageScroll.HorizontalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto;
                ImageScroll.VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto;
            }

            NotifyPropertyChanged(nameof(IsZoomed));
            ShowZoomIndicator();
        }

        private void ResetZoom_Click(object sender, RoutedEventArgs e)
        {
            SetZoom(1.0);
        }

        private async void ShowZoomIndicator()
        {
            ZoomText.Text = $"{(int)(_currentScale * 100)}%";


            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(150));
            ZoomIndicator.BeginAnimation(OpacityProperty, fadeIn);

            await System.Threading.Tasks.Task.Delay(1200);


            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(300));
            ZoomIndicator.BeginAnimation(OpacityProperty, fadeOut);
        }


        private void Close_Click(object sender, RoutedEventArgs e) => Close();

        private void Background_Click(object sender, MouseButtonEventArgs e) => Close();

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {

            e.Handled = true;
        }

        private void ImageScroll_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    Close();
                    break;
                case Key.Add:
                case Key.OemPlus:
                    SetZoom(_currentScale + ZoomStep);
                    break;
                case Key.Subtract:
                case Key.OemMinus:
                    SetZoom(_currentScale - ZoomStep);
                    break;
                case Key.D0:
                case Key.NumPad0:
                    SetZoom(1.0);
                    break;
            }
        }

        private void NotifyPropertyChanged(string name)
        {

        }
    }
}
