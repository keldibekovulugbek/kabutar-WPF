using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Kabutar_WPF.Helpers
{
    public enum NotificationType
    {
        Success,
        Error,
        Warning,
        Info
    }

    public static class NotificationService
    {
        private static Border? _currentNotification;
        private static Grid? _notificationContainer;

        public static void Initialize(Grid container)
        {
            _notificationContainer = container;
        }

        public static void Show(string message, NotificationType type = NotificationType.Info, int durationMs = 3000)
        {
            if (_notificationContainer == null)
                return;

            // Dismiss any existing notification
            if (_currentNotification != null)
            {
                DismissNotification();
            }

            // Create notification UI
            var notification = CreateNotification(message, type);
            _currentNotification = notification;

            // Add to container
            _notificationContainer.Children.Add(notification);

            // Animate in
            AnimateIn(notification);

            // Auto dismiss after duration
            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(durationMs)
            };
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                DismissNotification();
            };
            timer.Start();
        }

        private static Border CreateNotification(string message, NotificationType type)
        {
            // Determine colors based on type
            Brush backgroundColor;
            Brush iconColor;
            string icon;

            switch (type)
            {
                case NotificationType.Success:
                    backgroundColor = new SolidColorBrush(Color.FromRgb(76, 175, 80)); // Green
                    iconColor = Brushes.White;
                    icon = "✓";
                    break;
                case NotificationType.Error:
                    backgroundColor = new SolidColorBrush(Color.FromRgb(244, 67, 54)); // Red
                    iconColor = Brushes.White;
                    icon = "✕";
                    break;
                case NotificationType.Warning:
                    backgroundColor = new SolidColorBrush(Color.FromRgb(255, 152, 0)); // Orange
                    iconColor = Brushes.White;
                    icon = "!";
                    break;
                default: // Info
                    backgroundColor = new SolidColorBrush(Color.FromRgb(33, 150, 243)); // Blue
                    iconColor = Brushes.White;
                    icon = "ℹ";
                    break;
            }

            var border = new Border
            {
                Background = backgroundColor,
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(20, 15, 20, 15),
                MinWidth = 300,
                MaxWidth = 500,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 20, 0, 0),
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Colors.Black,
                    Opacity = 0.3,
                    BlurRadius = 15,
                    ShadowDepth = 5
                },
                RenderTransform = new TranslateTransform(0, -100),
                Opacity = 0
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Icon
            var iconText = new TextBlock
            {
                Text = icon,
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                Foreground = iconColor,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 12, 0)
            };
            Grid.SetColumn(iconText, 0);
            grid.Children.Add(iconText);

            // Message
            var messageText = new TextBlock
            {
                Text = message,
                FontSize = 14,
                Foreground = Brushes.White,
                TextWrapping = TextWrapping.Wrap,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(messageText, 1);
            grid.Children.Add(messageText);

            border.Child = grid;
            Panel.SetZIndex(border, 1000);

            return border;
        }

        private static void AnimateIn(Border notification)
        {
            var slideAnimation = new DoubleAnimation
            {
                From = -100,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            var fadeAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(300)
            };

            var transform = (TranslateTransform)notification.RenderTransform;
            transform.BeginAnimation(TranslateTransform.YProperty, slideAnimation);
            notification.BeginAnimation(UIElement.OpacityProperty, fadeAnimation);
        }

        private static void DismissNotification()
        {
            if (_currentNotification == null || _notificationContainer == null)
                return;

            var notification = _currentNotification;

            var slideAnimation = new DoubleAnimation
            {
                From = 0,
                To = -100,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };

            var fadeAnimation = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(200)
            };

            slideAnimation.Completed += (s, e) =>
            {
                _notificationContainer?.Children.Remove(notification);
            };

            var transform = (TranslateTransform)notification.RenderTransform;
            transform.BeginAnimation(TranslateTransform.YProperty, slideAnimation);
            notification.BeginAnimation(UIElement.OpacityProperty, fadeAnimation);

            _currentNotification = null;
        }
    }
}
