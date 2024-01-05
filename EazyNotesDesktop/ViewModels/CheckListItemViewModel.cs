using EazyNotes.Models.POCO;
using Prism.Commands;
using Prism.Mvvm;
using System;

namespace EazyNotesDesktop.ViewModels
{
    public class CheckListItemViewModel : BindableBase
    {
        public CheckListItemViewModel(CheckListItem todoItem, CheckListNoteViewModel checkListNoteVM)
        {
            CheckListItem = todoItem;
            Text = todoItem.Text;
            IsChecked = todoItem.IsChecked;
            IndentCount = todoItem.IndentCount;
            CheckListNoteVM = checkListNoteVM;

            RemoveCommand = new DelegateCommand(Remove, () => true);
            ToggleCheckCommand = new DelegateCommand(ToggleCheck, () => true);
        }

        public CheckListItem CheckListItem { get; set; }
        public CheckListNoteViewModel CheckListNoteVM { get; set; }

        public DelegateCommand RemoveCommand { get; set; }
        public DelegateCommand ToggleCheckCommand { get; set; }

        private bool _isChecked;
        public bool IsChecked
        {
            get { return _isChecked; }
            set { SetProperty(ref _isChecked, value); }
        }

        private bool _isBeingDraggedOver;
        public bool IsBeingDraggedOver
        {
            get { return _isBeingDraggedOver; }
            set { SetProperty(ref _isBeingDraggedOver, value); }
        }

        private bool _isBeingDraggedOverTop;
        public bool IsBeingDraggedOverTop
        {
            get { return _isBeingDraggedOverTop; }
            set { SetProperty(ref _isBeingDraggedOverTop, value); }
        }

        private bool _isBeingDraggedOverBottom;
        public bool IsBeingDraggedOverBottom
        {
            get { return _isBeingDraggedOverBottom; }
            set { SetProperty(ref _isBeingDraggedOverBottom, value); }
        }

        private int _indentCount;
        public int IndentCount
        {
            get { return _indentCount; }
            set { SetProperty(ref _indentCount, value); }
        }

        private string _text;
        public string Text
        {
            get { return _text; }
            set 
            { 
                SetProperty(ref _text, value);
                UpdateNoteState();
            }
        }

        public void ApplyChanges()
        {
            CheckListItem.Text = Text;
            CheckListItem.IsChecked = IsChecked;
        }

        public void DiscardChanges()
        {
            Text = CheckListItem.Text;
            IsChecked = CheckListItem.IsChecked;
        }

        private void Remove()
        {
            CheckListNoteVM.CheckListItemVMs.Remove(this);
            if (CheckListNoteVM.State == EntityState.Default)
            {
                CheckListNoteVM.State = EntityState.Modified;
            }
        }

        private void ToggleCheck()
        {
            IsChecked = !IsChecked;
            UpdateNoteState();
        }

        private void UpdateNoteState()
        {
            if (CheckListNoteVM?.State == EntityState.Default)
            {
                CheckListNoteVM.State = EntityState.Modified;
            }
        }

        public override string ToString()
        {
            return CheckListItem.ToString();
        }
    }
}
