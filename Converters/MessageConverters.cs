using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Kabutar_WPF.Converters
{
    public class MessageBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isFromMe)
            {
                var key = isFromMe ? "SentMessageBrush" : "ReceivedMessageBrush";
                if (System.Windows.Application.Current.Resources.Contains(key))
                    return System.Windows.Application.Current.Resources[key];
                return isFromMe
                    ? new SolidColorBrush(Color.FromRgb(139, 92, 246))
                    : new SolidColorBrush(Color.FromRgb(240, 242, 245));
            }
            return new SolidColorBrush(Color.FromRgb(240, 242, 245));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }

    public class MessageForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isFromMe)
            {
                var key = isFromMe ? "SentMessageTextBrush" : "ReceivedMessageTextBrush";
                if (System.Windows.Application.Current.Resources.Contains(key))
                    return System.Windows.Application.Current.Resources[key];
                return isFromMe
                    ? new SolidColorBrush(Colors.White)
                    : new SolidColorBrush(Color.FromRgb(33, 37, 41));
            }
            return new SolidColorBrush(Color.FromRgb(33, 37, 41));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }

    public class MessageTimeForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isFromMe)
            {
                var key = isFromMe ? "SentMessageTimeBrush" : "ReceivedMessageTimeBrush";
                if (System.Windows.Application.Current.Resources.Contains(key))
                    return System.Windows.Application.Current.Resources[key];
                return isFromMe
                    ? new SolidColorBrush(Color.FromRgb(230, 240, 255))
                    : new SolidColorBrush(Color.FromRgb(108, 117, 125));
            }
            return new SolidColorBrush(Color.FromRgb(108, 117, 125));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }

    public class MessageAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isFromMe)
            {
                return isFromMe ? HorizontalAlignment.Right : HorizontalAlignment.Left;
            }
            return HorizontalAlignment.Left;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class OnlineStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isOnline)
            {
                return isOnline ? "Onlayn" : "Offline";
            }
            return "Offline";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class LastSeenMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2)
                return "Offline";

            bool isOnline = values[0] is bool online && online;
            DateTime? lastActive = values[1] as DateTime?;

            if (isOnline)
                return "Onlayn";

            if (lastActive.HasValue)
                return FormatLastSeen(lastActive.Value);

            return "so'nggi faollik noma'lum";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private string FormatLastSeen(DateTime lastActive)
        {
            var now = DateTime.Now;
            var diff = now - lastActive;

            if (diff.TotalMinutes < 1)
                return "hozirgina chiqdi";
            if (diff.TotalMinutes < 60)
                return $"{(int)diff.TotalMinutes} daqiqa oldin";
            if (diff.TotalHours < 24)
                return $"{(int)diff.TotalHours} soat oldin";
            if (diff.TotalDays < 7)
                return $"{(int)diff.TotalDays} kun oldin";

            return lastActive.ToString("d-MMMM");
        }
    }


    public class BoolToPaddingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool hasImage && hasImage)
                return new Thickness(0);
            return new Thickness(10, 8, 10, 8);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class ImageBubbleBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {


            throw new NotImplementedException();
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
