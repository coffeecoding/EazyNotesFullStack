using EazyNotesDesktop.Util;
using EazyNotesDesktop.ViewModels;
using EazyNotesDesktop.Views.Util;
using System.Windows;
using System.Windows.Controls;

namespace EazyNotesDesktop.Views
{
    public partial class TopicDialog : Window
    {
        private Spinner SpinnerDlg;

        public TopicDialog(TopicViewModel topicVM)
        {
            DataContext = topicVM;
            InitializeComponent();
            topicVM.DiscardedChanges += (s, e) => Cancel(s, null);
            topicVM.RequestCloseTopicDialog += (s, e) => Cancel(s, null);
            TitleTextBox.Focus();
            TitleTextBox.CaretIndex = TitleTextBox.Text.Length;

            topicVM.RequestShowSpinner += (s, e) =>
            {
                UIUtils.BlurDarkenBackground(this);
                SpinnerDlg = new Spinner(s.ToString());
                SpinnerDlg.Show();
            };
            topicVM.RequestCloseSpinner += (s, e) =>
            {
                SpinnerDlg?.Hide();
                UIUtils.UndoBlurDarkenBackground(this);
            };
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void Trash_Click(object sender, RoutedEventArgs e)
        {
            TopicViewModel topicVM = (DataContext as TopicViewModel);
            AlertBoxArgs args = new AlertBoxArgs("Confirm to trash this Topic along with all its Notes?", AlertImage.Warning, AlertButton.YesNo);
            AlertBox alertBox = new AlertBox(this, args);
            if (topicVM.NoteViewModels.Count > 0)
            {
                alertBox.ShowDialog();
                if (alertBox.Result == MessageBoxResult.No)
                    return;
            }
            await topicVM.TrashUntrashCommand.ExecuteAsync();
        }
        
        // Mouse wheel scrolling for some reason didn't work, hence manual implementation
        //https://stackoverflow.com/a/16235785/12213872
        private void ColorsScrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToHorizontalOffset(scv.HorizontalOffset - e.Delta);
            e.Handled = true;
        }
    }
}
