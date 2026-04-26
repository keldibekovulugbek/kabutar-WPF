using System;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Kabutar_WPF.ViewModels;

namespace Kabutar_WPF.Views
{
    public partial class ProfileSettingsView : System.Windows.Controls.UserControl
    {
        public event Action? CloseRequested;

        public ProfileSettingsView(ProfileSettingsViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            viewModel.CloseRequested += () => CloseRequested?.Invoke();
            Loaded += async (s, e) => await viewModel.LoadAsync();
        }

        private void SelectImage_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not ProfileSettingsViewModel vm) return;

            var openFileDialog = new OpenFileDialog
            {
                Title = "Profil rasmini tanlang",
                Filter = "Rasm fayllari (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png",
                FilterIndex = 1
            };

            if (openFileDialog.ShowDialog() != true) return;

            try
            {
                var (fullPath, thumbPath) = CropAndResizeImage(openFileDialog.FileName);

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(fullPath);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();

                vm.SetSelectedImage(fullPath, thumbPath, bitmap);
            }
            catch
            {
                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(openFileDialog.FileName);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();

                    vm.SetSelectedImage(openFileDialog.FileName, null, bitmap);
                }
                catch { }
            }
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
                ctx.DrawImage(source, new Rect(0, 0, width, height));
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
    }
}
