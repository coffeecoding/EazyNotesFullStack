using EazyNotesDesktop.Library.Helpers;
using EazyNotes.Models.POCO;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using EazyNotes.Models.DTO;

namespace EazyNotesDesktop.Library.DAO
{
    public abstract class IDataSource
    {
        protected ENClient _enClient;
        protected readonly ICryptoRoutines _cryptoRoutines;

        public Dictionary<Guid, AbstractNote> Notes { get; protected set; }
        public Dictionary<Guid, Topic> Topics { get; protected set; }

        public IDataSource(ENClient client, ICryptoRoutines cryptoRoutines)
        {
            _enClient = client;
            _cryptoRoutines = cryptoRoutines;
            Notes = new Dictionary<Guid, AbstractNote>();
            Topics = new Dictionary<Guid, Topic>();
        }

        public virtual void Reset() { }

        public abstract Task<bool> CheckConnection();

        public virtual Task RegisterClient() => _enClient.RegisterClientDevice();

        public abstract Task<HttpStatusCode> UpdateUser(User user);

        public abstract Task<List<AbstractNote>> GetNotes();
        public abstract Task<List<AbstractNote>> GetTrashedNotes();
        public abstract AbstractNote GetNote(Guid id);
        public abstract Task<List<AbstractNote>> GetUpdatedNotesByTopic(ClientStateOfNotesInTopic clientNotesInfo);
        public abstract Task<bool> InsertOrUpdateNote(AbstractNote note, DataUpdateAction dua);
        public abstract Task<bool> InsertNotes(List<AbstractNote> notes);
        public abstract Task<bool> UpdateNoteTopic(Guid id, Guid newTopicId, DateTime timestamp);
        public abstract Task<bool> UpdateNotePinned(Guid id, bool pinned, DateTime timestamp);
        public abstract Task<bool> UpdateNoteGloballyPinned(Guid id, bool globallyPinned, DateTime timestamp);
        public abstract Task<bool> DeleteNote(AbstractNote note);

        public abstract Task<List<Topic>> GetTopics();
        public abstract Task<List<Topic>> GetTrashedTopics();
        public abstract Topic GetTopic(Guid id);
        public abstract Task<bool> InsertOrUpdateTopic(Topic topic, DataUpdateAction dua);
        public abstract Task<bool> InsertTopics(List<Topic> topics);
        public abstract Task<bool> UpdateTopicPositionData(TopicPositionData topicPositionData);
        public abstract Task<bool> DeleteTopic(Topic topic);

        public abstract Task<bool> TrashUntrashEntity(Entity entity, DateTime? date);
    }
}