using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EazyNotesDesktop.Util;
using EazyNotesDesktop.ViewModels;
using EazyNotesDesktop.Views.Util;

namespace EazyNotesDesktop.Views
{
    public partial class Main : Window
    {
        private Spinner SpinnerDlg;
        private MainViewModel datacontext;
        private Settings settingsWindow;
        private BackgroundWorker mainBackgroundWorker;

        public Main()
        {
            InitializeComponent();
            datacontext = DataContext as MainViewModel;

            settingsWindow = new Settings(datacontext.SettingsViewModel);

            datacontext.RequestShowAlertBox += (s, e) => new AlertBox(this, s as AlertBoxArgs).ShowDialog();
            datacontext.RequestShowSearchResult += (s, e) => ShowSearchResults(s, null);
            datacontext.RequestCloseSpinner += (s, e) => CloseSpinner(s, null);
            datacontext.RequestShowSpinner += (s, e) => ShowSpinner(s, null);
            datacontext.RequestShowTopicDialog += (s, e) => ShowTopicDialog(s, null);
            datacontext.TopicsChanged += (s, e) => RefreshTopicsList(s, null);

            Closing += (s, e) =>
            {
                PromptUnsavedNotes(e);
                if (e.Cancel)
                    return;
                datacontext.SettingsViewModel.Logout();
                GetShellWindow().Show();
            };
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ShowSpinner("Retrieving and decrypting notes ...", null);
            datacontext.FetchData().Await(() =>
            {
                datacontext.IsBusy = false;
                CloseSpinner(null, null);
                datacontext.SettingsViewModel.RaiseSettingsChanged();
                // For some reason we cannot call RaiseGloballyPinnedNotesChanged directly ...
                Dispatcher.Invoke(() => datacontext.RaiseGloballyPinnedNotesChanged());
            }, e =>
            {
                Dispatcher.Invoke(() =>
                {
                    new AlertBox(this, new AlertBoxArgs(e)).ShowDialog();
                });
            });
        }

        private void ShowSpinner(object sender, RoutedEventArgs e)
        {
            UIUtils.BlurDarkenBackground(this);
            SpinnerDlg = new Spinner(sender.ToString());
            SpinnerDlg.Show();
        }

        private void CloseSpinner(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                var x = Thread.CurrentThread;
                SpinnerDlg?.Hide();
                UIUtils.UndoBlurDarkenBackground(this);
            });
        }

        private Window GetShellWindow()
        {
            // This is the Shell Window
            return Application.Current.Windows[0];
        }

        private async void PromptUnsavedNotes(CancelEventArgs e)
        {
            MainViewModel mainVM = DataContext as MainViewModel;
            var unsavedNotes = mainVM.FilterNoteVMsByUnsaved();
            if (unsavedNotes.Count > 0)
            {
                AlertBoxArgs args = new AlertBoxArgs("There are still unsaved notes. Save all before logging out?", AlertImage.Warning, AlertButton.YesNoCancel);
                AlertBox alertBox = new AlertBox(this, args);
                alertBox.ShowDialog();
                switch (alertBox.Result)
                {
                    case MessageBoxResult.Yes:
                        await datacontext.SaveAllCommand.ExecuteAsync().ConfigureAwait(false);
                        break;
                    case MessageBoxResult.No:
                        break;
                    case MessageBoxResult.Cancel:
                        e.Cancel = true;
                        break;
                }
            }
        }

        private void RefreshTopicsList(object sender, RoutedEventArgs e)
        {
            // TODO: Update collections list with ICollectionView as well by sorting and stuff
            return;
        }

        //private void RefreshNotesList(object sender, RoutedEventArgs e)
        //{
        //    TopicViewModel topicVM = sender as TopicViewModel;
        //    Dispatcher.BeginInvoke(new ThreadStart(delegate ()
        //    {
        //        TopicViewsById.GetValueOrDefault(topicVM.EncryptedPersistedTopic.Id)?.Refresh();
        //    }), null);
        //}

        private void AddTopicView(object sender, RoutedEventArgs e)
        {
            //long newCollectionId = (long)sender;
            //MainViewModel datacontext = DataContext as MainViewModel;
            //CollectionViewModel newCollectionVM = datacontext.CollectionViewModels.Single((cvm) => cvm.EncryptedPersistedCollection.Id == newCollectionId);
            //ICollectionView collectionView = CollectionViewSource.GetDefaultView(newCollectionVM.NoteViewModels);
            //CollectionViewsById.Add(newCollectionId, collectionView);
        }

        private void ShowTopicDialog(object sender, RoutedEventArgs e)
        {
            UIUtils.BlurDarkenBackground(this);
            TopicViewModel topicVM = sender as TopicViewModel;
            TopicDialog dlg = new TopicDialog(topicVM) { Owner = this };
            dlg.ShowDialog();
            UIUtils.UndoBlurDarkenBackground(this);
        }

        private void OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            UIUtils.BlurDarkenBackground(this);
            // settingsWindow.Owner cannot be set in the constructor of this, so 
            settingsWindow.Owner = this;
            settingsWindow.ShowDialog();
            UIUtils.UndoBlurDarkenBackground(this);
        }

        private void ShowSearchResults(object sender, RoutedEventArgs e)
        {
            UIUtils.BlurDarkenBackground(this);
            MainViewModel mainVM = DataContext as MainViewModel;
            SearchResults searchResultsDlg = new SearchResults(mainVM, mainVM.SearchResultsViewModel) { Owner = this };
            searchResultsDlg.ShowDialog();
            UIUtils.UndoBlurDarkenBackground(this);
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = e.Source as ListBox;
            var itemsSource = listBox.ItemsSource;
            TopicViewModel topicVM = listBox.DataContext as TopicViewModel;

            //if (CollectionViewsById == null)
            //    CollectionViewsById = new Dictionary<long, BindingListCollectionView>();
            //if (!CollectionViewsById.ContainsKey(colVM.EncryptedPersistedCollection.Id))
            //{
            //    BindingListCollectionView collectionView = (BindingListCollectionView)CollectionViewSource.GetDefaultView(itemsSource);
            //    //collectionView.SortDescriptions.Clear();
            //    collectionView.SortDescriptions.Add(new SortDescription("Pinned", ListSortDirection.Descending));
            //    collectionView.SortDescriptions.Add(new SortDescription("Title", ListDirection.Ascending));
            //    CollectionViewsById.Add(colVM.EncryptedPersistedCollection.Id, collectionView);
            //    collectionView.Refresh();
            //}
        }

        private void Topic_DragEnter(object sender, DragEventArgs e)
        {

        }

        private void Topic_DragLeave(object sender, DragEventArgs e)
        {

        }

        private async void Topic_Drop(object sender, DragEventArgs e)
        {
            MainViewModel mainVM = DataContext as MainViewModel;
            FrameworkElement wrapper = sender as FrameworkElement;
            TopicViewModel targetTopic = wrapper.DataContext as TopicViewModel;
            if ((e.Data.GetData(typeof(TopicViewModel)) is TopicViewModel sourceTopic))
            {
                int sourceIndex = mainVM.TopicViewModels.IndexOf(sourceTopic);
                int targetIndex = mainVM.TopicViewModels.IndexOf(targetTopic);
                if (sourceIndex == targetIndex)
                    return;
                mainVM.TopicViewModels.RemoveAt(sourceIndex);
                mainVM.TopicViewModels.Insert(targetIndex, sourceTopic);
                //targetTopic.IsBeingDraggedOver = false;
                mainVM.SetTopicPositionPropertiesToTheirIndex();
            }
            object note = e.Data.GetData(typeof(NoteViewModel));
            if (note == null) 
                note = e.Data.GetData(typeof(CheckListNoteViewModel));
            if (note is BaseNoteViewModel sourceNote)
            {
                // Move Note to target topic
                if (sourceNote.TopicVM == targetTopic)
                    return;
                await sourceNote.ChangeTopic(targetTopic.Topic.Id);
                sourceNote.TopicVM = targetTopic;
                mainVM.HandleNoteTopicChanged(sourceNote);
            }
        }

        private void Topic_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            FrameworkElement source = sender as FrameworkElement;
            TopicViewModel topic = source.DataContext as TopicViewModel;
            if (source == null)
                return;
            MainViewModel mainVM = DataContext as MainViewModel;
            if (sender != null && e.LeftButton == MouseButtonState.Pressed)
            {
                mainVM.IsDraggingTopic = true;
                DragDrop.DoDragDrop(source, topic, DragDropEffects.Move);
                mainVM.IsDraggingTopic = false;
                //item.IsBeingDraggedOver = false;
            }
            else
            {
                mainVM.IsDraggingTopic = false;
            }
        }

        private void Note_MouseMove(object sender, MouseEventArgs e)
        {
            MainViewModel mainVM = DataContext as MainViewModel;
            FrameworkElement source = sender as FrameworkElement;
            BaseNoteViewModel noteVM = source.DataContext as BaseNoteViewModel;
            if (sender != null && e.LeftButton == MouseButtonState.Pressed)
            {
                mainVM.IsDraggingTopic = true;
                source.AllowDrop = true;
                DragDrop.DoDragDrop(source, noteVM, DragDropEffects.Move);
                source.AllowDrop = false;
                mainVM.IsDraggingTopic = false;
            }
            else
            {
                mainVM.IsDraggingTopic = false;
            }
        }

        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                SearchBtn.Command.Execute(null);
        }

        private void CreateTopicBtn_MouseMove(object sender, MouseEventArgs e)
        {
            datacontext.HoveredItemToolTip = (sender as FrameworkElement).ToolTip.ToString();
        }

        private void CreateSimpleNoteBtn_MouseMove(object sender, MouseEventArgs e)
        {
            datacontext.HoveredItemToolTip = (sender as FrameworkElement).ToolTip.ToString();
        }

        private void CreateCheckListNoteBtn_MouseMove(object sender, MouseEventArgs e)
        {
            datacontext.HoveredItemToolTip = (sender as FrameworkElement).ToolTip.ToString();
        }

        private void SaveAllBtn_MouseMove(object sender, MouseEventArgs e)
        {
            datacontext.HoveredItemToolTip = (sender as FrameworkElement).ToolTip.ToString();
        }

        private void ToggleTopicSymbolAndText_MouseMove(object sender, MouseEventArgs e)
        {
            datacontext.HoveredItemToolTip = (sender as FrameworkElement).ToolTip.ToString();
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            datacontext.HoveredItemToolTip = "";
        }

        private void NotesListBox_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (datacontext.SelectedTopicVM != null)
                datacontext.SelectedTopicVM.NotesListScrollOffset = e.VerticalOffset;
        }

        private void BtnGloballyPinnedNote_Click(object sender, RoutedEventArgs e)
        {
            Button btnSender = sender as Button;
            BaseNoteViewModel noteVM = btnSender.DataContext as BaseNoteViewModel;
            datacontext.HandleSelectedNoteChanged(noteVM);
        }
    }
}
