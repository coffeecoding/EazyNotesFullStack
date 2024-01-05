using EazyNotesDesktop.Library.Common;
using EazyNotes.Models.POCO;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using EazyNotes.Models.DTO;
using EazyNotesDesktop.Library.Helpers;
using EazyNotes.Common;

/// <summary>
/// Data Access Object, acts as the local repository mirroring the database.
/// </summary>
namespace EazyNotesDesktop.Library.DAO
{
    public enum RepoResult
    {
        APISuccessLOCALSuccess,
        APISuccessLOCALFailure,
        APIFailureLOCALSuccess,
        APIFailureLOCALFailure
    }

    public enum DataUpdateAction
    {
        Insert,
        UpdateBody,
        UpdateHeader
    }

    public class DataAccess
    {
        private readonly ICryptoRoutines _cryptoRoutines;

        protected SQLiteDataSource _sqLiteDataSource;
        protected APIDataSource _apiDataSource;

        public Dictionary<Guid, AbstractNote> Notes => _sqLiteDataSource.Notes;
        public Dictionary<Guid, Topic> Topics => _sqLiteDataSource.Topics;

        public DataAccess(ICryptoRoutines cryptoRoutines)
        {
            _cryptoRoutines = cryptoRoutines;
        }

        public void RenewDataSourceClients(ENClient enClient)
        {
            _apiDataSource = new APIDataSource(enClient, _cryptoRoutines);
            _sqLiteDataSource = new SQLiteDataSource(enClient, _cryptoRoutines);
            _sqLiteDataSource.SetConnectionStringAndOpenConnection(enClient.User.Username);

            // Propagate Events From APIDataSource to DataAccess, so they can be subscribed to from the outside
            // This is relevant for the SignalR events 
            if (DataChanged != null)
                foreach (Delegate d in DataChanged.GetInvocationList())
                    DataChanged -= (EventHandler<DataChangedEventArgs>)d;
            _apiDataSource.DataChanged += (s, e) =>
            {
                DataChanged?.Invoke(s, e);
            };
        }

        public event EventHandler<DataChangedEventArgs> DataChanged;

        public void Reset() => _sqLiteDataSource.Reset();

        public Task<bool> CheckConnection() => _apiDataSource.CheckConnection();

        public Task RegisterClient() => _sqLiteDataSource.RegisterClient();

        public async Task<HttpStatusCode> UpdateUser(User user)
        {
            HttpStatusCode status = await _apiDataSource.UpdateUser(user);
            if (status == HttpStatusCode.OK)
                return await _sqLiteDataSource.UpdateUser(user);
            throw new Exception($"User update failed with http status ${status}");
        }

        public Task<List<AbstractNote>> GetNotes() => _sqLiteDataSource.GetNotes();
        public Task<List<AbstractNote>> GetTrashedNotes() => _sqLiteDataSource.GetTrashedNotes();
        public AbstractNote GetNote(Guid id) => _sqLiteDataSource.GetNote(id);

        /// <summary>
        /// Returns null if the Topic was deleted or trashed.
        /// </summary>
        public Task<List<AbstractNote>> GetUpdatedNotesByTopic(ClientStateOfNotesInTopic clientNotesInfo)
            => _sqLiteDataSource.GetUpdatedNotesByTopic(clientNotesInfo);

        public async Task<AbstractNote> InsertOrUpdateNote(AbstractNote note, DataUpdateAction dua)
        {
            bool sqliteSuccess = await _sqLiteDataSource.InsertOrUpdateNote(note, dua);
            if (sqliteSuccess)
                _ = Task.Run(async () =>
                {
                    bool apiSuccess = await _apiDataSource.InsertOrUpdateNote(note, dua);
                    DateTime? dateSynced = apiSuccess ? new DateTime?(note.DateModifiedHeader) : null;
                    await _sqLiteDataSource.InsertEntitySyncState(note.ToEntitySyncState(dateSynced));
                }).ConfigureAwait(false);
            return note;
        }

        public async Task<List<AbstractNote>> InsertNotes(List<AbstractNote> notes)
        {
            bool sqliteSuccess = await _sqLiteDataSource.InsertNotes(notes);
            if (sqliteSuccess)
                _ = Task.Run(async () =>
                {
                    bool apiSuccess = await _apiDataSource.InsertNotes(notes);
                    DateTime? dateSynced = apiSuccess ? new DateTime?(notes[0].DateCreated) : null;
                    var tasks = notes.ConvertAll(n => _sqLiteDataSource.InsertEntitySyncState(n.ToEntitySyncState(dateSynced)));
                    await Task.WhenAll(tasks);
                }).ConfigureAwait(false);
            return notes;
        }

        public async Task<DateTime> UpdateNoteTopic(Guid id, Guid newTopicId)
        {
            DateTime now = DateTime.UtcNow;
            bool sqliteSuccess = await _sqLiteDataSource.UpdateNoteTopic(id, newTopicId, now);
            if (sqliteSuccess)
                _ = Task.Run(async () =>
                {
                    bool apiSuccess = await _apiDataSource.UpdateNoteTopic(id, newTopicId, now);
                    if (apiSuccess)
                        await _sqLiteDataSource.UpdateEntitySyncState(Notes[id].ToEntitySyncState(now));
                }).ConfigureAwait(false);
            return now;
        }

        public async Task<DateTime> UpdateNotePinned(Guid id, bool pinned)
        {
            DateTime now = DateTime.UtcNow;
            bool sqliteSuccess = await _sqLiteDataSource.UpdateNotePinned(id, pinned, now);
            if (sqliteSuccess)
                _ = Task.Run(async () =>
                {
                    bool apiSuccess = await _apiDataSource.UpdateNotePinned(id, pinned, now);
                    if (apiSuccess)
                        await _sqLiteDataSource.UpdateEntitySyncState(Notes[id].ToEntitySyncState(now));
                }).ConfigureAwait(false);
            return now;
        }

        public async Task<DateTime> UpdateNoteGloballyPinned(Guid id, bool pinned)
        {
            DateTime now = DateTime.UtcNow;
            bool sqliteSuccess = await _sqLiteDataSource.UpdateNoteGloballyPinned(id, pinned, now);
            if (sqliteSuccess)
                _ = Task.Run(async () =>
                {
                    bool apiSuccess = await _apiDataSource.UpdateNoteGloballyPinned(id, pinned, now);
                    if (apiSuccess)
                        await _sqLiteDataSource.UpdateEntitySyncState(Notes[id].ToEntitySyncState(now));
                }).ConfigureAwait(false);
            return now;
        }

        public async Task<AbstractNote> DeleteNote(AbstractNote note)
        {
            // Imporant answer on how to fire and forget https://stackoverflow.com/a/53184241
            bool sqliteSuccess = await _sqLiteDataSource.DeleteNote(note);
            if (sqliteSuccess)
                _ = Task.Run(async () =>
                {
                    bool apiSuccess = await _apiDataSource.DeleteNote(note);
                    if (apiSuccess)
                        await _sqLiteDataSource.DeleteEntitySyncState(note.Id, false);
                    else await _sqLiteDataSource.UpdateEntitySyncState(note.ToEntitySyncState(note.DateModifiedHeader, true), false);
                }).ConfigureAwait(false);
            return note;
        }

        public Task<List<Topic>> GetTopics() => _sqLiteDataSource.GetTopics();
        public Task<List<Topic>> GetTrashedTopics() => _sqLiteDataSource.GetTrashedTopics();
        public Topic GetTopic(Guid id) => _sqLiteDataSource.GetTopic(id);
        public async Task<Topic> InsertOrUpdateTopic(Topic topic, DataUpdateAction dua)
        {
            bool sqliteSuccess = await _sqLiteDataSource.InsertOrUpdateTopic(topic, dua);
            if (sqliteSuccess)
                _ = Task.Run(async () =>
                {
                    bool apiSuccess = await _apiDataSource.InsertOrUpdateTopic(topic, dua);
                    DateTime? dateSynced = apiSuccess ? new DateTime?(topic.DateModifiedHeader) : null;
                    await _sqLiteDataSource.InsertEntitySyncState(topic.ToEntitySyncState(dateSynced));
                }).ConfigureAwait(false);
            return topic;
        }
        public async Task<List<Topic>> InsertTopics(List<Topic> topics)
        {
            if (topics == null || topics.Count == 0)
                return topics;
            bool sqliteSuccess = await _sqLiteDataSource.InsertTopics(topics);
            if (sqliteSuccess)
                _ = Task.Run(async () =>
                {
                    bool apiSuccess = await _apiDataSource.InsertTopics(topics);
                    DateTime? dateSynced = apiSuccess ? new DateTime?(topics[0].DateCreated) : null;
                    var tasks = topics.ConvertAll(n => _sqLiteDataSource.InsertEntitySyncState(n.ToEntitySyncState(dateSynced)));
                    await Task.WhenAll(tasks);
                }).ConfigureAwait(false);
            return topics;
        }
        public async Task<DateTime> UpdateTopicPositions(TopicPositionData topicPositionData)
        {
            bool sqliteSuccess = await _sqLiteDataSource.UpdateTopicPositionData(topicPositionData);
            if (sqliteSuccess)
                _ = Task.Run(async () =>
                {
                    bool apiSuccess = await _apiDataSource.UpdateTopicPositionData(topicPositionData);
                    if (apiSuccess) {
                        var tasks = topicPositionData.TopicIds.ConvertAll(id => 
                            _sqLiteDataSource.UpdateEntitySyncState(Topics[id].ToEntitySyncState(topicPositionData.TimeStamp)));
                        await Task.WhenAll(tasks);
                    }
                }).ConfigureAwait(false);
            return topicPositionData.TimeStamp;
        }
        public async Task<Topic> DeleteTopic(Topic topic)
        {
            // Imporant answer on how to fire and forget https://stackoverflow.com/a/53184241
            bool sqliteSuccess = await _sqLiteDataSource.DeleteTopic(topic);
            if (sqliteSuccess)
                _ = Task.Run(async () =>
                {
                    bool apiSuccess = await _apiDataSource.DeleteTopic(topic);
                    if (apiSuccess)
                        await _sqLiteDataSource.DeleteEntitySyncState(topic.Id, true);
                    else await _sqLiteDataSource.UpdateEntitySyncState(Topics[topic.Id].ToEntitySyncState(topic.DateModifiedHeader, true), false);
                }).ConfigureAwait(false);
            return topic;
        }

        public async Task<bool> TrashUntrasEntity(Entity entity, DateTime? date)
        {
            bool sqliteSuccess = await _sqLiteDataSource.TrashUntrashEntity(entity, date);
            if (sqliteSuccess)
                _ = Task.Run(async () =>
                {
                    bool apiSuccess = await _apiDataSource.TrashUntrashEntity(entity, date);
                    if (apiSuccess)
                        await _sqLiteDataSource.UpdateEntitySyncState(entity.ToEntitySyncState(date));
                }).ConfigureAwait(false);
            return sqliteSuccess;
        }
    }
}