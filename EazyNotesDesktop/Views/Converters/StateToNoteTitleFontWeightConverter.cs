using EazyNotesDesktop.ViewModels;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace EazyNotesDesktop.Views
{
    public class StateToNoteTitleFontWeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            EntityState state = (EntityState)value;
            if (state == EntityState.Default)
                return FontWeights.Normal;
            return FontWeights.Bold;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
