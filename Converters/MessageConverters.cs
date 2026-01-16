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
                return isFromMe
                    ? new SolidColorBrush(Color.FromRgb(0, 123, 255)) // Primary blue
                    : new SolidColorBrush(Color.FromRgb(240, 242, 245)); // Light gray
            }
            return new SolidColorBrush(Color.FromRgb(240, 242, 245));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MessageForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isFromMe)
            {
                return isFromMe
                    ? new SolidColorBrush(Colors.White)
                    : new SolidColorBrush(Color.FromRgb(33, 37, 41));
            }
            return new SolidColorBrush(Color.FromRgb(33, 37, 41));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MessageTimeForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isFromMe)
            {
                return isFromMe
                    ? new SolidColorBrush(Color.FromRgb(230, 240, 255)) // Light blue
                    : new SolidColorBrush(Color.FromRgb(108, 117, 125)); // Gray
            }
            return new SolidColorBrush(Color.FromRgb(108, 117, 125));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
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
}
