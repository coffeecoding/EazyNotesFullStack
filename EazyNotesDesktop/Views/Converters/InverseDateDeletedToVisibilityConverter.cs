using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace EazyNotesDesktop.Views
{
    // Used for visibility of Trash / Restore Buttons depending on DateDeleted being null or not
    public class InverseDateDeletedToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
