using EazyNotes.Models.DTO;
using EazyNotesDesktop.Library.DAO;
using EazyNotesDesktop.Library.Common;
using EazyNotes.Models.POCO;
using EazyNotesDesktop.Util;
using MvvmHelpers.Commands;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EazyNotes.Common;
using EazyNotesDesktop.Library.Helpers;

namespace EazyNotesDesktop.ViewModels
{
    public class MainViewModel : BindableBase
    {
        private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();

        private readonly DataAccess _dataAccess;
        private readonly ENClient _enClient;

        public MainViewModel(DataAccess dataAccess, ENClient enClient)
        {
            _dataAccess = dataAccess;
            _enClient = enClient;

            SettingsViewModel = new SettingsViewModel(_dataAccess, _enClient.UserData, this);
            TrashBinViewModel = new TrashBinViewModel(_dataAccess, this);
            SearchResultsViewModel = new SearchResultsViewModel();

            CreateNoteCommand = new DelegateCommand(CreateNote, CanCreateNote);
            CreateCheckListNoteCommand = new DelegateCommand(CreateCheckListNote, CanCreateNote);
            CreateTopicCommand = new DelegateCommand(CreateTopic, () => !IsBusy);
            SearchCommand = new DelegateCommand(Search, () => !IsBusy);
            SaveAllCommand = new AsyncCommand(SaveAllUnsavedNotes, (x) => CanSaveAll);
            RefreshNotesCommand = new AsyncCommand(FetchUpdatedNotesOfSelectedTopic, (x) => true);
            RefreshTopicsCommand = new AsyncCommand(FetchTopics, (x) => true);
            UpdateTopicPositionsCommand = new AsyncCommand(UpdateTopicPositions, (x) => true);

            TopicViewModels = new BindingList<TopicViewModel>();
            GloballyPinnedNotes = new BindingList<BaseNoteViewModel>();
            FinishedFetchingData += (s, e) => RefreshGloballyPinnedNotes();

            // Subscribe to Websocket events from DataAccess class
            _dataAccess.DataChanged += (s, e) => ApplyDataChange(e.SyncAction, e.ChangeData);

            RunAPIConnectionWatcher();
            _dataAccess.RegisterClient();
            //FetchData().ConfigureAwait(false);
        }

        public event EventHandler RequestShowAlertBox;
        public event EventHandler RequestShowSearchResult;
        public event EventHandler TopicsChanged;
        public event EventHandler RequestShowTopicDialog;
        public event EventHandler RequestShowSpinner;
        public event EventHandler RequestCloseSpinner;
        public event EventHandler SelectedTopicChanged;
        public event EventHandler FinishedFetchingData;

        public DelegateCommand CreateNoteCommand { get; set; }
        public DelegateCommand CreateCheckListNoteCommand { get; set; }
        public DelegateCommand CreateTopicCommand { get; set; }
        public DelegateCommand SearchCommand { get; set; }
        public AsyncCommand SaveAllCommand { get; set; }
        public AsyncCommand RefreshNotesCommand { get; set; }
        public AsyncCommand RefreshTopicsCommand { get; set; }
        public AsyncCommand UpdateTopicPositionsCommand { get; set; }

        public SearchResultsViewModel SearchResultsViewModel;

        private string _statusMessage;
        public string StatusMessage
        {
            get { return _statusMessage; }
            set { SetProperty(ref _statusMessage, value.Replace(Environment.NewLine, " ")); }
        }

        private string _datasourceConnectionStatus;
        public string DatasourceConnectionStatus
        {
            get { return _datasourceConnectionStatus; }
            set { SetProperty(ref _datasourceConnectionStatus, value); }
        }

        private string _hoveredItemToolTip;
        public string HoveredItemToolTip
        {
            get { return _hoveredItemToolTip; }
            set { SetProperty(ref _hoveredItemToolTip, value); }
        }

        private SettingsViewModel _settingsViewModel;
        public SettingsViewModel SettingsViewModel
        {
            get { return _settingsViewModel; }
            set { SetProperty(ref _settingsViewModel, value); }
        }

        private TrashBinViewModel _trashBinViewModel;
        public TrashBinViewModel TrashBinViewModel
        {
            get { return _trashBinViewModel; }
            set { SetProperty(ref _trashBinViewModel, value); }
        }

        private BindingList<TopicViewModel> _topicViewModels;
        public BindingList<TopicViewModel> TopicViewModels
        {
            get { return _topicViewModels; }
            set { SetProperty(ref _topicViewModels, value); }
        }

        private BindingList<BaseNoteViewModel> _globallyPinnedNotes;
        public BindingList<BaseNoteViewModel> GloballyPinnedNotes
        {
            get { return _globallyPinnedNotes; }
            set { SetProperty(ref _globallyPinnedNotes, value); }
        }

        private TopicViewModel _selectedTopicVM;
        public TopicViewModel SelectedTopicVM
        {
            get { return _selectedTopicVM; }
            set { SetProperty(ref _selectedTopicVM, value); SelectedTopicChanged?.Invoke(null, null); }
        }

        private BaseNoteViewModel _selectedNoteVM;
        public BaseNoteViewModel SelectedNoteVM
        {
            get { return _selectedNoteVM; }
            set { SetProperty(ref _selectedNoteVM, value); }
        }

        private bool _isDraggingTopic;
        public bool IsDraggingTopic
        {
            get { return _isDraggingTopic; }
            set { SetProperty(ref _isDraggingTopic, value); }
        }

        private bool _canNavigateToSettings;
        public bool CanNavigateToSettings
        {
            get { return _canNavigateToSettings; }
            set { SetProperty(ref _canNavigateToSettings, value); }
        }

        public void RaiseTopicsChanged()
        {
            TopicsChanged(this, null);
            CreateCheckListNoteCommand.RaiseCanExecuteChanged();
            CreateNoteCommand.RaiseCanExecuteChanged();
        }

        public void RaiseRequestShowTopicDialog(object sender)
        {
            RequestShowTopicDialog(sender, null);
        }

        private string _searchTerm;
        public string SearchTerm
        {
            get { return _searchTerm; }
            set { SetProperty(ref _searchTerm, value); }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set 
            { 
                SetProperty(ref _isBusy, value);
                CanNavigateToSettings = !value;
                CreateCheckListNoteCommand?.RaiseCanExecuteChanged();
                CreateNoteCommand?.RaiseCanExecuteChanged();
                CreateTopicCommand?.RaiseCanExecuteChanged();
                SearchCommand?.RaiseCanExecuteChanged();
                var x = Thread.CurrentThread;
                // TODO: Fix: SaveAllCmd is the only async cmd and it throws "TargetInvocationException"
                // when FetchNotes() sets IsBusy to true which then tries to call this line; it says it
                // cannot access this object as it (the thread) doesn't own it
                //if (x.GetApartmentState() == ApartmentState.MTA)
                //    SaveAllCommand?.RaiseCanExecuteChanged();
            }
        }

        private bool CanCreateNote()
        {
            return TopicViewModels.Count > 0 && !IsBusy;
        }

        // Todo: Fix the TargetInvocationException for SaveAllCommand then uncomment this
        public bool CanSaveAll => !IsBusy /*&& CheckForUnsavedNotes().Count > 0*/;

        private int GetNoteInsertionIndex(TopicViewModel topicVM)
        {
            int nextIdx = 0;
            if (topicVM.NoteViewModels.Count == 0)
                return nextIdx;
            while (topicVM.NoteViewModels.Count > nextIdx && topicVM.NoteViewModels[nextIdx].Pinned)
                nextIdx++;
            return nextIdx;
        }

        public AbstractNote CreateNewNote(Type noteType, Guid topicId)
        {
            if (noteType == typeof(SimpleNote))
                return new SimpleNote(_enClient.User.Id, topicId);
            else if (noteType == typeof(CheckListNote))
                return new CheckListNote(_enClient.User.Id, topicId);
            else throw new NotSupportedException($"NoteType {noteType} not supported");
        }

        public Topic CreateNewTopic()
        {
            return new Topic(_enClient.User.Id, Symbols.DefaultTopicSymbol, ColorRef.DefaultTopicColor);
        }

        private void CreateNote()
        {
            // TODO: Rethink how to handle note creation without topic
            Guid topicId = Guid.Empty;
            if (SelectedTopicVM == null)
            {
                topicId = TopicViewModels[0].Topic.Id;
                SelectedTopicVM = TopicViewModels[0];
            }
            else topicId = SelectedTopicVM.Topic.Id;
            if (topicId == Guid.Empty)
                return;
            NoteViewModel noteVM = new NoteViewModel(_dataAccess, this, SelectedTopicVM, (SimpleNote)CreateNewNote(typeof(SimpleNote), topicId));
            SelectedTopicVM.NoteViewModels.Insert(GetNoteInsertionIndex(SelectedTopicVM), noteVM);
            SelectedNoteVM = noteVM;
        }

        private void CreateCheckListNote()
        {
            // TODO: Rethink how to handle note creation without topic
            Guid topicId = Guid.Empty;
            if (SelectedTopicVM == null) { 
                topicId = TopicViewModels[0].Topic.Id;
                SelectedTopicVM = TopicViewModels[0]; 
            }
            else topicId = SelectedTopicVM.Topic.Id;
            if (topicId == Guid.Empty)
                return;
            CheckListNoteViewModel noteVM = new CheckListNoteViewModel(_dataAccess,
                this, SelectedTopicVM, (CheckListNote)CreateNewNote(typeof(CheckListNote), topicId));
            SelectedTopicVM.NoteViewModels.Insert(GetNoteInsertionIndex(SelectedTopicVM), noteVM);
            SelectedNoteVM = noteVM;
        }

        private void CreateTopic()
        {
            TopicViewModel topicVM = new TopicViewModel(_dataAccess, this, Guid.Empty);
            RaiseRequestShowTopicDialog(topicVM);
        }

        public void HandleTopicCreated(TopicViewModel topicVM)
        {
            TopicViewModels.Insert(0, topicVM);
            SelectedTopicVM = topicVM;
        }

        private void Search()
        {
            if (SearchTerm == null || string.IsNullOrWhiteSpace(SearchTerm))
            {
                // DO NOTHING; just show previous search results 
            }
            else
            {
                string uppercaseKey = SearchTerm.ToUpper();
                IEnumerable<BaseNoteViewModel> result = null;
                foreach(var colVM in TopicViewModels) {
                    var result_n = colVM.NoteViewModels.Where((note) => note.ContainsSearchKey(uppercaseKey));
                    if (result == null)
                        result = result_n;
                    else result = result.Concat(result_n);
                }
                SearchResultsViewModel.SearchTerm = SearchTerm; 
                SearchResultsViewModel.NoteViewModels.Clear();
                SearchResultsViewModel.NoteViewModels.AddRange(result);
            }
            RequestShowSearchResult(this, new EventArgs());
        }

        /// <summary>
        /// Adds the BaseNoteViewModel instance to the list of NoteViewModels inside the respective TopicViewModel.
        /// </summary>
        public void HandleNoteUntrashed(BaseNoteViewModel noteVM, TopicViewModel topicVM = null)
        {
            if (topicVM == null)
                topicVM = TopicViewModels.SingleOrDefault(t => t.Topic.Id == noteVM.Note.TopicId);
            if (topicVM == null)
                // shouldnt happen but ok
                return;
            topicVM.AddNoteViewModel(noteVM);
            TrashBinViewModel.TrashedEntities.Remove(noteVM);
        }

        /// <summary>
        /// Removes the TopicViewModel from the list of TopicViewModels and fires events to update UI.
        /// </summary>
        public void HandleTopicTrashed(TopicViewModel topicVM)
        {
            TopicViewModels.Remove(topicVM);
            CreateNoteCommand.RaiseCanExecuteChanged();
            CreateCheckListNoteCommand.RaiseCanExecuteChanged();
        }

        public void HandleTopicUntrashed(TopicViewModel topicVM)
        {
            TopicViewModels.Add(topicVM);
            CreateNoteCommand.RaiseCanExecuteChanged();
            CreateCheckListNoteCommand.RaiseCanExecuteChanged();
            TrashBinViewModel.TrashedEntities.Remove(topicVM);
        }

        public void HandleNoteTopicChanged(BaseNoteViewModel noteVM)
        {
            SelectedTopicVM.RemoveNoteViewModel(noteVM);
            TopicViewModel newTopicVM = TopicViewModels.SingleOrDefault(tvm => tvm.Topic.Id == noteVM.Note.TopicId);
            newTopicVM.AddNoteViewModel(noteVM);
        }

        public void HandleNoteGloballyPinnedChanged(BaseNoteViewModel noteVM, bool newValue)
        {
            if (newValue)
                GloballyPinnedNotes.Add(noteVM);
            else GloballyPinnedNotes.Remove(noteVM);
        }

        /// <summary>
        /// This is the method which updates the topic positions if a new ground-truth has reached this client, e.g. by way of a sync triggered from another client.
        /// </summary>
        public void HandleTopicPositionChanged()
        {
            TopicViewModel selection = SelectedTopicVM;
            TopicViewModels = new BindingList<TopicViewModel>(TopicViewModels.OrderBy(tvm => tvm.SelectedPosition).ToList());
            SelectedTopicVM = selection;
        }

        public void HandleEntityDeleted(IEntityViewModel entityVM)
        {
            TrashBinViewModel.TrashedEntities.Remove(entityVM);
            TrashBinViewModel.RaiseItemCountChanged();
        }

        public void RaiseGloballyPinnedNotesChanged()
        {
            FinishedFetchingData?.Invoke(this, null);
        }

        public void HandleSelectedNoteChanged(BaseNoteViewModel selectedNoteVM)
        {
            if (selectedNoteVM == null)
                return;
            SelectedTopicVM = selectedNoteVM.TopicVM;
            SelectedNoteVM = selectedNoteVM;
        }

        public void RefreshGloballyPinnedNotes()
        {
            GloballyPinnedNotes.Clear();
            TopicViewModels.ForEach(tvm => tvm.NoteViewModels.ForEach(nvm => { if (nvm.GloballyPinned) GloballyPinnedNotes.Add(nvm); }));
        }

        /// <summary>
        /// This is the client-side method which sets the SelectedPosition property in each TopicViewModel after the positions have just changed.
        /// Concretely, this method just sets each topicVM's "SelectedPosition" property to the respective index of that topicVM in the list of topicVMs.
        /// This method has no bearing on persistence of topic positions yet!
        /// </summary>
        public void SetTopicPositionPropertiesToTheirIndex()
        {
            for (int i=0; i<TopicViewModels.Count; i++)
                TopicViewModels[i].SelectedPosition = i;
        }

        public async Task<DateTime> UpdateTopicPositions()
        {
            TopicPositionData tpd = new TopicPositionData();
            for (int i=0; i<TopicViewModels.Count; i++)
            {
                tpd.TopicIds.Add(TopicViewModels[i].Topic.Id);
                tpd.TopicPositions.Add(TopicViewModels[i].SelectedPosition);
            }
            tpd.TimeStamp = DateTime.UtcNow;
            var result = await _dataAccess.UpdateTopicPositions(tpd);
            // TODO: uh maybe show error if it failed (aka result != 0) or maybe just ignore it ... it's not the most important thing after all.
            return result;
        }

        public List<BaseNoteViewModel> FilterNoteVMsByUnsaved()
        {
            List<BaseNoteViewModel> unsavedNotes = new List<BaseNoteViewModel>();
            foreach (TopicViewModel tvm in TopicViewModels)
                unsavedNotes.AddRange(tvm.NoteViewModels.Where(nvm => nvm.State != EntityState.Default).ToList());
            return unsavedNotes;
        }

        private async Task SaveAllUnsavedNotes()
        {
            List<BaseNoteViewModel> unsavedNotes = FilterNoteVMsByUnsaved();
            if (unsavedNotes.Count == 0)
            {
                StatusMessage = "Everything saved already.";
                return;
            }
            IsBusy = true;
            foreach (BaseNoteViewModel noteVM in unsavedNotes)
                await noteVM.SaveCommand.ExecuteAsync();
            StatusMessage = $"Saved and encrypted {unsavedNotes.Count} Note(s).";
            IsBusy = false;
        }

        public async Task FetchTopics()
        {
            Guid selectedId = Guid.Empty;
            if (SelectedTopicVM != null)
                selectedId = SelectedTopicVM.Topic.Id;
            await _dataAccess.GetTopics();
            List<Topic> updatedTopics = _dataAccess.Topics.Values.ToList()
                .Where(t => t.DateDeleted == null).ToList();
            // Remove those local TopicViewModels who don't have a matching topic in the updatedTopics list
            int i = 0;
            while (true)
            {
                if (TopicViewModels[i].Topic.IsPersistedLocally() == false)
                    TopicViewModels.RemoveAt(i);
                else i++;
                if (i == TopicViewModels.Count)
                    break;
            }
            TopicViewModels.ForEach(tvm => {
                // Update the existing Topics
                tvm.UpdateStateFromRepo();
                // Remove the updated ones from the updatedTopics list
                updatedTopics.RemoveAll(topic => topic.Id == tvm.Topic.Id);
                });
            // Now updatedTopics only contains completely new topics => map to VMs and add
            updatedTopics.ForEach(t => TopicViewModels.Add(new TopicViewModel(_dataAccess, this, t.Id)));

            HandleTopicPositionChanged();

            if (selectedId != Guid.Empty)
                SelectedTopicVM = TopicViewModels.SingleOrDefault(t => t.Topic.Id == selectedId);
        }

        public async Task FetchUpdatedNotesOfSelectedTopic()
        {
            // TODO: Consider the case where the topic was in the meantime trashed/deleted ==> currently causes a crash
            try
            {
                if (SelectedTopicVM == null)
                    return;
                // Preserve note selection for seamless experience
                Guid selectedId = Guid.Empty;
                if (SelectedNoteVM != null)
                    selectedId = SelectedNoteVM.Note.Id;
                // Get Metadata of all current notes in selected Topic on client 
                List<NoteHeaderDTO> clientNoteMetadatas = _dataAccess.Notes.Values.ToList().Where(n => n.TopicId == SelectedTopicVM.Topic.Id && n.DateDeleted == null)
                    .ToList().ConvertAll(n => n.GetHeader());
                ClientStateOfNotesInTopic clientNoteSyncInfo = new ClientStateOfNotesInTopic(SelectedTopicVM.Topic.GetHeader(), clientNoteMetadatas);
                // Make API Call to retrieve updated notes
                List<AbstractNote> updatedNotes = await _dataAccess.GetUpdatedNotesByTopic(clientNoteSyncInfo);
                if (updatedNotes == null)
                {
                    HandleTopicTrashed(SelectedTopicVM);
                    return;
                }
                // Clear all local Note ViewModels
                SelectedTopicVM.NoteViewModels.Clear();
                // And recreate them from the now updated Notes Repository
                _dataAccess.Notes.Values.ToList()
                    .Where(n => n.TopicId == SelectedTopicVM.Topic.Id && n.DateDeleted == null)
                    .ForEach(note =>
                    {
                        BaseNoteViewModel noteVM = BaseNoteViewModel.FromNote(note, _dataAccess, this, SelectedTopicVM);
                        SelectedTopicVM.AddNoteViewModel(noteVM);
                    });
                // Reselect the previously selected Note for seamless experience
                if (selectedId != Guid.Empty)
                    SelectedNoteVM = SelectedTopicVM.NoteViewModels.SingleOrDefault(n => n.Note.Id == selectedId);

                // Finally, refresh globally pinned notes as well
                RefreshGloballyPinnedNotes();

                StatusMessage = $"Updated {updatedNotes.Count} Notes.";
            } catch (Exception e)
            {
                RequestShowAlertBox?.Invoke(new AlertBoxArgs("An unexpected error occurred."), null);
            }
        }

        public async Task FetchData()
        {
            IsBusy = true;
            await Task.Run(async () =>
            {
                Log.Info("Begin fetching data ...");
                try
                {
                    Log.Info("Fetching topics and notes ...");
                    // https://blog.stephencleary.com/2012/02/async-and-await.html
                    Task[] tasks = new Task[2];
                    tasks[0] = _dataAccess.GetTopics();
                    tasks[1] = _dataAccess.GetNotes();
                    await Task.WhenAll(tasks);

                    IList<TopicViewModel> topicVMS = _dataAccess.Topics.Values
                        .Where(t => t.DateDeleted == null).ToList()
                        .ConvertAll(t => new TopicViewModel(_dataAccess, this, t.Id));

                    IList<AbstractNote> notes = _dataAccess.Notes.Values
                        .Where(t => t.DateDeleted == null).ToList().ToList();

                    foreach (AbstractNote note in notes)
                    {
                        TopicViewModel topicVM = topicVMS
                            .SingleOrDefault(tvm => tvm.Topic.Id == note.TopicId);
                        if (topicVM == null)
                            continue;
                        BaseNoteViewModel noteVM = BaseNoteViewModel.FromNote(note, _dataAccess, this, topicVM);
                        topicVM.AddNoteViewModel(noteVM);
                    }

                    // Refresh globally pinned ntes from repo
                    //GloballyPinnedNotes = new BindingList<AbstractNote>();
                    //RefreshGloballyPinnedNotes();
                    //var x = Thread.CurrentThread;
                    //if (x.GetApartmentState() == ApartmentState.MTA)
                    //    SaveAllCommand?.RaiseCanExecuteChanged();
                    //FinishedFetchingData?.Invoke(this, null);

                    // Remove the trashed topics from the topics list from the api
                    TopicViewModels = new BindingList<TopicViewModel>(topicVMS);
                    HandleTopicPositionChanged();
                    CreateCheckListNoteCommand.RaiseCanExecuteChanged();
                    CreateNoteCommand.RaiseCanExecuteChanged();
                    Log.Info("Finished fetching data!");
                    RequestCloseSpinner?.Invoke(this, null);
                }
                // TODO: More granular error catching with appropriate user feedback
                // CryptographicException -> Say that the encrypted data seems to be invalid
                catch (Exception e)
                {
                    Log.Error(e);
                    throw (e);
                }
            });
        }

        private void ApplyDataChange(SyncAction syncAction, object changeData)
        {
            if (changeData is AbstractNote updatedNote)
            {
                TopicViewModel topicVM;
                BaseNoteViewModel noteVM = null;

                switch (syncAction)
                {
                    case SyncAction.Create:
                        topicVM = TopicViewModels.SingleOrDefault(t => t.Topic.Id == (changeData as AbstractNote).TopicId);
                        if (topicVM == null)
                            // out of sync ==> Resync from API
                            return;
                        noteVM = BaseNoteViewModel.FromNote(changeData as AbstractNote, _dataAccess, this, topicVM);
                        topicVM.AddNoteViewModel(noteVM);
                        break;

                    case SyncAction.Update:
                        topicVM = TopicViewModels.SingleOrDefault(t => t.Topic.Id == (changeData as AbstractNote).TopicId);
                        if (topicVM == null)
                            // out of sync ==> Resync from API
                            return;
                        noteVM = topicVM.NoteViewModels
                            .SingleOrDefault(nvm => nvm.Note.Id == updatedNote.Id);
                        if (noteVM == null)
                            // out of sync ==> Resync from API
                            return;
                        noteVM.UpdateStateFromRepo();
                        break;

                    default:
                        return;
                }
            } 
            else if (changeData is NoteHeaderDTO noteMetadata)
            {
                TopicViewModel topicVM = null;
                BaseNoteViewModel noteVM = null;

                topicVM = TopicViewModels.SingleOrDefault(tvm => tvm.Topic.Id == noteMetadata.TopicId);
                // in the case that date deleted != null, we want to trash the topic, so only then topicVM may be null
                if (topicVM == null)
                    // out of sync ==> Resync from API
                    return;
                noteVM = topicVM.NoteViewModels.SingleOrDefault(nvm => nvm.Note.Id == noteMetadata.Id);
                if (noteVM == null && noteMetadata.DateDeleted != null)
                    // out of sync ==> Resync from API
                    return;

                switch (syncAction)
                {
                    case SyncAction.UpdateNoteOptions:
                        // TODO: Update Note Options
                        NoteViewModel simpleNoteVM = noteVM as NoteViewModel;
                        if (simpleNoteVM == null)
                            break;
                        simpleNoteVM.WrapText = simpleNoteVM.Note.HasFlag(NoteOptions.WRAP_TEXT);
                        break;

                    case SyncAction.UpdateNotePinned:
                        noteVM.Pinned = noteMetadata.Pinned;
                        break;

                    case SyncAction.UpdateNoteTopic:
                        TopicViewModel newTopicVM = TopicViewModels.SingleOrDefault(tvm => tvm.Topic.Id == noteMetadata.TopicId);
                        if (newTopicVM == null)
                            // out of sync ==> Resync from API
                            return;
                        newTopicVM.AddNoteViewModel(noteVM);
                        topicVM.RemoveNoteViewModel(noteVM);
                        break;

                    case SyncAction.ToggleTrash:
                        if (noteMetadata.DateDeleted == null)
                        {
                            AbstractNote note = _dataAccess.Notes[noteMetadata.Id];
                            noteVM = BaseNoteViewModel.FromNote(changeData as AbstractNote, _dataAccess, this, topicVM);
                            HandleNoteUntrashed(noteVM, topicVM);
                        }
                        else
                        {
                            noteVM.Note.DateDeleted = noteMetadata.DateDeleted;
                            topicVM.RemoveNoteViewModel(noteVM);
                        }
                        break;

                    case SyncAction.Delete:
                        // TODO: While this sync is happening, we could be viewing the trash bin, so we can fire an event that it updates itself from repository
                        //TrashBinViewModel.RaiseEntitiesChanged();
                        break;

                    default:
                        return;
                }
            }
            else if (changeData is Topic updatedTopic)
            {
                TopicViewModel topicVM;
                switch (syncAction)
                {
                    case SyncAction.Create:
                        topicVM = new TopicViewModel(_dataAccess, this, updatedTopic.Id);
                        HandleTopicCreated(topicVM);
                        break;

                    case SyncAction.Update:
                        topicVM = TopicViewModels.SingleOrDefault(tvm => tvm.Topic.Id == updatedTopic.Id);
                        if (topicVM == null)
                            // out of sync ==> Resync from API
                            return;
                        topicVM.UpdateStateFromRepo();
                        break;

                    default: 
                        return;
                }
            }
            else if (changeData is TopicHeaderDTO topicMetadata)
            {
                TopicViewModel topicVM = null;
                switch (syncAction)
                {
                    case SyncAction.ToggleTrash:
                        if (topicMetadata.DateDeleted == null)
                        {
                            Topic topic = _dataAccess.Topics[topicMetadata.Id];
                            topicVM = new TopicViewModel(_dataAccess, this, topic.Id);
                            HandleTopicUntrashed(topicVM);
                        }
                        else
                        {
                            topicVM = TopicViewModels.SingleOrDefault(tvm => tvm.Topic.Id == topicMetadata.Id);
                            HandleTopicTrashed(topicVM);
                        }
                        break;

                    case SyncAction.UpdateTopicSymbol:
                        topicVM = TopicViewModels.SingleOrDefault(tvm => tvm.Topic.Id == topicMetadata.Id);
                        if (topicVM == null)
                            // out of sync --> Resync from API
                            return;
                        topicVM.Topic.Symbol = topicVM.SelectedSymbol = topicMetadata.Symbol;
                        topicVM.RaiseTopicChangedEvents();
                        break;

                    case SyncAction.UpdateTopicColor:
                        topicVM = TopicViewModels.SingleOrDefault(tvm => tvm.Topic.Id == topicMetadata.Id);
                        if (topicVM == null)
                            // out of sync --> Resync from API
                            return;
                        topicVM.Topic.Color = topicVM.SelectedColor = topicMetadata.Color;
                        topicVM.RaiseTopicChangedEvents();
                        break;

                    case SyncAction.UpdateTopicPosition:
                        topicVM = TopicViewModels.SingleOrDefault(tvm => tvm.Topic.Id == topicMetadata.Id);
                        if (topicVM == null)
                            // out of sync --> Resync from API
                            return;
                        int originalPosition = topicVM.Topic.Position;
                        if (originalPosition == topicMetadata.Position)
                            break;
                        topicVM.Topic.Position = topicMetadata.Position;
                        if (originalPosition < topicMetadata.Position)
                        {
                            for (int i = originalPosition; i < topicMetadata.Position; i++)
                                TopicViewModels[i].Topic.Position--;
                        }
                        else if (originalPosition > topicMetadata.Position)
                        {
                            for (int i = topicMetadata.Position + 1; i <= originalPosition; i++)
                                TopicViewModels[i].Topic.Position++;
                        }
                        topicVM.RaiseTopicChangedEvents();
                        break;

                    case SyncAction.Delete:
                        // TODO: While this sync is happening, we could be viewing the trash bin, so we can fire an event that it updates itself from repository
                        //TrashBinViewModel.RaiseEntitiesChanged();
                        break;

                    default:
                        return;
                }
            }
        }

        private async Task RunAPIConnectionWatcher()
        {
            while (true)
            {
                await Task.Run(async () => {
                    bool connected = await _dataAccess.CheckConnection();
                    if (!connected)
                        DatasourceConnectionStatus = "Disconnected!";
                    else DatasourceConnectionStatus = "Connected";
                    await Task.Delay(10000);
                });
            }
        }
    }
}
