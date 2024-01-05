using Dapper;
using Dapper.Contrib.Extensions;
using EazyNotes.Models.DTO;
using EazyNotesDesktop.Library.Helpers;
using EazyNotes.Models.POCO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using EazyNotes.Common;
using EazyNotesDesktop.Library.Common;
using Newtonsoft.Json;

namespace EazyNotesDesktop.Library.DAO
{
    public class SQLiteUtils
    {
        public static string CreateStringNoteValues(NoteDTO n)
            => $"'{n.Id}', '{n.UserId}', '{n.TopicId}', '{n.Title}', '{n.Content}', {n.NoteType}, {n.Pinned}, {n.GloballyPinned}, '{n.Options}', '{n.DateCreated.ToISO8601UTCString()}', '{n.DateModifiedHeader.ToISO8601UTCString()}', '{n.DateModified.ToISO8601UTCString()}', {n.DateDeleted.ToISO8601UTCSQLValueString()}, '{n.IVKey}'";

        public static string CreateStringTopicValues(TopicDTO t)
            => $"'{t.Id}', '{t.UserId}', '{t.Title}', '{t.Symbol}', '{t.DateCreated.ToISO8601UTCString()}', '{t.DateModifiedHeader.ToISO8601UTCString()}', '{t.DateModified.ToISO8601UTCString()}', {t.DateDeleted.ToISO8601UTCSQLValueString()}, '{t.IVKey}', '{t.Color}', {t.Position}";

        public static string CreateQueryUpdateNote(NoteDTO n)
            => $"UPDATE {SQLiteDataSource.TBL_NOTES} SET TopicId = '{n.TopicId}', Title = '{n.Title}', Content = '{n.Content}', Pinned = {n.Pinned}, GloballyPinned = {n.GloballyPinned}, Options = '{n.Options}', DateCreated = '{n.DateCreated.ToISO8601UTCString()}', DateModifiedHeader = '{n.DateModifiedHeader.ToISO8601UTCString()}', DateModified = '{n.DateModified.ToISO8601UTCString()}', DateDeleted = {n.DateDeleted.ToISO8601UTCSQLValueString()}, IVKey = '{n.IVKey}' WHERE {SQLiteDataSource.NOTE_ID_COL} = '{n.Id}'";

        public static string CreateQueryUpdateTopic(TopicDTO n)
            => $"UPDATE {SQLiteDataSource.TBL_NOTES} SET Title = '{n.Title}', Symbol = '{n.Symbol}', Color = '{n.Color}', Position = {n.Position}, DateCreated = '{n.DateCreated.ToISO8601UTCString()}', DateModifiedHeader = '{n.DateModifiedHeader.ToISO8601UTCString()}', DateModified = '{n.DateModified.ToISO8601UTCString()}', DateDeleted = {n.DateDeleted.ToISO8601UTCSQLValueString()}, IVKey = '{n.IVKey}' WHERE {SQLiteDataSource.TOPIC_ID_COL} = '{n.Id}'";

        public static string CreateQueryUpdateNoteHeader(NoteHeaderDTO n)
            => $"UPDATE {SQLiteDataSource.TBL_NOTES} SET TopicId = '{n.TopicId}', Pinned = {n.Pinned}, GloballyPinned = {n.GloballyPinned}, Options = '{n.Options}', DateCreated = '{n.DateCreated.ToISO8601UTCString()}', DateModifiedHeader = '{n.DateModifiedHeader.ToISO8601UTCString()}', DateModified = '{n.DateModified.ToISO8601UTCString()}', DateDeleted = {n.DateDeleted.ToISO8601UTCSQLValueString()} WHERE {SQLiteDataSource.NOTE_ID_COL} = '{n.Id}'";

        public static string CreateQueryUpdateTopicHeader(TopicHeaderDTO n)
            => $"UPDATE {SQLiteDataSource.TBL_NOTES} SET Symbol = '{n.Symbol}', Color = '{n.Color}', Position = {n.Position}, DateCreated = '{n.DateCreated.ToISO8601UTCString()}', DateModifiedHeader = '{n.DateModifiedHeader.ToISO8601UTCString()}', DateModified = '{n.DateModified.ToISO8601UTCString()}', DateDeleted = {n.DateDeleted.ToISO8601UTCSQLValueString()} WHERE {SQLiteDataSource.TOPIC_ID_COL} = '{n.Id}'";

        public static string CreateStringEntityValues(EntitySyncState e)
            => $"'{e.Id}', {e.IsTopic}, {e.DateSynced.ToISO8601UTCSQLValueString()}, {e.Delete}";

        public static string CreateQueryUpdateEntitySyncState(EntitySyncState e, bool updateDate = true)
            => $"UPDATE {SQLiteDataSource.TBL_ENTITY} SET {(updateDate ? ($"DateSynced = {e.DateSynced.ToISO8601UTCSQLValueString()}, ") : "")}Delete = {e.Delete} WHERE {SQLiteDataSource.ENTITY_ID_COL} = '{e.Id}' AND IsTopic={(e.IsTopic ? '1' : '0')}";
    }

    public class SQLiteDataSource : IDataSource
    {
        public const string TBL_NOTES = "Notes";
        public const string TBL_TOPICS = "Topics";
        public const string TBL_ENTITY = "EntitySyncState";
        public const string NOTE_ID_COL = "NoteId";
        public const string TOPIC_ID_COL = "TopicId";
        public const string ENTITY_ID_COL = "EntityId";
        const string NOTE_COLUMNS = "NoteId, UserId, TopicId, Title, Content, NoteType, Pinned, GloballyPinned, Options, DateCreated, DateModifiedHeader, DateModified, DateDeleted, IVKey";
        const string NOTE_COLUMNS_MAPPED = "NoteId as Id, UserId, TopicId, Title, Content, NoteType, Pinned, GloballyPinned, Options, DateCreated, DateModifiedHeader, DateModified, DateDeleted, IVKey";
        const string TOPIC_COLUMNS = "TopicId, UserId, Title, Symbol, DateCreated, DateModifiedHeader, DateModified, DateDeleted, IVKey, Color, Position";
        const string TOPIC_COLUMNS_MAPPED = "TopicId as Id, UserId, Title, Symbol, DateCreated, DateModifiedHeader, DateModified, DateDeleted, IVKey, Color, Position";
        const string ENTITY_COLS_MAPPED = "EntityId as Id, IsTopic, DateSynced, Delete";

        private string ConnectionString;
        private IDbConnection _cn;
        private IDbConnection Conn => _cn ??= new SQLiteConnection(ConnectionString);

        public SQLiteDataSource(ENClient client, ICryptoRoutines cryptoRoutines)
            : base(client, cryptoRoutines)
        {
            SqlMapper.RemoveTypeMap(typeof(DateTime));
            SqlMapper.AddTypeHandler(typeof(DateTime), new DateTimeHandler());
            SqlMapper.RemoveTypeMap(typeof(Guid));
            SqlMapper.AddTypeHandler(typeof(Guid), new GuidHandler());
        }

        public override async Task<bool> CheckConnection()
        {
            // TODO: ONLY CHECK IF FILE EXISTS; OTHERWISE IT CREATES AN EMPTY DATABASE EVEN IF WE JUST TRY TO READ FROM IT
            return await Task.Run(() => true);
        }

        public static bool InsertOrUpdateUser(UserDTO userDTO)
        {
            string cnString = $@"Data Source={ENClient.GetUserDBPath(userDTO.Username)};Version=3;";
            IDbConnection cn = new SQLiteConnection(cnString);
            var existing = (Dictionary<string, string>)cn.QuerySingleOrDefault("SELECT Value FROM Meta WHERE Key = 'User'");
            int affected;
            if (existing == null || existing.TryGetValue("User", out string val) == false)
                affected = cn.Execute($"INSERT INTO Meta (Key, Value) VALUES ('User', '{JsonConvert.SerializeObject(userDTO)}')");
            else affected = cn.Execute($"UPDATE Meta SET Value = '{JsonConvert.SerializeObject(userDTO)}' WHERE Key = 'User'");
            return affected == 1;
        }

        public static UserDTO GetUser(string username)
        {
            string cnString = $@"Data Source={ENClient.GetUserDBPath(username)};Version=3;";
            IDbConnection cn = new SQLiteConnection(cnString);
            var existing = cn.QuerySingleOrDefault<string>("SELECT Value FROM Meta WHERE Key = 'User'");
            if (existing == null)
                return null;
            UserDTO userDTO = JsonConvert.DeserializeObject<UserDTO>(existing);
            return userDTO;
        }

        /// <summary>
        /// Sets the connection string using the username of the currently logged in user as well as the "_Diff" suffix if this class is meant to 
        /// access the local diff-database for any subsequent calls, rather than the usual one. E.g. "myusername.db" vs "myusername_diff.db".
        /// </summary>
        /// <param name="ofDiffDb">Indicates whether the connection string to be set should contain the "_Diff" suffix to refer to the diff db.</param>
        public void SetConnectionStringAndOpenConnection(string username)
        {
            ConnectionString = $@"Data Source={ENClient.GetUserDBPath(username)};Version=3;";
            _cn = new SQLiteConnection(ConnectionString);
        }

        private void UpdateSchema(int newversion, string updatequery, StringBuilder log)
        {
            string metaTableName = "Meta";
            log.AppendLine("Checking sqlite db version ...");
            var result = Conn.Query("SELECT Value FROM Meta WHERE Key = 'Version'");
            string oldversion = result.ToList()[0].Value;
            log.AppendLine($"Found {oldversion}");
            if (newversion > int.Parse(oldversion))
            {
                log.AppendLine("Updating schema ...");
                Conn.Query(updatequery);
                log.AppendLine("Updating version info ...");
                Conn.Query($"UPDATE {metaTableName} SET Value = '{newversion}' WHERE Key = 'Version';");
            }
        }

        /// <summary>
        /// Potentially not needed anywhere.
        /// </summary>
        public Entity GetEntity<T>(Guid id, Type type) where T : Entity
        {
            if (type == typeof(Topic))
                return GetTopic(id);
            return GetNote(id);
        }

        #region Sync related CRUD
        public async Task<bool> InsertEntitySyncState(EntitySyncState e)
        {
            var a = await Conn.ExecuteAsync($"INSERT INTO {TBL_ENTITY} ({ENTITY_COLS_MAPPED}) VALUES ({SQLiteUtils.CreateStringEntityValues(e)};");
            return a == 1;
        }

        public async Task<bool> UpdateEntitySyncState(EntitySyncState e, bool updateDate = true)
        {
            var affected = await Conn.ExecuteAsync(SQLiteUtils.CreateQueryUpdateEntitySyncState(e, updateDate));
            return affected == 1;
        }

        public async Task<bool> DeleteEntitySyncState(Guid id, bool isTopic)
        {
            var affected = await Conn.ExecuteAsync($"DELETE FROM {TBL_ENTITY} WHERE EntityId='{id}' AND IsTopic={(isTopic ? 1 : 0)};");
            return affected == 1;
        }
        #endregion

        /// <summary>
        /// Gets the notes except trashed ones.
        /// </summary>
        public override async Task<List<AbstractNote>> GetNotes()
        {
            var output = await Conn.QueryAsync<NoteDTO>($"SELECT {NOTE_COLUMNS_MAPPED} FROM {TBL_NOTES} WHERE DateDeleted IS NULL OR DateDeleted = ''");
            List<AbstractNote> notes = output.ToList().ConvertAll(n => _cryptoRoutines.DecryptNote(n));
            Notes.Where(n => n.Value.DateDeleted == null).ToList().ForEach(n => Notes.Remove(n.Key));
            notes.ForEach(n => Notes[n.Id] = n);
            return notes;
        }

        public override async Task<List<AbstractNote>> GetTrashedNotes()
        {
            var output = await Conn.QueryAsync<NoteDTO>($"SELECT {NOTE_COLUMNS_MAPPED} FROM {TBL_NOTES} WHERE DateDeleted IS NOT NULL");
            List<AbstractNote> notes = output.ToList().ConvertAll(n => _cryptoRoutines.DecryptNote(n));
            Notes.Where(n => n.Value.DateDeleted != null).ToList().ForEach(n => Notes.Remove(n.Key));
            notes.ForEach(n => Notes.Add(n.Id, n));
            return notes;
        }

        public override AbstractNote GetNote(Guid id)
        {
            NoteDTO noteDTO = Conn.QuerySingleOrDefault<NoteDTO>($"SELECT {NOTE_COLUMNS_MAPPED} FROM {TBL_NOTES} WHERE {NOTE_ID_COL} = '{id}'");
            AbstractNote note = _cryptoRoutines.DecryptNote(noteDTO);
            Notes[note.Id] = note;
            return note;
        }

        public override async Task<List<AbstractNote>> GetUpdatedNotesByTopic(ClientStateOfNotesInTopic clientNotes)
        {
            // TODO: Reconsider this, but:
            // It seems this method is not needed for the local sqlite client
            // as there should only ever be 1 client reading from / writing to it.
            // However since it's part of the Data Layer API, it has to be implemented. 
            // TODO: 
            // 1) Write SQLite CRUD method to get notes by topic, just like spNotesGetByTopicId on server side
            // 2) Use 1) and take implementation from NoteController where metadata is compared,
            // and only the different notes are actually updated.
            // ----
            // Temporary, HIGHLY suboptimal and EXTREMELY inefficient implementation: Just get all again from DB and decrypt
            List<AbstractNote> notes = await GetNotes();
            return notes.Where(n => n.TopicId == clientNotes.TopicMetadata.Id && n.DateDeleted == null).ToList();
        }

        public override async Task<bool> InsertOrUpdateNote(AbstractNote note, DataUpdateAction dua)
        {
            if (note.IsPersistedLocally())
                return await UpdateNote(note);
            return await InsertNote(note);
        }

        public async Task<bool> InsertNote(AbstractNote note)
        {
            NoteDTO e = _cryptoRoutines.EncryptNote(note);
            e.Id = Guid.NewGuid();
            //lock(Conn)
            await Conn.ExecuteAsync($"INSERT INTO {TBL_NOTES}({NOTE_COLUMNS}) VALUES ({SQLiteUtils.CreateStringNoteValues(e)});");
            note.Id = e.Id;
            Notes[e.Id] = note;
            return true;
        }

        private async Task<bool> UpdateNote(AbstractNote note)
        {
            NoteDTO encrypted = _cryptoRoutines.EncryptNote(note);
            await Conn.ExecuteAsync(SQLiteUtils.CreateQueryUpdateNote(encrypted));
            Notes[note.Id] = note;
            return true;
        }

        public override async Task<bool> InsertNotes(List<AbstractNote> notes)
        {
            List<Task<bool>> tasks = notes.ConvertAll(n => InsertNote(n));
            bool[] re = await Task.WhenAll(tasks);
            return new List<bool>(re).All(r => r);
        }

        public override async Task<bool> UpdateNoteTopic(Guid id, Guid newTopicId, DateTime u)
        {
            DateTime now = DateTime.UtcNow;
            string sql = $"UPDATE {TBL_NOTES} SET TopicId = {newTopicId}, DateModifiedHeader = '{now.ToISO8601UTCString()}' WHERE {NOTE_ID_COL} = '{id}';";
            await Conn.QueryAsync(sql);
            Notes[id].TopicId = newTopicId;
            Notes[id].DateModifiedHeader = now;
            return true;
        }

        public override async Task<bool> UpdateNotePinned(Guid id, bool pinned, DateTime timestamp)
        {
            int pinnedInt = pinned ? 1 : 0;
            string sql = $"UPDATE {TBL_NOTES} SET Pinned = {pinnedInt}, DateModifiedHeader = '{timestamp.ToISO8601UTCString()}' WHERE {NOTE_ID_COL} = '{id}';";
            await Conn.QueryAsync(sql);
            Notes[id].Pinned = pinned;
            Notes[id].DateModifiedHeader = timestamp;
            return true;
        }

        public override async Task<bool> UpdateNoteGloballyPinned(Guid id, bool newValue, DateTime timestamp)
        {
            int pinnedInt = newValue ? 1 : 0;
            string sql = $"UPDATE {TBL_NOTES} SET GloballyPinned = {pinnedInt}, DateModifiedHeader = '{timestamp.ToISO8601UTCString()}' WHERE {NOTE_ID_COL} = '{id}';";
            await Conn.QueryAsync(sql);
            Notes[id].GloballyPinned = newValue;
            Notes[id].DateModifiedHeader = timestamp;
            return true;
        }

        public override async Task<bool> DeleteNote(AbstractNote note)
        {
            NoteDTO encrypted = _cryptoRoutines.EncryptNote(note);
            int success = await Conn.ExecuteAsync($"DELETE FROM {TBL_NOTES} WHERE {NOTE_ID_COL} = '{encrypted.Id}'");
            if (success == 1)
                Notes.Remove(note.Id);
            return true;
        }

        public override async Task<List<Topic>> GetTopics()
        {
            string sql = $"SELECT {TOPIC_COLUMNS_MAPPED} FROM {TBL_TOPICS} WHERE DateDeleted IS NULL OR DateDeleted = ''";
            var asd = await Conn.QueryAsync(sql);
            var output = await Conn.QueryAsync<TopicDTO>(sql);
            List<Topic> topics = output.ToList().ConvertAll(t => _cryptoRoutines.DecryptTopic(t));
            Topics.Where(n => n.Value.DateDeleted == null).ToList().ForEach(n => Notes.Remove(n.Key));
            topics.ForEach(t => Topics[t.Id] = t);
            return topics;
        }

        public override async Task<List<Topic>> GetTrashedTopics()
        {
            var output = await Conn.QueryAsync<TopicDTO>($"SELECT {TOPIC_COLUMNS_MAPPED} FROM {TBL_TOPICS} WHERE DateDeleted IS NOT NULL", new DynamicParameters());
            List<Topic> topics = output.ToList().ConvertAll(t => _cryptoRoutines.DecryptTopic(t));
            Topics.Where(n => n.Value.DateDeleted != null).ToList().ForEach(n => Notes.Remove(n.Key));
            topics.ForEach(t => Topics[t.Id] = t);
            return topics;
        }

        public override Topic GetTopic(Guid id)
        {
            TopicDTO topicDTO = Conn.QuerySingleOrDefault<TopicDTO>($"SELECT {TOPIC_COLUMNS_MAPPED} FROM {TBL_TOPICS} WHERE Id = '{id}'", new DynamicParameters());
            Topic topic = _cryptoRoutines.DecryptTopic(topicDTO);
            Topics[id] = topic;
            return topic;
        }

        public override async Task<bool> InsertOrUpdateTopic(Topic topic, DataUpdateAction dua)
        {
            if (topic.IsPersistedLocally())
                return await UpdateTopic(topic);
            return await InsertTopic(topic);
        }

        public async Task<bool> InsertTopic(Topic topic)
        {
            TopicDTO encrypted = _cryptoRoutines.EncryptTopic(topic);
            encrypted.Id = Guid.NewGuid();
            string sql = $"INSERT INTO {TBL_TOPICS} ({TOPIC_COLUMNS}) VALUES({SQLiteUtils.CreateStringTopicValues(encrypted)});";
            await Conn.ExecuteAsync(sql);
            topic.Id = encrypted.Id;
            Topics[encrypted.Id] = topic;
            return true;
        }

        private async Task<bool> UpdateTopic(Topic topic)
        {
            TopicDTO encrypted = _cryptoRoutines.EncryptTopic(topic);
            await Conn.ExecuteAsync(SQLiteUtils.CreateQueryUpdateTopic(encrypted));
            Topics[topic.Id] = topic;
            return true;
        }

        public override async Task<bool> InsertTopics(List<Topic> topics)
        {
            List<Task<bool>> tasks = topics.ConvertAll(async t => await InsertTopic(t));
            bool[] re = await Task.WhenAll(tasks);
            return new List<bool>(re).All(r => r);
        }

        public override async Task<bool> UpdateTopicPositionData(TopicPositionData tpd)
        {
            for (int i = 0; i < tpd.TopicIds.Count; i++)
            {
                await Conn.ExecuteAsync($"UPDATE {TBL_TOPICS} SET Position = '{tpd.TopicPositions[i]}', DateModifiedHeader = '{tpd.TimeStamp.ToISO8601UTCString()}' WHERE {TOPIC_ID_COL} = '{tpd.TopicIds[i]}'");
                Topics[tpd.TopicIds[i]].Position = tpd.TopicPositions[i];
                Topics[tpd.TopicIds[i]].DateModifiedHeader = tpd.TimeStamp;
            }
            return true;
        }

        public override async Task<bool> DeleteTopic(Topic topic)
        {
            int affected = await Conn.ExecuteAsync($"DELETE FROM {TBL_TOPICS} WHERE {TOPIC_ID_COL} = '{topic.Id}'");
            if (affected == 1)
                Topics.Remove(topic.Id);
            return true;
        }

        public override async Task<HttpStatusCode> UpdateUser(User user)
        {
            await Task.Run(() => InsertOrUpdateUser(user.ToUserDTO()));
            return HttpStatusCode.OK;
        }

        public override async Task<bool> TrashUntrashEntity(Entity entity, DateTime? date)
        {
            string sql = "UPDATE ";
            string idCol = "";
            if (entity is AbstractNote)
            {
                sql += $"{TBL_NOTES} ";
                idCol = NOTE_ID_COL;
            }
            else if (entity is Topic)
            {
                sql += $"{TBL_TOPICS} ";
                idCol = TOPIC_ID_COL;
            }
            DateTime dateModifiedHeader = date ?? DateTime.UtcNow;
            sql += $"SET DateDeleted = {date.ToISO8601UTCSQLValueString()}, DateModifiedHeader = '{dateModifiedHeader.ToISO8601UTCString()}' WHERE {idCol} = '{entity.Id}';";
            await Conn.QueryAsync(sql);
            if (entity is Topic)
            {
                Topics[entity.Id].DateDeleted = date;
                Topics[entity.Id].DateModifiedHeader = dateModifiedHeader;
            }
            else
            {
                Notes[entity.Id].DateDeleted = date;
                Notes[entity.Id].DateModifiedHeader = dateModifiedHeader;
            }
            return true;
        }
    }
}