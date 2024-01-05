using EazyNotes.Models.DTO;
using MvvmHelpers.Commands;
using Prism.Commands;
using System;
using System.Threading.Tasks;
using EazyNotesDesktop.Library.DAO;
using EazyNotes.Models.POCO;
using Prism.Mvvm;
using EazyNotes.Common;

namespace EazyNotesDesktop.ViewModels
{
    public class BaseNoteViewModel : BindableBase, IEntityViewModel
    {
        private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();

        protected readonly DataAccess _dataAccess;
        protected readonly MainViewModel _mainVM;
        protected Guid _id;

        public BaseNoteViewModel(DataAccess dataAccess, MainViewModel mainVM, TopicViewModel topicVM, Guid id)
        {
            _dataAccess = dataAccess;
            _mainVM = mainVM;
            _id = id;

            TopicVM = topicVM;
            // UpdateFromRepo is done in derived notes, which call the UpdateFromRepo method from this base class

            TrashUntrashCommand = new AsyncCommand(TrashUntrash, (x) => CanDeleteOrRestore);
            SaveCommand = new AsyncCommand(Save, (x) => CanDiscardOrSave);
            DiscardCommand = new DelegateCommand(Discard, () => CanDiscardOrSave);
            DeleteCommand = new AsyncCommand(Delete, (x) => true);
            TogglePinnedCommand = new AsyncCommand(TogglePinned, (x) => true);
            ToggleGloballyPinnedCommand = new AsyncCommand(ToggleGloballyPinned, (x) => true);
        }

        public static BaseNoteViewModel FromNote(AbstractNote note, DataAccess da, MainViewModel mainVM, TopicViewModel topicVM)
        {
            if (note.IsPersistedLocally())
            {
                if (note is SimpleNote note1)
                    return new NoteViewModel(da, mainVM, topicVM, note1.Id);
                else if (note is CheckListNote note2)
                    return new CheckListNoteViewModel(da, mainVM, topicVM, note2.Id);
                else throw new NotSupportedException();
            }
            else
            {
                if (note is SimpleNote note1)
                    return new NoteViewModel(da, mainVM, topicVM, note1);
                else if (note is CheckListNote note2)
                    return new CheckListNoteViewModel(da, mainVM, topicVM, note2);
                else throw new NotSupportedException();
            }
        }

        public event EventHandler NotesChanged;

        public AsyncCommand TrashUntrashCommand { get; set; }
        public AsyncCommand SaveCommand { get; set; }
        public DelegateCommand DiscardCommand { get; set; }
        public AsyncCommand ShareCommand { get; set; }
        public AsyncCommand DeleteCommand { get; set; }
        public AsyncCommand TogglePinnedCommand { get; set; }
        public AsyncCommand ToggleGloballyPinnedCommand { get; set; }

        public TopicViewModel TopicVM { get; set; }

        public string PersistedTitle { get => Note.Title; }
        public string PersistedSymbol { get => Note.Symbol; }
        public string PersistedColor { get => ColorRef.DefaultTopicColor; }
        public DateTime? DateDeleted { get => Note.DateDeleted; }
        public DateTime DateLastEdited { get => Note.DateModified; }
        protected AbstractNote _newNote;
        public AbstractNote Note
        {
            get
            {
                if (_dataAccess.Notes.TryGetValue(_id, out AbstractNote val))
                    return val;
                return _newNote;
            }
        }

        public virtual void RaiseNoteChangedEvents()
        {
            RaisePropertyChanged("DateDeleted");
            RaisePropertyChanged("DateLastEdited");
            RaisePropertyChanged("PersistedTitle");
            RaisePropertyChanged("PersistedSymbol");
            RaisePropertyChanged("PersistedDateDeleted");
            TopicVM.SortNotes(_mainVM.SettingsViewModel.SelectedNoteSortKey);
        }

        protected virtual AbstractNote CreateChangedNote() => null;
        protected virtual void UpdateLocalNote(NoteDTO encryptedUpdatedNote) { }
        public virtual bool ContainsSearchKey(string uppercaseKey) => false;
        public virtual string PreviewText { get; }
        public string GetSerializedContent() => Note.GetSerializedContent();

        protected string Options;

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
                        CanDeleteOrRestore = true;
                        CanDiscardOrSave = false;
                        CanShare = true;
                        
                        //var x = Thread.CurrentThread;
                        // This line causes TargetInvocationException as SaveAllCommand is owned
                        //if (x.GetApartmentState() == ApartmentState.MTA)
                        //    _mainVM.SaveAllCommand.RaiseCanExecuteChanged();
                        break;
                    case EntityState.Modified:
                        CanDeleteOrRestore = true;
                        CanDiscardOrSave = true;
                        CanShare = false;
                        // This line causes TargetInvocationException as SaveAllCommand is owned
                        //var y = Thread.CurrentThread;
                        //if (y.GetApartmentState() == ApartmentState.MTA)
                        //    _mainVM.SaveAllCommand.RaiseCanExecuteChanged();
                        break;
                    case EntityState.New:
                        CanDeleteOrRestore = false;
                        CanDiscardOrSave = true;
                        CanShare = false;
                        // This line causes TargetInvocationException as SaveAllCommand is owned
                        //var z = Thread.CurrentThread;
                        //if (z.GetApartmentState() == ApartmentState.MTA)
                        //    _mainVM.SaveAllCommand.RaiseCanExecuteChanged();
                        break;
                }
                SaveCommand?.RaiseCanExecuteChanged();
                DiscardCommand?.RaiseCanExecuteChanged();
                ShareCommand?.RaiseCanExecuteChanged();
                TrashUntrashCommand?.RaiseCanExecuteChanged();
            }
        }

        private string _resultMessage;
        public string ResultMessage
        {
            get { return _resultMessage; }
            set { SetProperty(ref _resultMessage, value); }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set { SetProperty(ref _isBusy, value); }
        }

        public virtual void Discard()
        {
            TopicVM.NoteViewModels.Remove(this);
        }

        /// <summary>
        /// Updates the ViewModel state with a possibly non-persisted note that comes from a different source,
        /// such as the local file system or a local database.
        /// </summary>
        public virtual void UpdateStateWithObject(AbstractNote note)
        {
            _newNote = note;
            Title = note.Title;
            Pinned = note.Pinned;
            GloballyPinned = note.GloballyPinned;
            Options = note.Options;
            State = EntityState.New;
            RaiseNoteChangedEvents();
        }

        /// <summary>
        /// Updates the ViewModel state with the persisted note data taken from the local repo.
        /// </summary>
        public virtual void UpdateStateFromRepo()
        {
            Title = Note.Title;
            Pinned = Note.Pinned;
            GloballyPinned = Note.GloballyPinned;
            Options = Note.Options;
            if (Note.IsPersistedLocally())
                State = EntityState.Default;
            else State = EntityState.New;
            RaiseNoteChangedEvents();
        }

        private async Task TrashUntrash()
        {
            try
            {
                // If it is trashed already, untrash
                if (Note.DateDeleted != null)
                {
                    bool success = await _dataAccess.TrashUntrasEntity(Note, null);
                    // On Success:
                    // If the TopicVM happens to also be trashed, just untrash it as well
                    // TODO: This is an OBVIOUS Bug: Here we are untrashing all associated notes while we only wanted to do so for this note
                    if (TopicVM.DateDeleted != null)
                        await TopicVM.TrashUntrashCommand.ExecuteAsync();
                    // Notify a change because the noteslist is changed by adding a note again
                    // TODO: add eventhandle in either MainViewModel or Main.xaml.cs; Currently exists but is commented out
                    _mainVM.HandleNoteUntrashed(this, TopicVM);
                    NotesChanged?.Invoke(this, null);
                }
                // If it is not trashed already, trash it
                else if (Note.DateDeleted == null)
                {
                    DateTime? now = DateTime.UtcNow;
                    bool success = await _dataAccess.TrashUntrasEntity(Note, now);
                    // On Success:
                    TopicVM.RemoveNoteViewModel(this);
                }
                State = EntityState.Default;
            } 
            catch (Exception e)
            {
                ResultMessage = "An unexpected error occurred.";
                Log.Error(e);
            }
        }

        private DataUpdateAction DetermineUpdateAction(AbstractNote changedNote)
        {
            if (!changedNote.IsPersistedLocally())
                return DataUpdateAction.Insert;
            else if (Note.BodyEquals(changedNote))
                return DataUpdateAction.UpdateHeader;
            return DataUpdateAction.UpdateBody;
        }

        protected async Task Save()
        {
            CanDiscardOrSave = false;
            AbstractNote changedNote = CreateChangedNote();
            if (Note.BodyEquals(changedNote) && State == EntityState.Modified)
            {
                State = EntityState.Default;
                return;
            }
            try
            {
                DateTime utcNow = DateTime.UtcNow;
                DataUpdateAction dua = DetermineUpdateAction(changedNote);
                if (dua == DataUpdateAction.Insert)
                    changedNote.DateCreated = changedNote.DateModifiedHeader = changedNote.DateModified = utcNow;
                else if (dua == DataUpdateAction.UpdateBody)
                    changedNote.DateModifiedHeader = changedNote.DateModified = utcNow;
                else changedNote.DateModifiedHeader = utcNow;
                AbstractNote updatedNote = await _dataAccess.InsertOrUpdateNote(changedNote, dua);
                _id = updatedNote.Id;
                UpdateStateFromRepo();
                ResultMessage = "Saved successfully!";
                _mainVM.StatusMessage = $"Encrypted and saved Note '{GetPreviewString()}'";
                //NotesChanged(this, null);
                // TODO: This is spaghetti control flow; rather throw an event or sth
                TopicVM.SortNotes(_mainVM.SettingsViewModel.SelectedNoteSortKey);
            }
            catch (Exception e)
            {
                ResultMessage = "An unexpected error occurred";
                Log.Error(e);
            }
        }

        public async Task TogglePinned()
        {
            try
            {
                if (State != EntityState.New)
                {
                    bool newPinnedValue = !Pinned;
                    DateTime date = await _dataAccess.UpdateNotePinned(Note.Id, newPinnedValue);
                    Pinned = newPinnedValue;
                    //NotesChanged(this, null);
                    // TODO: Like above in the other mehods, refactor the controlflow here potentially
                    TopicVM.SortNotes(_mainVM.SettingsViewModel.SelectedNoteSortKey);
                }
            }
            catch (Exception e)
            {
                ResultMessage = $"{e.GetType()} {e.Message}";
                Log.Error(e);
            }
        }

        public async Task ToggleGloballyPinned()
        {
            try
            {
                if (State != EntityState.New)
                {
                    bool newPinnedValue = !GloballyPinned;
                    DateTime date = await _dataAccess.UpdateNoteGloballyPinned(Note.Id, newPinnedValue);
                    GloballyPinned = newPinnedValue;
                    // Notify MainVM of change OR directly call event handler in MainVM
                    _mainVM.HandleNoteGloballyPinnedChanged(this, newPinnedValue);
                }
            }
            catch (Exception e)
            {
                ResultMessage = $"{e.GetType()} {e.Message}";
                Log.Error(e);
            }
        }

        public async Task ChangeTopic(Guid newTopicId)
        {
            try
            {
                if (State != EntityState.New)
                {
                    DateTime date = await _dataAccess.UpdateNoteTopic(Note.Id, newTopicId);
                }
            } 
            catch (Exception e)
            {
                ResultMessage = $"{e.GetType()} {e.Message}";
                Log.Error(e);
            }
        }

        public async Task<bool> Delete()
        {
            try
            {
                AbstractNote deletedNote = await _dataAccess.DeleteNote(Note);
                _mainVM.HandleEntityDeleted(this);
                return true;
            }
            catch (Exception e)
            {
                ResultMessage = $"{e.GetType()} {e.Message}";
                Log.Error(e);
                return false;
            }
        }

        public string TopicTitle => TopicVM.Title;

        private string _title;
        public string Title
        {
            get { return _title; }
            set 
            { 
                SetProperty(ref _title, value);
                if (State == EntityState.Default)
                {
                    State = EntityState.Modified;
                }
            }
        }

        private bool _pinned;
        public bool Pinned
        {
            get { return _pinned; }
            set 
            { 
                SetProperty(ref _pinned, value);
                TopicVM.SortNotes();
            }
        }

        private bool _globallyPinned;
        public bool GloballyPinned
        {
            get { return _globallyPinned; }
            set { SetProperty(ref _globallyPinned, value); }
        }

        private bool _canDeleteOrRestore;
        public bool CanDeleteOrRestore
        {
            get { return _canDeleteOrRestore; }
            set { SetProperty(ref _canDeleteOrRestore, value); }
        }

        private bool _canDiscardOrSave;
        public bool CanDiscardOrSave
        {
            get { return _canDiscardOrSave; }
            set { SetProperty(ref _canDiscardOrSave, value); }
        }

        private bool _canShare;
        public bool CanShare
        {
            get { return _canShare; }
            set { SetProperty(ref _canShare, value); }
        }

        public string GetPreviewString() => Note.GetPreviewString();
    }
}
