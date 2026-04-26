using System;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Kabutar_WPF.ViewModels;

namespace Kabutar_WPF.Views
{
    public partial class ThemeSettingsView : System.Windows.Controls.UserControl
    {
        public event Action? CloseRequested;

        public ThemeSettingsView(ThemeSettingsViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            viewModel.CloseRequested += () => CloseRequested?.Invoke();
            Loaded += async (s, e) => await viewModel.LoadAsync();
        }

        private void SelectBackground_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not ThemeSettingsViewModel vm) return;

            var openFileDialog = new OpenFileDialog
            {
                Title = "Chat orqa fonini tanlang",
                Filter = "Rasm fayllari (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png",
                FilterIndex = 1
            };

            if (openFileDialog.ShowDialog() != true) return;

            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(openFileDialog.FileName);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();

                vm.SetSelectedBackground(openFileDialog.FileName, bitmap);
            }
            catch
            {
                Helpers.NotificationService.Show("Rasmni yuklashda xatolik", Helpers.NotificationType.Error);
            }
        }
    }
}
