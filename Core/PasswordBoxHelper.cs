using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Kabutar_WPF.Core
{
    public static class PasswordBoxHelper
    {
        public static readonly DependencyProperty HasPasswordProperty =
            DependencyProperty.RegisterAttached(
                "HasPassword",
                typeof(bool),
                typeof(PasswordBoxHelper),
                new PropertyMetadata(false));

        public static bool GetHasPassword(DependencyObject obj)
        {
            return (bool)obj.GetValue(HasPasswordProperty);
        }

        public static void SetHasPassword(DependencyObject obj, bool value)
        {
            obj.SetValue(HasPasswordProperty, value);
        }

        public static readonly DependencyProperty AttachProperty =
            DependencyProperty.RegisterAttached(
                "Attach",
                typeof(bool),
                typeof(PasswordBoxHelper),
                new PropertyMetadata(false, OnAttachChanged));

        public static bool GetAttach(DependencyObject obj)
        {
            return (bool)obj.GetValue(AttachProperty);
        }

        public static void SetAttach(DependencyObject obj, bool value)
        {
            obj.SetValue(AttachProperty, value);
        }

        private static void OnAttachChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PasswordBox passwordBox)
            {
                if ((bool)e.NewValue)
                {
                    passwordBox.PasswordChanged += PasswordBox_PasswordChanged;
                }
                else
                {
                    passwordBox.PasswordChanged -= PasswordBox_PasswordChanged;
                }
            }
        }

        private static void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                SetHasPassword(passwordBox, passwordBox.Password.Length > 0);
                UpdatePlaceholderVisibility(passwordBox);
            }
        }

        private static void UpdatePlaceholderVisibility(PasswordBox passwordBox)
        {
            var placeholder = FindPlaceholder(passwordBox);
            if (placeholder != null)
            {
                placeholder.Visibility = passwordBox.Password.Length > 0 ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        private static TextBlock? FindPlaceholder(DependencyObject obj)
        {
            if (obj is TextBlock textBlock && textBlock.Name == "PlaceholderText")
            {
                return textBlock;
            }

            int childCount = VisualTreeHelper.GetChildrenCount(obj);
            for (int i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);
                var result = FindPlaceholder(child);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }
    }
}
