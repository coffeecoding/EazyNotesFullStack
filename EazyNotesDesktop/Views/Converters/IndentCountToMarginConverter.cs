using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace EazyNotesDesktop.Views
{
    public class IndentCountToMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int indentCount = (int)value;
            int indentWidth = 50;
            return new Thickness(indentCount * indentWidth, 0, 0, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
