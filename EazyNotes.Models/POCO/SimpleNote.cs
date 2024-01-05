using EazyNotes.Models.DTO;
using System;

namespace EazyNotes.Models.POCO
{
    public class SimpleNote : AbstractNote, ICloneable
    {
        private static string SYMBOL_NOTE = "\uE160";

        public string Content { get; set; }

        public SimpleNote()
        {
            Symbol = SYMBOL_NOTE;
        }

        public SimpleNote(Guid userId, Guid topicId, string title = "", string content = "")
        {
            Symbol = SYMBOL_NOTE;
            UserId = userId;
            TopicId = topicId;
            Title = title;
            Content = content;
            NoteType = 0;
            Options = "";
            DateTime now = DateTime.UtcNow;
            DateCreated = now;
            DateModified = now;
        }

        public SimpleNote(Guid id, Guid userId, Guid topicId, string title, string content, Int16 noteType, bool pinned, bool globallyPinned, 
            string options, DateTime dateCreated, DateTime dateModifiedHeader, DateTime dateModified, DateTime? dateDeleted, string ivkey)
        {
            Symbol = SYMBOL_NOTE;
            Id = id;
            UserId = userId;
            TopicId = topicId;
            Title = title;
            Content = content;
            NoteType = noteType;
            Pinned = pinned;
            GloballyPinned = globallyPinned;
            Options = options;
            DateCreated = dateCreated;
            DateModifiedHeader = dateModifiedHeader;
            DateModified = dateModified;
            DateDeleted = dateDeleted;
            IVKey = ivkey;
        }

        public override object Clone()
        {
            return new SimpleNote(Id, UserId, TopicId, Title, Content, NoteType, Pinned, GloballyPinned, Options, DateCreated, DateModifiedHeader, DateModified, DateDeleted, IVKey);
        }

        public override string GetPreviewString()
        {
            string full = $"{Title}: {Content}";
            string preview = full.Length > 50 ? full[0..47] + "..." : full;
            return preview;
        }

        public override string GetSerializedContent()
        {
            return Content;
        }

        public override void RemoveKeyAndDataFields()
        {
            base.RemoveKeyAndDataFields();
            Content = "";
        }

        public override NoteDTO ToNoteDTO()
        {
            return new NoteDTO(Id, UserId, TopicId, Title, Content, NoteType, Pinned, GloballyPinned, Options, DateCreated, DateModifiedHeader, DateModified, DateDeleted, IVKey);
        }

        public override void UpdateFrom(AbstractNote other)
        {
            base.UpdateFrom(other);
            SimpleNote note = other as SimpleNote;
            Content = note.Content;
            Title = note.Title;
        }

        /// <summary>
        /// Returns true if the encrypted parts of the notes are equal, that is the Title and Content.
        /// </summary>
        public override bool BodyEquals(AbstractNote other)
        {
            if (!(other is SimpleNote sn) || other == null)
                return false;
            return base.BodyEquals(sn) && Content == sn.Content;
        }

        /// <summary>
        /// Returns true if BodyEquals && BodyMetadataEquals both return true.
        /// </summary>
        public override bool Equals(object obj)
        {
            Entity entity = obj as Entity;
            return BodyEquals(entity) && BodyMetadataEquals(entity);
        }
    }
}
