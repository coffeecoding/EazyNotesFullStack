using EazyNotes.Models.DTO;
using System;

namespace EazyNotes.Models.POCO
{
    public enum NoteType
    {
        SimpleNote = 0,
        CheckList = 1,
    }

    public abstract class AbstractNote : Entity, ICloneable
    {
        public Guid TopicId { get; set; }
        public short NoteType { get; set; }
        public bool Pinned { get; set; }
        public bool GloballyPinned { get; set; }
        public string Options { get; set; }

        public NoteType GetNoteType()
        {
            return (NoteType)NoteType;
        }

        public virtual object Clone()
        {
            throw new NotImplementedException();
        }

        public virtual bool BodyEquals(AbstractNote other)
        {
            return base.BodyEquals(other) && NoteType == other.NoteType && Options == other.Options;
        }

        public bool HasFlag(string flag) => Options != null && Options.Contains(flag);
        public bool HasFlag(char flag) => Options != null && Options.Contains(flag);

        public void UpdateMetadataFrom(NoteHeaderDTO other)
        {
            TopicId = other.TopicId;
            Pinned = other.Pinned;
            GloballyPinned = other.GloballyPinned;
            Options = other.Options;
            DateModified = other.DateModified;
            DateDeleted = other.DateDeleted;
        }

        public virtual void UpdateFrom(AbstractNote other)
        {
            TopicId = other.TopicId;
            Pinned = other.Pinned;
            GloballyPinned = other.GloballyPinned;
            Options = other.Options;
            DateModified = other.DateModified;
            DateModifiedHeader = other.DateModifiedHeader;
            DateDeleted = other.DateDeleted;
        }

        public virtual void UpdateFrom(NoteHeaderDTO other)
        {
            Options = other.Options;
            DateModified = other.DateModified;
        }

        public abstract string GetPreviewString();

        public abstract string GetSerializedContent();

        public virtual void RemoveKeyAndDataFields()
        {
            UserId = Guid.Empty;
            Title = IVKey = "";
            Id = TopicId = Guid.Empty;
        }

        public virtual NoteDTO ToNoteDTO()
        {
            return new NoteDTO(Id, UserId, TopicId, Title, null, NoteType, Pinned, GloballyPinned, Options, DateCreated, DateModifiedHeader, DateModified, DateDeleted, IVKey);
        }

        public NoteHeaderDTO GetHeader()
        {
            return new NoteHeaderDTO(Id, TopicId, Pinned, GloballyPinned, Options, DateCreated, DateModifiedHeader, DateModified, DateDeleted);
        }

        public override EntitySyncState ToEntitySyncState(DateTime? date, bool setDeleteFlag = false)
            => new EntitySyncState(Id, false, date, setDeleteFlag);
    }
}
