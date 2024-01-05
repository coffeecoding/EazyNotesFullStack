using EazyNotes.Models.POCO;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using EazyNotesDesktop.Library.DAO;

namespace EazyNotesDesktop.ViewModels
{
    public class CheckListNoteViewModel : BaseNoteViewModel
    {
        public CheckListNoteViewModel(DataAccess dataAccess, MainViewModel mainVM, TopicViewModel topicVM, Guid id)
            : base(dataAccess, mainVM, topicVM, id)
        {
            UpdateStateFromRepo();
            InitDefaults();
        }

        public CheckListNoteViewModel(DataAccess dataAccess, MainViewModel mainVM, TopicViewModel topicVM, CheckListNote note)
            : base(dataAccess, mainVM, topicVM, note.Id)
        {
            UpdateStateWithObject(note);
            InitDefaults();
        }

        private void InitDefaults()
        {
            AddCheckListItemCommand = new DelegateCommand(AddCheckListItem, () => true);
        }

        public DelegateCommand AddCheckListItemCommand { get; set; }

        private ObservableCollection<CheckListItemViewModel> _checkListItemVMs;
        public ObservableCollection<CheckListItemViewModel> CheckListItemVMs
        {
            get { return _checkListItemVMs; }
            set { SetProperty(ref _checkListItemVMs, value); }
        }

        private string _selectedCheckListItem;
        public string SelectedCheckListItem
        {
            get { return _selectedCheckListItem; }
            set { SetProperty(ref _selectedCheckListItem, value); }
        }

        private bool _isDraggingItem;
        public bool IsDraggingItem
        {
            get { return _isDraggingItem; }
            set { SetProperty(ref _isDraggingItem, value); }
        }

        private ObservableCollection<CheckListItemViewModel> ConvertCheckListItemsToVMs(IList<CheckListItem> checkListItems)
        {
            return new ObservableCollection<CheckListItemViewModel>(checkListItems.ToList()
                .ConvertAll((item) => new CheckListItemViewModel(item, this)));
        }

        protected override AbstractNote CreateChangedNote()
        {
            List<CheckListItem> changedCheckListItems = CheckListItemVMs.ToList()
                .ConvertAll((i) => new CheckListItem(i.IsChecked, i.Text, i.IndentCount));
            CheckListNote changedNote = (CheckListNote)Note.Clone();
            changedNote.Title = Title;
            changedNote.CheckListItems = changedCheckListItems;
            return changedNote;
        }

        public override void UpdateStateWithObject(AbstractNote note)
        {
            CheckListItemVMs = ConvertCheckListItemsToVMs((note as CheckListNote).CheckListItems);
            base.UpdateStateWithObject(note);
        }

        public override void UpdateStateFromRepo()
        {
            if (CheckListItemVMs == null)
                CheckListItemVMs = new ObservableCollection<CheckListItemViewModel>();
            CheckListItemVMs.Clear();
            CheckListItemVMs.AddRange(ConvertCheckListItemsToVMs((Note as CheckListNote).CheckListItems));
            base.UpdateStateFromRepo();
        }

        public override void Discard()
        {
            if (State == EntityState.Modified)
            {
                Title = Note.Title;
                CheckListItemVMs = ConvertCheckListItemsToVMs(
                    ((CheckListNote)Note).CheckListItems);
                State = EntityState.Default;
            }
            else if (State == EntityState.New)
            {
                base.Discard();
            }
        }

        public override string PreviewText
        {
            get
            {
                string content = "";
                CheckListItemVMs.ToList().ForEach((item) => content += $"\u00b7 {item.Text} ");
                bool contentLongerThanCutoff = content.Length > 50;
                return contentLongerThanCutoff
                    ? content.Substring(0, 50) + "..."
                    : content;
            }
        }

        public override bool ContainsSearchKey(string uppercaseKey)
        {
            return (Title?.ToUpper().Contains(uppercaseKey)) == true
                || CheckListItemVMs.Any((item) => (item.Text?.ToUpper().Contains(uppercaseKey)) == true);
        }

        public void AddCheckListItems(IEnumerable<CheckListItemViewModel> checkListItemVMs)
        {
            CheckListItemVMs.AddRange(checkListItemVMs);
            RaisePropertyChanged(PreviewText);
        }

        private void AddCheckListItem()
        {
            CheckListItem checkListItem = new CheckListItem();
            CheckListItemViewModel last = CheckListItemVMs.LastOrDefault();
            if (last != null)
                checkListItem.IndentCount = last.IndentCount;
            CheckListItemViewModel checkListItemVM = new CheckListItemViewModel(checkListItem, this);
            CheckListItemVMs.Add(checkListItemVM);
            RaisePropertyChanged(PreviewText);
            if (State == EntityState.Default)
            {
                State = EntityState.Modified;
            }
        }
    }
}
