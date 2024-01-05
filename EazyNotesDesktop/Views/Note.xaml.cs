using EazyNotesDesktop.ViewModels;
using System.Windows.Controls;

namespace EazyNotesDesktop.Views
{
    public partial class Note : UserControl
    {
        public Note()
        {
            InitializeComponent();
        }

        private void ContentTextBox_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            NoteViewModel vm = DataContext as NoteViewModel;
            if (vm == null)
                return;
            vm.VerticalOffset = e.VerticalOffset;
        }

        private void UserControl_IsVisibleChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == true)
            {
                NoteViewModel vm = DataContext as NoteViewModel;
                if (vm == null)
                    return;
                ContentTextBox.ScrollToVerticalOffset(vm.VerticalOffset);
            }
        }
    }
}
