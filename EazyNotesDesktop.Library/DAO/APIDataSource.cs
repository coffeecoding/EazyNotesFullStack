using EazyNotes.Common;
using EazyNotesDesktop.Library.Helpers;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using EazyNotes.Models.POCO;
using System.Net;
using System;
using Microsoft.AspNetCore.SignalR.Client;
using EazyNotesDesktop.Library.Common;
using EazyNotes.Models.DTO;
using EazyNotes.Common.JsonConverters;
using Newtonsoft.Json;

namespace EazyNotesDesktop.Library.DAO
{
    public class APIDataSource : IDataSource
    {
        private HubConnection DataHubConnection;

        public APIDataSource(ENClient enClient, ICryptoRoutines cryptoRoutines)
            : base(enClient, cryptoRoutines)
        {

        }

        public event EventHandler<DataChangedEventArgs> DataChanged;

        public override void Reset()
        {
            _enClient.Logout();
        }

        public override async Task<bool> CheckConnection()
        {
            using HttpResponseMessage response = await _enClient.ApiClient.GetAsync("monitor/connection");
            return response.IsSuccessStatusCode;
        }

        private async Task ConfigureWebsockets()
        {
            await Task.Run(() =>
            {
                JsonSerializerOptions opts = new JsonSerializerOptions()
                {
                    PropertyNameCaseInsensitive = true,
                };
                DataHubConnection = _enClient.ApiClient.DataHubConnection;
                CustomDateTimeNullableConverter jsonNullDateConverter = new CustomDateTimeNullableConverter();

                DataHubConnection.On<Guid, bool>("AcceptEntityDelete", (id, isTopic) => {
                    EntityDTO metadata;
                    if (isTopic)
                        metadata = new TopicHeaderDTO(id);
                    else metadata = new NoteHeaderDTO(id, Guid.Empty);
                    AcceptSyncAction(SyncAction.Delete, metadata);
                });
                DataHubConnection.On<Guid, bool, string>("AcceptEntityTrashed", (id, isTopic, dateString) => {
                    DateTime? date = jsonNullDateConverter.ConvertBack(dateString);
                    EntityDTO metadata;
                    if (isTopic)
                        metadata = new TopicHeaderDTO(id, dateDeleted: date);
                    else
                    {
                        var hasNote = Notes.TryGetValue(id, out AbstractNote note);
                        if (!hasNote) return;
                        metadata = new NoteHeaderDTO(id, topicId: note.TopicId, dateDeleted: date);
                    }
                    AcceptSyncAction(SyncAction.ToggleTrash, metadata);
                });

                DataHubConnection.On<string>("AcceptSyncNoteInsert", (noteJson) => {
                    NoteDTO noteDTO = System.Text.Json.JsonSerializer.Deserialize<NoteDTO>(noteJson, opts);
                    AbstractNote note = _cryptoRoutines.DecryptNote(noteDTO);
                    AcceptSyncAction(SyncAction.Create, note);
                });
                DataHubConnection.On<string>("AcceptSyncNoteUpdateBody", (noteJson) => {
                    NoteDTO noteDTO = System.Text.Json.JsonSerializer.Deserialize<NoteDTO>(noteJson, opts);
                    AbstractNote note = _cryptoRoutines.DecryptNote(noteDTO);
                    Notes[noteDTO.Id] = note;
                    AcceptSyncAction(SyncAction.Update, note);
                });
                DataHubConnection.On<string>("AcceptSyncNoteUpdateBodyMetadata", (noteJson) => {
                    NoteHeaderDTO noteMeta = System.Text.Json.JsonSerializer.Deserialize<NoteHeaderDTO>(noteJson, opts);
                    AbstractNote note = Notes[noteMeta.Id];
                    note.UpdateFrom(noteMeta);
                    AcceptSyncAction(SyncAction.Update, note);
                });
                DataHubConnection.On<Guid, Guid>("AcceptSyncNoteTopic", (noteId, newTopicId) => {
                    AcceptSyncAction(SyncAction.UpdateNoteTopic, new NoteHeaderDTO(noteId, topicId: newTopicId));
                });
                DataHubConnection.On<Guid, bool>("AcceptSyncNotePinned", (noteId, newPinnedValue) => {
                    var hasNote = Notes.TryGetValue(noteId, out AbstractNote note);
                    if (!hasNote) return;
                    AcceptSyncAction(SyncAction.UpdateNotePinned, new NoteHeaderDTO(noteId, topicId: note.TopicId, pinned: newPinnedValue));
                });
                DataHubConnection.On<Guid, string>("AcceptSyncNoteOptions", (noteId, newOptions) => {
                    var hasNote = Notes.TryGetValue(noteId, out AbstractNote note);
                    if (!hasNote) return;
                    AcceptSyncAction(SyncAction.UpdateNoteOptions, new NoteHeaderDTO(noteId, topicId: note.TopicId, options: newOptions));
                });

                DataHubConnection.On<string>("AcceptSyncTopicInsert", (topicJson) => {
                    TopicDTO topicDTO = System.Text.Json.JsonSerializer.Deserialize<TopicDTO>(topicJson, opts);
                    Topic topic = _cryptoRoutines.DecryptTopic(topicDTO);
                    AcceptSyncAction(SyncAction.Create, topic);
                });
                DataHubConnection.On<string>("AcceptSyncTopicUpdateBody", (topicJson) => {
                    TopicDTO topicDTO = System.Text.Json.JsonSerializer.Deserialize<TopicDTO>(topicJson, opts);
                    Topic topic = _cryptoRoutines.DecryptTopic(topicDTO);
                    Topics[topicDTO.Id] = topic;
                    AcceptSyncAction(SyncAction.Update, topic);
                });
                DataHubConnection.On<string>("AcceptSyncTopicUpdateBodyMetadata", (topicJson) => {
                    TopicHeaderDTO topicHeaderDTO = System.Text.Json.JsonSerializer.Deserialize<TopicHeaderDTO>(topicJson, opts);
                    Topic topic = Topics[topicHeaderDTO.Id];
                    topic.UpdateHeaderFrom(topicHeaderDTO);
                    AcceptSyncAction(SyncAction.Update, topic);
                });
                DataHubConnection.On<Guid, string>("AcceptSyncTopicSymbol", (topicId, newSymbol) => {
                    AcceptSyncAction(SyncAction.UpdateTopicSymbol, new TopicHeaderDTO(topicId, symbol: newSymbol));
                });
                DataHubConnection.On<Guid, string>("AcceptSyncTopicColor", (topicId, newColor) => {
                    AcceptSyncAction(SyncAction.UpdateTopicColor, new TopicHeaderDTO(topicId, color: newColor));
                });
                DataHubConnection.On<Guid, int>("AcceptSyncTopicColor", (topicId, newPosition) => {
                    AcceptSyncAction(SyncAction.UpdateTopicPosition, new TopicHeaderDTO(topicId, position: newPosition));
                });

                try
                {
                    DataHubConnection.StartAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Failed to start websocket connection: {e}");
                }
            });
        }

        private void AcceptSyncAction(SyncAction syncAction, object changeData)
        {
            try
            {
                if (changeData is AbstractNote updatedNote)
                {
                    switch (syncAction)
                    {
                        case SyncAction.Create:
                            Notes[updatedNote.Id] = updatedNote;
                            break;
                        case SyncAction.Update:
                            Notes[updatedNote.Id].UpdateFrom(updatedNote);
                            break;
                        default: throw new NotSupportedException();
                    }
                }
                else if (changeData is NoteHeaderDTO updatedNoteMetadata)
                {
                    switch (syncAction)
                    {
                        case SyncAction.Delete:
                            Notes.Remove(updatedNoteMetadata.Id);
                            break;
                        default:
                            Notes[updatedNoteMetadata.Id].UpdateMetadataFrom(updatedNoteMetadata);
                            break;
                    }
                }
                else if (changeData is Topic updatedTopic)
                {
                    switch (syncAction)
                    {
                        case SyncAction.Create:
                            Topics[updatedTopic.Id] = updatedTopic;
                            break;
                        case SyncAction.Update:
                            Topics[updatedTopic.Id].UpdateFrom(updatedTopic);
                            break;
                        default: throw new NotSupportedException();
                    }
                }
                else if (changeData is TopicHeaderDTO updatedTopicMetadata)
                {
                    switch (syncAction)
                    {
                        case SyncAction.Delete:
                            Topics.Remove(updatedTopicMetadata.Id);
                            break;
                        default:
                            Topics[updatedTopicMetadata.Id].UpdateHeaderFrom(updatedTopicMetadata);
                            break;
                    }
                }
                // Finally, let subscribers know that the data in our repository has changed, so e.g. the UI can respond
                DataChanged?.Invoke(this, new DataChangedEventArgs(syncAction, changeData));
            }
            catch (Exception e)
            {
                // TODO
                // We are probably too much out of sync ==> Resync Fully with API/Database
            }
        }

        private async Task TriggerSyncEntityTrashed(Guid id, bool isTopic, DateTime? date)
        {
            if (DataHubConnection == null || DataHubConnection.State != HubConnectionState.Connected)
                return;
            CustomDateTimeNullableConverter jsonNullDateConverter = new CustomDateTimeNullableConverter();
            string dateString = jsonNullDateConverter.Convert(date);
            await DataHubConnection.InvokeAsync("SyncEntityTrashed", id, isTopic, dateString);
        }

        private async Task TriggerSyncEntityDelete(Guid id, bool isTopic)
        {
            if (DataHubConnection == null || DataHubConnection.State != HubConnectionState.Connected)
                return;
            await DataHubConnection.InvokeAsync("SyncEntityDelete", id, isTopic);
        }

        private async Task TriggerSyncNote(string noteDTO, DataUpdateAction dua)
        {
            if (DataHubConnection == null || DataHubConnection.State != HubConnectionState.Connected)
                return;
            string suffix = dua.ToString().Split('.').Last();
            await DataHubConnection.InvokeAsync($"SyncNote{suffix}", noteDTO);
        }

        private async Task TriggerSyncNoteTopic(Guid noteId, Guid newTopicId)
        {
            if (DataHubConnection == null || DataHubConnection.State != HubConnectionState.Connected)
                return;
            await DataHubConnection.InvokeAsync("SyncNoteTopic", noteId, newTopicId);
        }

        private async Task TriggerSyncNotePinned(Guid noteId, bool newPinnedValue)
        {
            if (DataHubConnection == null || DataHubConnection.State != HubConnectionState.Connected)
                return;
            await DataHubConnection.InvokeAsync("SyncNotePinned", noteId, newPinnedValue);
        }

        private async Task TriggerSyncNoteOptions(Guid noteId, string newOptions)
        {
            if (DataHubConnection == null || DataHubConnection.State != HubConnectionState.Connected)
                return;
            await DataHubConnection.InvokeAsync("SyncNoteOptions", noteId, newOptions);
        }

        private async Task TriggerSyncTopic(string topicDTO, DataUpdateAction dua)
        {
            if (DataHubConnection == null || DataHubConnection.State != HubConnectionState.Connected)
                return;
            string suffix = dua.ToString().Split('.').Last();
            await DataHubConnection.InvokeAsync($"SyncTopic{suffix}", topicDTO);
        }

        private async Task TriggerSyncTopicMetadata(string topicDTO, TopicHeaderDTO t)
        {
            if (DataHubConnection == null || DataHubConnection.State != HubConnectionState.Connected)
                return;
            string json = JsonConvert.SerializeObject(t);
            await DataHubConnection.InvokeAsync("SyncTopicMetadata", json);
        }

        private async Task TriggerSyncTopicSymbol(Guid topicId, string newSymbol)
        {
            if (DataHubConnection == null || DataHubConnection.State != HubConnectionState.Connected)
                return;
            await DataHubConnection.InvokeAsync("SyncTopicSymbol", topicId, newSymbol);
        }

        private async Task TriggerSyncTopicColor(Guid topicId, string newColor)
        {
            if (DataHubConnection == null || DataHubConnection.State != HubConnectionState.Connected)
                return;
            await DataHubConnection.InvokeAsync("SyncTopicColor", topicId, newColor);
        }

        private async Task TriggerSyncTopicPosition(Guid topicId, int newPosition)
        {
            if (DataHubConnection == null || DataHubConnection.State != HubConnectionState.Connected)
                return;
            await DataHubConnection.InvokeAsync("SyncTopicPosition", topicId, newPosition);
        }

        public override async Task<HttpStatusCode> UpdateUser(User user)
        {
            UserProfile userProfile = user.GetUserProfile();
            string jsonData = JsonConvert.SerializeObject(userProfile);
            var body = new StringContent(jsonData, Encoding.UTF8, "application/json");

            using HttpResponseMessage response = await _enClient.ApiClient.PutAsync($"api/user/{user.Id}", body);
            if (response.IsSuccessStatusCode)
            {
                return response.StatusCode;
            }
            else
            {
                throw new HttpRequestException(response.ReasonPhrase);
            }
        }

        #region Data Access
        public override async Task<List<AbstractNote>> GetNotes()
        {
            using HttpResponseMessage response =
                await _enClient.ApiClient.GetAsync($"/api/notes/");

            if (response.IsSuccessStatusCode)
            {
                IEnumerable<NoteDTO> encryptedNotes =
                    await response.Content.ReadAsAsync<IEnumerable<NoteDTO>>();
                List<AbstractNote> notes = encryptedNotes.ToList()
                    .ConvertAll(n => _cryptoRoutines.DecryptNote(n));
                Notes.Where(n => n.Value.DateDeleted == null).ToList().ForEach(n => Notes.Remove(n.Key));
                notes.ForEach(n => Notes[n.Id] = n);
                return notes;
            }
            else
            {
                throw new HttpRequestException(response.ReasonPhrase);
            }
        }

        public override async Task<List<AbstractNote>> GetTrashedNotes()
        {
            using HttpResponseMessage response =
                await _enClient.ApiClient.GetAsync($"/api/notes/trashed");

            if (response.IsSuccessStatusCode)
            {
                IEnumerable<NoteDTO> encryptedNotes =
                    await response.Content.ReadAsAsync<IEnumerable<NoteDTO>>();
                List<AbstractNote> trashedNotes = encryptedNotes.ToList()
                    .ConvertAll(n => _cryptoRoutines.DecryptNote(n));
                Notes.Where(n => n.Value.DateDeleted != null).ToList().ForEach(n => Notes.Remove(n.Key));
                trashedNotes.ForEach(n => Notes[n.Id] = n);
                return trashedNotes;
            }
            else
            {
                throw new HttpRequestException(response.ReasonPhrase);
            }
        }

        /// <summary>
        /// Returns null if the associated topic was deleted or trashed.
        /// </summary>
        public override async Task<List<AbstractNote>> GetUpdatedNotesByTopic(ClientStateOfNotesInTopic clientNoteInfo)
        {
            string jsonData = JsonConvert.SerializeObject(clientNoteInfo);
            var body = new StringContent(jsonData, Encoding.UTF8, "application/json");

            Guid topicId = clientNoteInfo.TopicMetadata.Id;

            using HttpResponseMessage response = await _enClient.ApiClient.PostAsync($"/api/notes/topicId={topicId}", body);
            if (response.IsSuccessStatusCode)
            {
                APIStateOfNotesInTopic serverState = await response.Content.ReadAsAsync<APIStateOfNotesInTopic>();

                if (serverState.TopicDeleted)
                {
                    // Topic is non-existent in database, hence it and all contained notes must be totally removed!
                    Notes.Where(kvPair => kvPair.Value.TopicId == topicId).ToList().ForEach(kvPair => Notes.Remove(kvPair.Key));
                    Topics.Where(kvPair => kvPair.Key == topicId).ToList().ForEach(kvPair => Topics.Remove(kvPair.Key));
                    return null;
                }
                else if (serverState.Topic != null)
                {
                    if (serverState.Topic.DateDeleted != null)
                        return null;
                    Topic clientTopic = Topics[topicId];
                    Topic decryptedTopic = _cryptoRoutines.DecryptTopic(serverState.Topic);
                    clientTopic.UpdateFrom(decryptedTopic);
                }

                List<AbstractNote> updatedNotes = new List<AbstractNote>();

                foreach (NoteHeaderDTO deletedNote in serverState.DeletedNotes)
                {
                    AbstractNote clientNote = Notes.SingleOrDefault(n => n.Key == deletedNote.Id).Value;
                    if (clientNote != null)
                        Notes.Remove(clientNote.Id);
                }

                foreach (NoteDTO updatedNote in serverState.UpdatedNotes)
                {
                    AbstractNote decryptedNote = _cryptoRoutines.DecryptNote(updatedNote);
                    AbstractNote clientNote = Notes.SingleOrDefault(n => n.Key == updatedNote.Id).Value;
                    if (clientNote == null)
                        Notes.Add(updatedNote.Id, decryptedNote);
                    else
                        clientNote.UpdateFrom(decryptedNote);

                    updatedNotes.Add(decryptedNote);
                }

                return updatedNotes;
            }
            else
            {
                throw new HttpRequestException(response.ReasonPhrase);
            }
        }

        public override async Task<bool> InsertOrUpdateNote(AbstractNote note, DataUpdateAction dua)
        {
            HttpResponseMessage response = null;
            EntityDTO updatedEntity;
            string jsonData = null;

            if (dua == DataUpdateAction.UpdateHeader)
            {
                updatedEntity = note.GetHeader();
                jsonData = JsonConvert.SerializeObject(updatedEntity as NoteHeaderDTO);
                var body = new StringContent(jsonData, Encoding.UTF8, "application/json");
                response = await _enClient.ApiClient.PutAsync("api/note/meta", body);
            }
            else
            {
                updatedEntity = _cryptoRoutines.EncryptNote(note);
                NoteDTO updatedNote = updatedEntity as NoteDTO;
                jsonData = JsonConvert.SerializeObject(updatedNote);
                var body = new StringContent(jsonData, Encoding.UTF8, "application/json");
                if (dua == DataUpdateAction.Insert)
                    response = await _enClient.ApiClient.PostAsync("api/note", body);
                else if (dua == DataUpdateAction.UpdateBody)
                    response = await _enClient.ApiClient.PutAsync("api/note", body);
            }

            if (response.IsSuccessStatusCode)
            {
                var id = await response.Content.ReadAsAsync<Guid>();
                note.Id = updatedEntity.Id = id;

                // Trigger Sync Action
                if (dua == DataUpdateAction.Insert)
                    // Only in case of an insert does the object have a new Id, thus needing to be reserialized!
                    jsonData = JsonConvert.SerializeObject(updatedEntity as NoteDTO);
                _ = TriggerSyncNote(jsonData, dua);

                // Update local repository
                Notes.TryGetValue(note.Id, out AbstractNote existing);
                if (existing != null)
                    existing.UpdateFrom(note);
                else
                    Notes[note.Id] = note;

                response.Dispose();

                return true;
            }
            else
            {
                throw new HttpRequestException(response.ReasonPhrase);
            }
        }

        public override async Task<bool> InsertNotes(List<AbstractNote> notes)
        {
            List<NoteDTO> encryptedNotes = notes.ConvertAll(n => _cryptoRoutines.EncryptNote(n));
            string jsonData = JsonConvert.SerializeObject(encryptedNotes);
            var body = new StringContent(jsonData, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _enClient.ApiClient.PostAsync("api/notes", body);

            if (response.IsSuccessStatusCode)
            {
                List<Guid> noteIds = await response.Content.ReadAsAsync<List<Guid>>();
                // assume that the returned notes are in the same order as the sent notes => just copy the id fields
                for (int i = 0; i < noteIds.Count; i++)
                    notes[i].Id = noteIds[i];
                notes.ForEach(n => Notes[n.Id] = n);
                return true;
            }
            else throw new HttpRequestException(response.ReasonPhrase);
        }

        public override async Task<bool> UpdateNoteTopic(Guid id, Guid newTopicId, DateTime timestamp)
        {
            using HttpResponseMessage response = await _enClient.ApiClient.PutAsync($"api/note/{id}/topicId={newTopicId}", new StringContent(timestamp.ToISO8601UTCString()));

            if (response.IsSuccessStatusCode)
            {
                DateTime dateModifiedHeader = await response.Content.ReadAsAsync<DateTime>();

                // Trigger Sync Action
                _ = TriggerSyncNoteTopic(id, newTopicId);

                // Update local repo
                Notes[id].TopicId = newTopicId;
                Notes[id].DateModifiedHeader = dateModifiedHeader;

                return true;
            }
            else
            {
                throw new HttpRequestException(response.ReasonPhrase);
            }
        }

        public override async Task<bool> UpdateNotePinned(Guid id, bool pinned, DateTime timestamp)
        {
            int val = pinned ? 1 : 0;

            using HttpResponseMessage response = await _enClient.ApiClient.PutAsync($"api/note/{id}/pin={val}", new StringContent(timestamp.ToISO8601UTCString()));

            if (response.IsSuccessStatusCode)
            {
                DateTime dateModifiedHeader = await response.Content.ReadAsAsync<DateTime>();

                // Trigger Sync Action
                _ = TriggerSyncNotePinned(id, pinned);

                // Update local repo
                Notes[id].Pinned = pinned;
                Notes[id].DateModifiedHeader = dateModifiedHeader;

                return true;
            }
            else
            {
                throw new HttpRequestException(response.ReasonPhrase);
            }
        }

        public override async Task<bool> UpdateNoteGloballyPinned(Guid id, bool newValue, DateTime timestamp)
        {
            int val = newValue ? 1 : 0;

            using HttpResponseMessage response = await _enClient.ApiClient.PutAsync($"api/note/{id}/globalpin={val}", new StringContent(timestamp.ToISO8601UTCString()));

            if (response.IsSuccessStatusCode)
            {
                DateTime dateModifiedHeader = await response.Content.ReadAsAsync<DateTime>();

                // Trigger Sync Action
                _ = TriggerSyncNotePinned(id, newValue);

                // Update local repo
                Notes[id].GloballyPinned = newValue;
                Notes[id].DateModifiedHeader = dateModifiedHeader;

                return true;
            }
            else
            {
                throw new HttpRequestException(response.ReasonPhrase);
            }
        }

        public override async Task<bool> DeleteNote(AbstractNote note)
        {
            using HttpResponseMessage response = await _enClient.ApiClient
                .DeleteAsync($"api/note/delete/{note.Id}");
            if (response.IsSuccessStatusCode)
            {
                // TODO: Reconsider what to do -- if anything -- with result here, or if reading it is even necessary.
                // Idea: Validation of some sort
                Guid id = await response.Content.ReadAsAsync<Guid>();

                // Trigger sync action
                _ = TriggerSyncEntityDelete(note.Id, false);

                // Update local repo
                Notes.Remove(note.Id);

                return true;
            }
            else
            {
                throw new HttpRequestException(response.ReasonPhrase);
            }
        }

        public override async Task<List<Topic>> GetTopics()
        {
            using HttpResponseMessage response =
                await _enClient.ApiClient.GetAsync($"/api/topics/");

            if (response.IsSuccessStatusCode)
            {
                IEnumerable<TopicDTO> encryptedTopics =
                    await response.Content.ReadAsAsync<IEnumerable<TopicDTO>>();
                List<Topic> decryptedTopics = encryptedTopics.ToList()
                    .ConvertAll(t => _cryptoRoutines.DecryptTopic(t));
                Topics.Clear();
                decryptedTopics.ForEach(t => Topics[t.Id] = t);
                return decryptedTopics;
            }
            else
            {
                throw new HttpRequestException(response.ReasonPhrase);
            }
        }

        /// <summary>
        /// Deprecated. GetTopics() now gets all topics, both trashed and not.
        /// </summary>
        public override async Task<List<Topic>> GetTrashedTopics()
        {
            using HttpResponseMessage response =
                await _enClient.ApiClient.GetAsync($"/api/topics/trashed");

            if (response.IsSuccessStatusCode)
            {
                IEnumerable<TopicDTO> encryptedTopics =
                    await response.Content.ReadAsAsync<IEnumerable<TopicDTO>>();
                List<Topic> trashedTopics = encryptedTopics.ToList()
                    .ConvertAll(t => _cryptoRoutines.DecryptTopic(t));
                trashedTopics.ForEach(t => Topics[t.Id] = t);
                return trashedTopics;
            }
            else
            {
                throw new HttpRequestException(response.ReasonPhrase);
            }
        }

        public override async Task<bool> InsertOrUpdateTopic(Topic topic, DataUpdateAction dua)
        {
            HttpResponseMessage response = null;
            EntityDTO updatedEntity;
            string jsonData;

            if (dua == DataUpdateAction.UpdateHeader)
            {
                updatedEntity = topic.GetHeader();
                jsonData = JsonConvert.SerializeObject(updatedEntity as TopicHeaderDTO);
                var body = new StringContent(jsonData, Encoding.UTF8, "application/json");
                response = await _enClient.ApiClient.PutAsync("api/topic/meta", body);
            }
            else
            {
                updatedEntity = _cryptoRoutines.EncryptTopic(topic);
                TopicDTO updatedTopic = updatedEntity as TopicDTO;
                jsonData = JsonConvert.SerializeObject(updatedTopic);
                var body = new StringContent(jsonData, Encoding.UTF8, "application/json");
                if (dua == DataUpdateAction.Insert)
                    response = await _enClient.ApiClient.PostAsync("api/topic", body);
                else if (dua == DataUpdateAction.UpdateBody)
                    response = await _enClient.ApiClient.PutAsync("api/topic", body);
            }

            if (response.IsSuccessStatusCode)
            {
                Guid id = await response.Content.ReadAsAsync<Guid>();
                topic.Id = updatedEntity.Id = id;

                // Trigger sync
                if (dua == DataUpdateAction.Insert)
                    // Only in case of an insert does the object have a new Id, thus it needs to be reserialized!
                    jsonData = JsonConvert.SerializeObject(updatedEntity as TopicDTO);
                _ = TriggerSyncTopic(jsonData, dua);

                // Update local repo
                Topics.TryGetValue(topic.Id, out Topic existing);
                if (existing == null)
                    Topics[topic.Id] = topic;
                else Topics[topic.Id].UpdateFrom(topic);

                response.Dispose();
                return true;
            }
            else
            {
                throw new HttpRequestException(response.ReasonPhrase);
            }
        }

        public override async Task<bool> InsertTopics(List<Topic> topics)
        {
            List<TopicDTO> encryptedTopics = topics.ConvertAll(t => _cryptoRoutines.EncryptTopic(t));
            string jsonData = JsonConvert.SerializeObject(encryptedTopics);
            var body = new StringContent(jsonData, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _enClient.ApiClient.PostAsync("api/topics", body);

            if (response.IsSuccessStatusCode)
            {
                List<Guid> topicIds = await response.Content.ReadAsAsync<List<Guid>>();
                // assume that the returned topic ids are in the same order as the sent ones 
                for (int i = 0; i < topicIds.Count; i++)
                    topics[i].Id = topicIds[i];
                topics.ForEach(t => Topics[t.Id] = t);
                return true;
            }
            else
            {
                throw new HttpRequestException(response.ReasonPhrase);
            }
        }

        public override async Task<bool> UpdateTopicPositionData(TopicPositionData topicPositionData)
        {
            string json = JsonConvert.SerializeObject(topicPositionData);
            var body = new StringContent(json, Encoding.UTF8, "application/json");
            using HttpResponseMessage response = await _enClient.ApiClient.PutAsync($"api/topics/positions", body);
            if (response.IsSuccessStatusCode)
            {
                DateTime dateModifiedHeader = await response.Content.ReadAsAsync<DateTime>();

                // TODO: Trigger Sync

                // Update local repo
                for (int i = 0; i < topicPositionData.TopicIds.Count; i++)
                {
                    Topics[topicPositionData.TopicIds[i]].Position = topicPositionData.TopicPositions[i];
                    Topics[topicPositionData.TopicIds[i]].DateModifiedHeader = dateModifiedHeader;
                }

                return true;
            }
            else
            {
                throw new HttpRequestException(response.ReasonPhrase);
            }
        }

        public override async Task<bool> DeleteTopic(Topic topic)
        {
            using HttpResponseMessage response = await _enClient.ApiClient
                .DeleteAsync($"api/topic/delete/{topic.Id}");
            if (response.IsSuccessStatusCode)
            {
                var id = await response.Content.ReadAsAsync<long>();

                // Trigger Sync
                _ = TriggerSyncEntityDelete(topic.Id, true);

                // Update local repo
                Topics.Remove(topic.Id);

                return true;
            }
            else
            {
                throw new HttpRequestException(response.ReasonPhrase);
            }
        }

        public override async Task<bool> TrashUntrashEntity(Entity entity, DateTime? date)
        {
            string route = "api/";
            bool isTopic = entity is Topic;
            if (isTopic)
                route += $"topic/trash/";
            else
                route += $"note/trash/";

            string urldate = WebUtility.UrlEncode(date.ToISO8601UTCString());

            route += $"{entity.Id}/{urldate}";

            using HttpResponseMessage response = await _enClient.ApiClient.PutAsync(route, new StringContent(""));
            if (response.IsSuccessStatusCode)
            {
                DateTime dateModifiedHeader = await response.Content.ReadAsAsync<DateTime>();

                // Update local repo
                if (isTopic)
                {
                    Topics[entity.Id].DateDeleted = date;
                    Topics[entity.Id].DateModifiedHeader = dateModifiedHeader;
                }
                else
                {
                    Notes[entity.Id].DateDeleted = date;
                    Notes[entity.Id].DateModifiedHeader = dateModifiedHeader;
                }

                // Trigger Sync
                _ = TriggerSyncEntityTrashed(entity.Id, isTopic, date);

                return true;
            }
            else
            {
                throw new HttpRequestException(response.ReasonPhrase);
            }
        }

        /// <summary>
        /// TODO: Implement
        /// </summary>
        public override AbstractNote GetNote(Guid id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// TODO: Implement
        /// </summary>
        public override Topic GetTopic(Guid id)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}