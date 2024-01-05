using Dapper.Contrib.Extensions;
using EazyNotes.Common;
using EazyNotes.Models.POCO;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace EazyNotes.Models.DTO
{
    [Table("Notes")]
    public class NoteDTO : NoteHeaderDTO, IEquatable<NoteDTO>
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public short NoteType { get; set; }
        public string IVKey { get; set; }

        public NoteDTO()
        {
            // empty ctor needed by dapper contrib
        }

        public NoteDTO(Guid id, Guid userId, Guid topicId, string title, string content, short noteType, bool pinned, 
            bool globallyPinned, string options, DateTime dateCreated, DateTime dateModifiedHeader, 
            DateTime dateModified, DateTime? dateDeleted, string ivkey)
            : base(id, topicId, pinned, globallyPinned, options, dateDeleted)
        {
            UserId = userId;
            TopicId = topicId;
            Title = title;
            Content = content;
            NoteType = noteType;
            Pinned = pinned;
            GloballyPinned = globallyPinned;
            DateCreated = dateCreated;
            DateModifiedHeader = dateModifiedHeader;
            DateModified = dateModified;
            IVKey = ivkey;
        }

        public override List<APIValidationError> IsValid()
        {
            if (Id == Guid.Empty)
                return new List<APIValidationError>() { new APIValidationError("Note", "Id", "may not be empty") };
            if (UserId == Guid.Empty)
                return new List<APIValidationError>() { new APIValidationError("Note", "UserId", "may not be empty") };
            if (TopicId == Guid.Empty)
                return new List<APIValidationError>() { new APIValidationError("Note", "TopicId", "may not be empty") };
            if (Title.Length > Constraints.NOTE_TITLE_MAXLEN)
                return new List<APIValidationError>() { new APIValidationError("Note", "Title", "is too long") };
            if (!Constraints.NOTE_ISVALIDTYPE((ushort)NoteType))
                return new List<APIValidationError>() { new APIValidationError("Note", "Type", "is invalid") };
            if (Options.Length > Constraints.NOTE_OPTIONS_MAX_LEN)
                return new List<APIValidationError>() { new APIValidationError("Note", "Options", "is too long") };
            if (IVKey.Length > Constraints.NOTE_IVKEY_MAXLEN)
                return new List<APIValidationError>() { new APIValidationError("Note", "IVKey", "is too long") };
            return null;
        }

        public override string ToString()
        {
            return "Note: " + Title;
        }

        public bool EqualsHeader(NoteHeaderDTO other)
        {
            return other.EqualsHeader(this);
        }

        public bool Equals([AllowNull] NoteDTO other)
        {
            return Id == other.Id && UserId == other.UserId && TopicId == other.TopicId && Title == other.Title && IVKey == other.IVKey &&
                Content == other.Content && NoteType == other.NoteType && Pinned == other.Pinned && GloballyPinned == other.GloballyPinned && Options == other.Options &&
                DateDeleted.IsEqual(other.DateDeleted) && DateCreated.IsEqual(other.DateCreated) &&
                DateModifiedHeader.IsEqual(other.DateModifiedHeader) && DateModified.IsEqual(other.DateModified);
        }

        public SimpleNote ToSimpleNote()
        {
            return new SimpleNote(Id, UserId, TopicId, Title, Content, NoteType, Pinned, GloballyPinned, Options, DateCreated, DateModifiedHeader, DateModified, DateDeleted, IVKey);
        }

        public CheckListNote ToCheckListNote(List<CheckListItem> items)
        {
            return new CheckListNote(this, items);
        }

        public object Clone()
        {
            return new NoteDTO(Id, UserId, TopicId, Title, Content, NoteType, Pinned, GloballyPinned, Options, DateCreated, DateModifiedHeader, DateModified, DateDeleted, IVKey);
        }
    }
}
