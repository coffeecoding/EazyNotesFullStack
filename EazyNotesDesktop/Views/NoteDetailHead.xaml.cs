using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;

namespace EazyNotesDesktop.Views
{
    public partial class NoteDetailHead : UserControl
    {
        public NoteDetailHead()
        {
            InitializeComponent();
        }

        private void UserControl_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            TitleTextBox.Focus();
            TitleTextBox.CaretIndex = TitleTextBox.Text.Length;
        }

        private void TitleTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                var checklistNoteView = (DataContext as CheckListNote);
                if (checklistNoteView != null)
                {
                    ButtonAutomationPeer peer = new ButtonAutomationPeer(checklistNoteView.BtnAddItem);
                    IInvokeProvider invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
                    invokeProv.Invoke();
                } else
                {
                    var note = DataContext as Note;
                    if (note != null)
                        note.ContentTextBox.Focus();
                }
            }
        }
    }
}
