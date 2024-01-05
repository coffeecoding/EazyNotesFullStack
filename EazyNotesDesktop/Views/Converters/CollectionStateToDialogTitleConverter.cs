using EazyNotesDesktop.ViewModels;
using System;
using System.Globalization;
using System.Windows.Data;

namespace EazyNotesDesktop.Views
{
    public class TopicStateToDialogTitleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            EntityState topicState = (EntityState)value;
            if (topicState == EntityState.New)
                return "Create";
            return "Edit";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
