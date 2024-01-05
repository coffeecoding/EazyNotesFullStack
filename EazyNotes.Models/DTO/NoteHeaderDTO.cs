using EazyNotes.Common;
using System;
using System.Diagnostics.CodeAnalysis;

namespace EazyNotes.Models.DTO
{
    public class NoteHeaderDTO : EntityDTO, IEquatable<NoteHeaderDTO>
    {
        public Guid TopicId { get; set; }
        public bool Pinned { get; set; }
        public bool GloballyPinned { get; set; }
        public string Options { get; set; }

        public NoteHeaderDTO()
        {

        }

        public NoteHeaderDTO(Guid id, Guid topicId, bool pinned = false, bool globallyPinned = false, string options = "", DateTime? dateDeleted = null)
        {
            Id = id;
            TopicId = topicId;
            Pinned = pinned;
            GloballyPinned = globallyPinned;
            Options = options;
            DateDeleted = dateDeleted;
        }

        public NoteHeaderDTO(Guid id, Guid topicId, bool pinned, bool globallyPinned,
            string options, DateTime dateCreated, DateTime dateModifiedHeader, DateTime dateLastEdited, DateTime? dateDeleted) {
            Id = id;
            TopicId = topicId;
            Pinned = pinned;
            GloballyPinned = globallyPinned;
            Options = options;
            DateCreated = dateCreated;
            DateModifiedHeader = dateModifiedHeader;
            DateModified = dateLastEdited;
            DateDeleted = dateDeleted;
        }

        public NoteHeaderDTO(Guid id, DateTime? dateDeleted)
        {
            Id = id;
            DateDeleted = dateDeleted;
        }

        /// <summary>
        /// Checks if the metadata of the notes is equal, that is, Id, TopicId, Pinned, Options and DateDeleted.
        /// </summary>
        public virtual bool EqualsHeader([AllowNull] NoteHeaderDTO other)
        {
            if (other == null)
                return false;
            bool c0 = base.EqualsHeader(other);
            bool c1 = Pinned == other.Pinned && GloballyPinned == other.GloballyPinned && Options == other.Options && TopicId == other.TopicId;
            return c0 && c1;
        }

        /// <summary>
        /// Checks if the contents of the notes are equal, that is, Id and DateLastEdited.
        /// </summary>
        public bool EqualsBody([AllowNull] NoteHeaderDTO other)
        {
            if (other == null)
                return false;
            return Id == other.Id && DateModified == other.DateModified;
        }

        public bool Equals([AllowNull] NoteHeaderDTO other)
        {
            if (other == null)
                return false;
            return EqualsBody(other) && EqualsHeader(other);
        }
    }
}
