using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Kabutar_WPF.Helpers;
using Kabutar_WPF.ViewModels.Auth;

namespace Kabutar_WPF.Views.Auth
{
    public partial class ForgotPasswordView : Window
    {
        public ForgotPasswordView(ForgotPasswordViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            viewModel.RequestClose += Close;
            Loaded += (s, e) => NotificationService.Initialize(RootGrid);
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
                WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            else
                DragMove();
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
        private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void Maximize_Click(object sender, RoutedEventArgs e) =>
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

        private void ThemeToggle_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button) return;
            var isDark = button.Content.ToString() == "🌙";
            var app = Application.Current;
            app.Resources.MergedDictionaries.Clear();
            app.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri(isDark
                    ? "pack://application:,,,/Resources/Themes/DarkTheme.xaml"
                    : "pack://application:,,,/Resources/Themes/LightTheme.xaml")
            });
            button.Content = isDark ? "☀" : "🌙";
            app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/Resources/Styles/ButtonStyles.xaml") });
            app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/Resources/Styles/TextBoxStyles.xaml") });
        }

        private void NewPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is ForgotPasswordViewModel vm)
                vm.NewPassword = NewPasswordBox.Password;
        }
    }
}
