using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace EazyNotesDesktop.Views
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Used to differentiate between Visibility.Collapsed (parameter=null) and Visibility.Hidden
        /// </summary>
        /// <param name="parameter">Used to differentiate between Visibility.Collapsed (parameter=null) and Visibility.Hidden</param>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility result = (bool)value
                ? Visibility.Visible
                : (parameter == null) 
                    ? Visibility.Collapsed
                    : Visibility.Hidden;
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool result = (Visibility)value == Visibility.Visible ? true : false;
            return result;
        }
    }
}
