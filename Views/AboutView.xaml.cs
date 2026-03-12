using System;
using System.Windows;
using System.Windows.Controls;

namespace Kabutar_WPF.Views
{
    public partial class AboutView : UserControl
    {
        public event Action? CloseRequested;

        public AboutView()
        {
            InitializeComponent();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            CloseRequested?.Invoke();
        }
    }
}
