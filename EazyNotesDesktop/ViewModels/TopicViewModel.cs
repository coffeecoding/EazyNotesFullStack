using EazyNotesDesktop.Library.Common;
using EazyNotesDesktop.Library.DAO;
using EazyNotes.Models.POCO;
using EazyNotesDesktop.Util;
using MvvmHelpers.Commands;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using EazyNotes.Common;

namespace EazyNotesDesktop.ViewModels
{
    public class TopicViewModel : BindableBase, IEntityViewModel
    {
        private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();

        protected readonly DataAccess _dataAccess;
        private readonly MainViewModel _mainVM;
        private Guid _id;

        public TopicViewModel(DataAccess dataAccess, MainViewModel mainVM, Guid id)
        {
            _dataAccess = dataAccess;
            _mainVM = mainVM;
            _id = id;

            UpdateStateFromRepo();
            InitDefaults();
        }

        public TopicViewModel(DataAccess dataAccess, MainViewModel mainVM, Topic topic)
        {
            _dataAccess = dataAccess;
            _mainVM = mainVM;
            _id = topic.Id;

            UpdateStateWithObject(topic);
            InitDefaults();
        }

        private void InitDefaults()
        {
            NoteViewModels = new BindingList<BaseNoteViewModel>();

            TrashUntrashCommand = new AsyncCommand(TrashUntrash, (x) => CanTrash);
            SaveCommand = new AsyncCommand(Save, (x) => CanSave);
            DiscardCommand = new DelegateCommand(Discard, () => true);
            DeleteCommand = new AsyncCommand(Delete, (x) => true);
            EditCommand = new DelegateCommand(Edit, () => true);
        }

        public event EventHandler DiscardedChanges;
        public event EventHandler RequestCloseTopicDialog;
        public event EventHandler RequestShowSpinner;
        public event EventHandler RequestCloseSpinner;

        public AsyncCommand TrashUntrashCommand { get; set; }
        public AsyncCommand SaveCommand { get; set; }
        public AsyncCommand DeleteCommand { get; set; }
        public DelegateCommand DiscardCommand { get; set; }
        public DelegateCommand EditCommand { get; set; }

        public string PersistedTitle { get => Topic.Title; }
        public string PersistedSymbol { get => Topic.Symbol; }
        public string PersistedColor { get => Topic.Color; }
        public DateTime? PersistedDateDeleted { get => Topic.DateDeleted; }
        private Topic _topic;
        public Topic Topic 
        { 
            get {
                if (_dataAccess.Topics.TryGetValue(_id, out Topic val))
                    return val;
                _topic = _mainVM.CreateNewTopic();
                return _topic;
            }
        }

        public void RaiseTopicChangedEvents()
        {
            _mainVM.RaiseTopicsChanged();
            RaisePropertyChanged("PersistedTitle");
            RaisePropertyChanged("PersistedSymbol");
            RaisePropertyChanged("PersistedDateDeleted");
            RaisePropertyChanged("PersistedColor");
            // Position may have changed, so sort Topics
            _mainVM.HandleTopicPositionChanged();
        }

        // This is strictly necessary for generic binding in a list of IEntityViewModels
        public string Title => Topic.Title;
        // This is strictly necessary for generic binding in a list of ÌEntityViewModels
        public string Symbol => Topic.Symbol;
        // This is strictly necessary for generic binding in a list of ÌEntityViewModels
        public DateTime? DateDeleted => Topic.DateDeleted;

        private EntityState _state;
        public EntityState State
        {
            get { return _state; }
            set
            {
                SetProperty(ref _state, value);
                switch (value)
                {
                    case EntityState.Default:
                        CanSave = false;
                        CanTrash = true;
                        break;
                    case EntityState.Modified:
                        CanSave = true;
                        CanTrash = true;
                        break;
                    case EntityState.New:
                        CanSave = true;
                        CanTrash = false;
                        break;
                }
                SaveCommand?.RaiseCanExecuteChanged();
                TrashUntrashCommand?.RaiseCanExecuteChanged();
            }
        }

        private string _resultMessage;
        public string ResultMessage
        {
            get { return _resultMessage; }
            set { SetProperty(ref _resultMessage, value); }
        }

        private string _titleField;
        public string TitleField
        {
            get { return _titleField; }
            set 
            { 
                SetProperty(ref _titleField, value);
                if (State == EntityState.Default)
                {
                    State = EntityState.Modified;
                }
            }
        }

        private string _selectedSymbol;
        public string SelectedSymbol
        {
            get { return _selectedSymbol; }
            set 
            { 
                SetProperty(ref _selectedSymbol, value);
                if (State == EntityState.Default)
                {
                    State = EntityState.Modified;
                }
            }
        }

        private int _selectedPosition;
        public int SelectedPosition
        {
            get { return _selectedPosition; }
            set { SetProperty(ref _selectedPosition, value); }
        }

        public List<string> Symbols => EazyNotes.Common.Symbols.GetList;

        private string _selectedColor;
        public string SelectedColor
        {
            get { return _selectedColor; }
            set 
            { 
                SetProperty(ref _selectedColor, value);
                if (State == EntityState.Default)
                {
                    State = EntityState.Modified;
                }
            }
        }

        public List<string> Colors => ColorRef.Colors;

        private BindingList<BaseNoteViewModel> _noteViewModels;
        public BindingList<BaseNoteViewModel> NoteViewModels
        {
            get { return _noteViewModels; }
            set { SetProperty(ref _noteViewModels, value); }
        }

        public void UpdateStateWithObject(Topic topic)
        {
            TitleField = topic.Title;
            SelectedSymbol = topic.Symbol;
            SelectedColor = topic.Color;
            SelectedPosition = topic.Position;
            if (Topic.IsPersistedLocally())
                State = EntityState.Default;
            else State = EntityState.New;
            RaiseTopicChangedEvents();
        }

        public void UpdateStateFromRepo()
        {
            TitleField = Topic.Title;
            SelectedSymbol = Topic.Symbol;
            SelectedColor = Topic.Color;
            SelectedPosition = Topic.Position;
            if (Topic.IsPersistedLocally())
                State = EntityState.Default;
            else State = EntityState.New;
            RaiseTopicChangedEvents();
        }

        private Topic CreateChangedTopic()
        {
            Topic changedTopic = (Topic)Topic.Clone();
            changedTopic.Title = TitleField;
            changedTopic.Symbol = SelectedSymbol;
            changedTopic.Color = SelectedColor ?? ColorRef.DefaultTopicColor;
            return changedTopic;
        }

        public void AddNoteViewModel(BaseNoteViewModel noteViewModel)
        {
            NoteViewModels.Add(noteViewModel);
            SortNotes(_mainVM.SettingsViewModel.SelectedNoteSortKey);
        }

        public void AddNoteViewModels(List<BaseNoteViewModel> noteViewModels)
        {
            NoteViewModels.AddRange(noteViewModels);
            SortNotes(_mainVM.SettingsViewModel.SelectedNoteSortKey);
        }

        public void RemoveNoteViewModel(BaseNoteViewModel noteViewModel)
        {
            NoteViewModels.Remove(noteViewModel);
        }

        private void Edit()
        {
            _mainVM.RaiseRequestShowTopicDialog(this);
        }

        public void SortNotes(NoteSortKey noteSortKey = null)
        {
            if (noteSortKey == null)
                noteSortKey = _mainVM.SettingsViewModel.SelectedNoteSortKey;

            // memorize the selected note, as it will get lost when sorting
            BaseNoteViewModel selected = _mainVM.SelectedNoteVM;

            IOrderedEnumerable<BaseNoteViewModel> ordered = null;
            switch (noteSortKey.SortKey)
            {
                case SortKey.Title:
                    ordered = NoteViewModels.OrderBy(noteVM => noteVM.Title);
                    break;
                case SortKey.TitleDescending:
                    ordered = NoteViewModels.OrderByDescending(noteVM => noteVM.Title);
                    break;
                case SortKey.LastEdited:
                    ordered = NoteViewModels.OrderBy(noteVM => noteVM.Note.DateModified);
                    break;
                case SortKey.LastEditedDescending:
                    ordered = NoteViewModels.OrderByDescending(noteVM => noteVM.Note.DateModified);
                    break;
                case SortKey.Created:
                    ordered = NoteViewModels.OrderBy(noteVM => noteVM.Note.DateCreated);
                    break;
                case SortKey.CreatedDescending:
                    ordered = NoteViewModels.OrderByDescending(noteVM => noteVM.Note.DateCreated);
                    break;
                default: throw new NotSupportedException("Sorting key not supported");
            }
            // finally, order by pinned
            NoteViewModels = new BindingList<BaseNoteViewModel>(ordered
                .OrderByDescending(noteVM => noteVM.Pinned).ToList());

            // reselect the memorized, previously selected note
            _mainVM.SelectedNoteVM = selected;
        }

        private async Task Save()
        {
            Log.Info($"Saving {State} topic");
            Topic changedTopic = CreateChangedTopic();
            if (Topic.Equals(changedTopic) && State == EntityState.Modified)
            {
                return;
            }
            try
            {
                DateTime utcNow = DateTime.UtcNow;
                changedTopic.DateModified = utcNow;
                DataUpdateAction dua = DetermineUpdateAction(changedTopic);
                if (dua == DataUpdateAction.Insert)
                    changedTopic.DateCreated = changedTopic.DateModifiedHeader = changedTopic.DateModified = utcNow;
                else if (dua == DataUpdateAction.UpdateBody)
                    changedTopic.DateModifiedHeader = changedTopic.DateModified = utcNow;
                else changedTopic.DateModifiedHeader = utcNow;
                Topic updatedTopic = await _dataAccess.InsertOrUpdateTopic(changedTopic, dua);
                _id = updatedTopic.Id;
                ResultMessage = "Saved successfully!";

                if (State == EntityState.New)
                    _mainVM.HandleTopicCreated(this);
                State = EntityState.Default;

                UpdateStateFromRepo();
                // Raise topics changed event for example to re-sort them
                // TODO: Again, review this control flow
                //_mainVM.RaiseTopicsChanged();
                RequestCloseTopicDialog?.Invoke(this, null);

                // THE FOLLOWING IS ALREADY DONE WHEN SETTING PersistedTopic = updatedTopic INSIDE THE SETTER
                //RaisePropertyChanged("PersistedSymbol");
                //RaisePropertyChanged("PersistedTitle");
                //RaisePropertyChanged("PersistedDateDeleted");
                //RaisePropertyChanged("PersistedColor");
            }
            // TODO: more granular catching, appropriate user feedback
            catch (Exception e)
            {
                ResultMessage = "An unexpected error occurred.";
                Log.Error(e);
            }
        }

        private void Discard()
        {
            if (State == EntityState.Modified)
            {
                TitleField = Topic.Title;
                SelectedSymbol = Topic.Symbol;
                State = EntityState.Default;
            }
            else if (State == EntityState.New)
            {
                _mainVM.TopicViewModels.Remove(this);
            }
            // TODO: Review if this event raise is needed or should be refactored
            DiscardedChanges(this, null);
        }

        private async Task TrashUntrash()
        {
            Log.Info($"Trashing {State} topic");
            RequestShowSpinner?.Invoke("Trashing items ...", null);
            try
            {
                if (Topic.DateDeleted != null)
                {
                    var tasks = NoteViewModels.Where(noteVM => noteVM.DateDeleted != null)
                        .ToList().Select(note => note.TrashUntrashCommand.ExecuteAsync());
                    // 2. execute all simultaneously
                    await Task.WhenAll(tasks);

                    bool success = await _dataAccess.TrashUntrasEntity(Topic, null);
                    // On Success:
                    _mainVM.HandleTopicUntrashed(this);
                }
                else if (Topic.DateDeleted == null)
                {
                    var tasks = NoteViewModels.ToList()
                        .Select(note => note.TrashUntrashCommand.ExecuteAsync());
                    await Task.WhenAll(tasks);

                    DateTime? now = DateTime.UtcNow;
                    bool success = await _dataAccess.TrashUntrasEntity(Topic, now);
                    // On Success:
                    _mainVM.HandleTopicTrashed(this);
                    RequestCloseTopicDialog(this, null);
                }
                State = EntityState.Default;
            }
            // TODO: proper user feedback and catch more granularly 
            catch (Exception e)
            {
                ResultMessage = "An unexpected error occurred.";
                Log.Error(e);
            }
            finally
            {
                RequestCloseSpinner?.Invoke(null, null);
            }
        }

        private async Task Delete()
        {
            Log.Info($"Deleting {State} topic");
            try
            {
                // First delete all encompassed Notes
                // 1. project corresponding notes from trash bin into tasks of trash commands
                var tasks = NoteViewModels.ToList().Select(note => note.DeleteCommand.ExecuteAsync());
                // 2. execute all simultaneously
                await Task.WhenAll(tasks);

                Topic deletedTopic = await _dataAccess.DeleteTopic(Topic);

                // On success
                _mainVM.HandleEntityDeleted(this);
            }
            // TODO: catch more granular errors and give appropriate feedback
            catch (Exception e)
            {
                string error = $"{e.GetType()}: {e.Message}";
                Log.Error(e);
            }
        }

        private DataUpdateAction DetermineUpdateAction(Topic changedTopic)
        {
            if (!changedTopic.IsPersistedLocally())
                return DataUpdateAction.Insert;
            else if (Topic.BodyEquals(changedTopic))
                return DataUpdateAction.UpdateHeader;
            return DataUpdateAction.UpdateBody;
        }

        private bool _canSave;
        public bool CanSave
        {
            get { return _canSave; }
            set { SetProperty(ref _canSave, value); }
        }

        private bool _canTrash;
        public bool CanTrash
        {
            get { return _canTrash; }
            set { SetProperty(ref _canTrash, value); }
        }

        private double _notesListScrollOffset;
        public double NotesListScrollOffset
        {
            get { return _notesListScrollOffset; }
            set { SetProperty(ref _notesListScrollOffset, value); }
        }
    }
}
