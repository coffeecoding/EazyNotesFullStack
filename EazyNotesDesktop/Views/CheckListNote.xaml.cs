using EazyNotesDesktop.ViewModels;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace EazyNotesDesktop.Views
{
    public partial class CheckListNote : UserControl
    {
        public CheckListNote()
        {
            InitializeComponent();
        }

        private void Item_MouseMove(object sender, MouseEventArgs e)
        {
            Debug.WriteLine($"MouseMove in {sender}, LeftButton is {e.LeftButton}");
            FrameworkElement source = sender as FrameworkElement;
            CheckListItemViewModel item = source.DataContext as CheckListItemViewModel;
            if (source == null)
                return;
            if (sender != null && e.LeftButton == MouseButtonState.Pressed)
            {
                item.CheckListNoteVM.IsDraggingItem = true;
                DragDrop.DoDragDrop(source, item, DragDropEffects.Move);
                item.CheckListNoteVM.IsDraggingItem = false;
                item.IsBeingDraggedOverTop = false;
                item.IsBeingDraggedOverBottom = false;
            }
            else
            {
                item.CheckListNoteVM.IsDraggingItem = false;
            }
        }

        private void DragEnter_TopHalf(object sender, DragEventArgs e)
        {
            FrameworkElement wrapper = sender as FrameworkElement;
            if (!(wrapper.DataContext is CheckListItemViewModel targetItem))
                return;
            if (!(e.Data.GetData(typeof(CheckListItemViewModel)) is CheckListItemViewModel sourceItem))
                return;
            int sourceIndex = sourceItem.CheckListNoteVM.CheckListItemVMs.IndexOf(sourceItem);
            int targetIndex = targetItem.CheckListNoteVM.CheckListItemVMs.IndexOf(targetItem);
            if (sourceIndex == targetIndex)
                return;
            targetItem.IsBeingDraggedOverTop = true;
        }

        private void DragEnter_BottomHalf(object sender, DragEventArgs e)
        {
            FrameworkElement wrapper = sender as FrameworkElement;
            if (!(wrapper.DataContext is CheckListItemViewModel targetItem))
                return;
            if (!(e.Data.GetData(typeof(CheckListItemViewModel)) is CheckListItemViewModel sourceItem))
                return;
            int sourceIndex = sourceItem.CheckListNoteVM.CheckListItemVMs.IndexOf(sourceItem);
            int targetIndex = targetItem.CheckListNoteVM.CheckListItemVMs.IndexOf(targetItem);
            if (sourceIndex == targetIndex)
                return;
            targetItem.IsBeingDraggedOverBottom = true;
        }

        private void DropItem_TopHalf(object sender, DragEventArgs e)
        {
            FrameworkElement wrapper = sender as FrameworkElement;
            CheckListItemViewModel targetItem = wrapper.DataContext as CheckListItemViewModel;
            if (!(e.Data.GetData(typeof(CheckListItemViewModel)) is CheckListItemViewModel sourceItem))
                return;
            int sourceIndex = sourceItem.CheckListNoteVM.CheckListItemVMs.IndexOf(sourceItem);
            int targetIndex = targetItem.CheckListNoteVM.CheckListItemVMs.IndexOf(targetItem);
            targetItem.IsBeingDraggedOverTop = false;
            if (sourceIndex == targetIndex || sourceIndex == targetIndex - 1)
                return;
            else if (sourceIndex < targetIndex)
                targetItem.CheckListNoteVM.CheckListItemVMs.Move(sourceIndex, targetIndex-1);
            targetItem.CheckListNoteVM.CheckListItemVMs.Move(sourceIndex, targetIndex);
            targetItem.CheckListNoteVM.State = EntityState.Modified;
        }

        private void DropItem_BottomHalf(object sender, DragEventArgs e)
        {
            FrameworkElement wrapper = sender as FrameworkElement;
            CheckListItemViewModel targetItem = wrapper.DataContext as CheckListItemViewModel;
            if (!(e.Data.GetData(typeof(CheckListItemViewModel)) is CheckListItemViewModel sourceItem))
                return;
            int sourceIndex = sourceItem.CheckListNoteVM.CheckListItemVMs.IndexOf(sourceItem);
            int targetIndex = targetItem.CheckListNoteVM.CheckListItemVMs.IndexOf(targetItem);
            targetItem.IsBeingDraggedOverBottom = false;
            if (sourceIndex == targetIndex || sourceIndex == targetIndex + 1)
                return;
            else if (sourceIndex > targetIndex)
                targetItem.CheckListNoteVM.CheckListItemVMs.Move(sourceIndex, targetIndex + 1);
            targetItem.CheckListNoteVM.CheckListItemVMs.Move(sourceIndex, targetIndex);
            targetItem.CheckListNoteVM.State = EntityState.Modified;
        }

        private void DragLeave_TopHalf(object sender, DragEventArgs e)
        {
            FrameworkElement wrapper = sender as FrameworkElement;
            CheckListItemViewModel targetItem = wrapper.DataContext as CheckListItemViewModel;
            if (targetItem == null)
                return;
            targetItem.IsBeingDraggedOverTop = false;
        }

        private void DragLeave_BottomHalf(object sender, DragEventArgs e)
        {
            FrameworkElement wrapper = sender as FrameworkElement;
            CheckListItemViewModel targetItem = wrapper.DataContext as CheckListItemViewModel;
            if (targetItem == null)
                return;
            targetItem.IsBeingDraggedOverBottom = false;
        }

        private void BtnAddItem_Click(object sender, RoutedEventArgs e)
        {
            CheckListNoteViewModel vm = DataContext as CheckListNoteViewModel;
            vm.AddCheckListItemCommand.Execute();
            MainScrollViewer.ScrollToBottom();
        }

        private void TextTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            CheckListNoteViewModel context = DataContext as CheckListNoteViewModel;
            if (context.State != EntityState.Default) 
                (sender as TextBox).Focus();
        }

        private void TextTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (e.Key == Key.Enter)
                BtnAddItem_Click(sender, e);
            if (tb.CaretIndex == 0)
            {
                CheckListItemViewModel vm = tb.DataContext as CheckListItemViewModel;
                if (e.Key == Key.Tab)
                {
                    vm.IndentCount++;
                    // setting handled to true prevents the textbox from losing focus due to the tab press
                    e.Handled = true;
                }
                else if (e.Key == Key.Back && vm.IndentCount > 0)
                    vm.IndentCount--;
            }
        }

        private void TextTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (e.Key == Key.Up || e.Key == Key.Down)
            {
                FocusNeighbouringCheckListItemTextBox(sender, e.Key, false);
            }
            if (tb.CaretIndex == 0)
            {
                CheckListItemViewModel vm = tb.DataContext as CheckListItemViewModel;
                if (e.Key == Key.Back) {
                    if (vm.IndentCount > 0)
                        vm.IndentCount--;
                    else if (vm.IndentCount == 0 && string.IsNullOrWhiteSpace(vm.Text))
                    {
                        FocusNeighbouringCheckListItemTextBox(sender, Key.Up, true);
                        vm.RemoveCommand.Execute();
                    }
                }
            }
        }

        private void FocusNeighbouringCheckListItemTextBox(object sender, Key key, bool setCaretIndexToEnd)
        {
            TextBox tb = sender as TextBox;
            CheckListNoteViewModel note = DataContext as CheckListNoteViewModel;
            FrameworkElement button = tb.Parent as FrameworkElement;
            while (button.Parent != null)
                button = button.Parent as FrameworkElement;
            // button parent is now null, lets search for visual parent 2 levels up
            ContentPresenter buttonParent = VisualTreeHelper.GetParent(button) as ContentPresenter;
            UIElement stackPanel = VisualTreeHelper.GetParent(buttonParent) as UIElement;
            while (!(stackPanel is StackPanel))
                stackPanel = VisualTreeHelper.GetParent(stackPanel) as UIElement;
            int currentIndex = (stackPanel as StackPanel).Children.IndexOf(buttonParent);
            // i now holds the correct index of the source checklistitem in the rows of checklist items
            // now we need to focus and set caret index of textbox at i-1 / i+1 depending on key
            int newIndex = 0;
            if (key == Key.Up && currentIndex > 0)
                newIndex = currentIndex - 1;
            else if (key == Key.Down && currentIndex < note.CheckListItemVMs.Count - 1)
                newIndex = currentIndex + 1;
            if (currentIndex != newIndex)
            {
                ContentPresenter upperNeighbour = (stackPanel as StackPanel).Children[newIndex] as ContentPresenter;
                TextBox upperNeighbourTextBox = GetChildOfType<TextBox>(upperNeighbour);
                upperNeighbourTextBox.Focus();
                if (setCaretIndexToEnd)
                    upperNeighbourTextBox.CaretIndex = upperNeighbourTextBox.Text.Length;
                else 
                    upperNeighbourTextBox.CaretIndex = tb.CaretIndex;
            }
        }

        // https://stackoverflow.com/a/10279201/12213872
        private T GetChildOfType<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                var result = (child as T) ?? GetChildOfType<T>(child);
                if (result != null) return result;
            }
            return null;
        }
    }
}
