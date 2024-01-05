using System;

namespace EazyNotes.Models.POCO
{
    public abstract class Entity
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Title { get; set; }
        public string Symbol { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModifiedHeader { get; set; }
        public DateTime DateModified { get; set; }
        public DateTime? DateDeleted { get; set; }
        public string IVKey { get; set; }

        public bool IsPersistedLocally() => Id != Guid.Empty;

        /// <summary>
        /// Returns true if the encrypted parts of the entities are equal, that is the Title and Content.
        /// </summary>
        public virtual bool BodyEquals(Entity other) => Id == other.Id && Title == other.Title;

        /// <summary>
        /// Returns true if the unencrypted parts of the entities' bodies are equal.
        /// </summary>
        public virtual bool BodyMetadataEquals(Entity other) => Id == other.Id;

        public abstract EntitySyncState ToEntitySyncState(DateTime? date, bool setDeleteFlag = false);
    }
}
