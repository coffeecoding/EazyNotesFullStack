using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace EazyNotesDesktop.Views
{
    public class InverseBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility result = (bool)value ? Visibility.Collapsed : Visibility.Visible;
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool result = (Visibility)value == Visibility.Visible ? false : true;
            return result;
        }
    }
}
