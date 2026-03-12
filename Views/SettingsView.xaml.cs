using System;
using System.Windows;
using System.Windows.Controls;

namespace Kabutar_WPF.Views
{
    public partial class SettingsView : UserControl
    {
        public event Action? CloseRequested;

        public SettingsView()
        {
            InitializeComponent();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            CloseRequested?.Invoke();
        }

        private void FontSize_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.Tag is string tagValue && double.TryParse(tagValue, out double fontSize))
            {
                Application.Current.Resources["ChatFontSize"] = fontSize;
            }
        }
    }
}
