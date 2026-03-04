using System;
using System.Globalization;
using System.Net.Http;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Kabutar_WPF.Converters
{
    public class ImagePathToUrlConverter : IValueConverter
    {
        private const string BaseUrl = "http://localhost:5237/";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string imagePath && !string.IsNullOrEmpty(imagePath))
            {
                if (imagePath.StartsWith("http://") || imagePath.StartsWith("https://"))
                    return imagePath;

                return $"{BaseUrl}{imagePath.TrimStart('/')}";
            }
            return null!;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ImagePathToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string imagePath && !string.IsNullOrEmpty(imagePath))
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class NoImageToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string imagePath && !string.IsNullOrEmpty(imagePath))
            {
                return Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ReadStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isRead)
            {
                return isRead ? "✓✓" : "✓";
            }
            return "✓";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ReadStatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isRead && isRead)
            {
                return "#4FC3F7";
            }
            return "#9E9E9E";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
