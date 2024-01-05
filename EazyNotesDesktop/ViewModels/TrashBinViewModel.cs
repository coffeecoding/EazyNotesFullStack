using EazyNotesDesktop.Library.DAO;
using EazyNotes.Models.POCO;
using MvvmHelpers.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel;

namespace EazyNotesDesktop.ViewModels
{
    public class TrashBinViewModel : BindableBase
    {
        private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();

        private MainViewModel _mainVM;
        public MainViewModel MainVM
        {
            get { return _mainVM; }
            set { SetProperty(ref _mainVM, value); }
        }

        private readonly DataAccess _dataAccess;

        public TrashBinViewModel(DataAccess dataAccess, MainViewModel mainViewModel)
        {
            _dataAccess = dataAccess;
            MainVM = mainViewModel;

            DeleteTrashedItemsCommand = new AsyncCommand(DeleteTrashedItems,
                (x) => TrashedItemCount > 0);
        }

        public event EventHandler RequestShowSpinner;
        public event EventHandler RequestCloseSpinner;

        public AsyncCommand DeleteTrashedItemsCommand { get; set; }

        private IEntityViewModel _selectedTrashedEntity;
        public IEntityViewModel SelectedTrashedEntity
        {
            get { return _selectedTrashedEntity; }
            set { SetProperty(ref _selectedTrashedEntity, value); }
        }

        private BindingList<IEntityViewModel> _trashedEntities;
        public BindingList<IEntityViewModel> TrashedEntities 
        { 
            get { return _trashedEntities; } 
            set { SetProperty(ref _trashedEntities, value); } 
        }

        public int TrashedItemCount { get => TrashedEntities == null ? 0 : TrashedEntities.Count; }

        public bool IsDataDownloaded { get; private set; }

        public void RaiseItemCountChanged() => RaisePropertyChanged("TrashedItemCount");

        private async Task DeleteTrashedItems()
        {
            Log.Info("Begin deleting items from trashbin ...");
            RequestShowSpinner?.Invoke("Deleting items ...", null);
            try
            {
                while (TrashedEntities?.Count > 0)
                {
                    await TrashedEntities[0].DeleteCommand.ExecuteAsync();
                    TrashedEntities.RemoveAt(0);
                }
                DeleteTrashedItemsCommand.RaiseCanExecuteChanged();
            }
            catch (Exception e)
            {
                // TODO: show error message to user
                Log.Error(e);
            }
            finally
            {
                RequestCloseSpinner?.Invoke(null, null);
            }
        }

        public async Task FetchTrashedItems()
        {
            //await Task.Run(async () =>
            //{
            try
            {
                MainVM.SettingsViewModel.IsNotBusy = false;
                DeleteTrashedItemsCommand.RaiseCanExecuteChanged();
                bool isTrashEmpty = TrashedEntities == null || TrashedEntities.Count == 0;
                if (isTrashEmpty)
                {
                    Log.Info("Retrieving trashed data ...");
                    List<Topic> trashedTopics = await _dataAccess.GetTrashedTopics();
                    List<AbstractNote> trashedNotes = await _dataAccess.GetTrashedNotes();

                    MapDataToViewModels(trashedTopics, trashedNotes);
                }
                else
                {
                    RefreshTrashedDataFromRepo();
                    TrashedEntities.OrderBy(t => t.DateDeleted);
                }
                IsDataDownloaded = true;
                DeleteTrashedItemsCommand.RaiseCanExecuteChanged();
                Log.Info("Finished fetching trashed items!");
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw (e);
            }
            //});
        }

        public void RefreshTrashedDataFromRepo()
        {
            List<Topic> trashedTopics = _dataAccess.Topics.Values.Where(t => t.DateDeleted != null).ToList();
            List<AbstractNote> trashedNotes = _dataAccess.Notes.Values.Where(n => n.DateDeleted != null).ToList();

            MapDataToViewModels(trashedTopics, trashedNotes);
        }

        private void MapDataToViewModels(List<Topic> topics, List<AbstractNote> notes)
        {
            TrashedEntities = new BindingList<IEntityViewModel>();
            List<TopicViewModel> trashedTopicVMs = topics.ConvertAll(t => new TopicViewModel(_dataAccess, MainVM, t.Id));
            TrashedEntities.AddRange(trashedTopicVMs);

            List<AbstractNote> trashedNotes = notes.ToList();

            foreach (AbstractNote note in trashedNotes)
            {
                TopicViewModel topicVM = (TopicViewModel)TrashedEntities.Where(e => e is TopicViewModel)
                    .SingleOrDefault(t => t is TopicViewModel tvm && tvm.Topic.Id == note.TopicId);
                if (topicVM == null)
                    topicVM = MainVM.TopicViewModels
                    .SingleOrDefault(tvm => tvm.Topic.Id == note.TopicId);
                BaseNoteViewModel noteVM = BaseNoteViewModel.FromNote(note, _dataAccess, _mainVM, topicVM);
                TrashedEntities.Add(noteVM);
            }

            RaiseItemCountChanged();
        }

        public int GetRequestShowSpinnerHandlerCount()
        {
            return RequestShowSpinner == null ? 0 : RequestShowSpinner.GetInvocationList().Length;
        }

        public int GetRequestCloseSpinnerHandlerCount()
        {
            return RequestCloseSpinner == null ? 0 : RequestCloseSpinner.GetInvocationList().Length;
        }
    }
}
