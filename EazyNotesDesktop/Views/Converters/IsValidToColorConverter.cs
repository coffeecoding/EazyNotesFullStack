using System;
using System.Globalization;
using System.Windows.Data;

namespace EazyNotesDesktop.Views
{
    public class ValidToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool? isValid = (bool?)value;
            if (isValid == true)
            {
                return App.Current.FindResource("GreenBrush");
            }
            else if (isValid == false)
            {
                return App.Current.FindResource("RedBrush");
            }
            return App.Current.FindResource("BorderBrush");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}