using EazyNotesDesktop.ViewModels;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace EazyNotesDesktop.Views
{
    public class NoteTypeToCheckListNoteVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            BaseNoteViewModel itemViewModel = value as BaseNoteViewModel;
            if (itemViewModel == null)
                return Visibility.Collapsed;
            else if (itemViewModel is CheckListNoteViewModel)
                return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
