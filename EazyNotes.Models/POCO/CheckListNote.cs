using System;
using System.Linq;
using System.Collections.Generic;
using EazyNotes.Models.DTO;
using System.Text;
using EazyNotes.Common;

namespace EazyNotes.Models.POCO
{
    public class CheckListNote : AbstractNote, ICloneable
    {
        private static string SYMBOL_CHECKLIST = "\uE133";

        public List<CheckListItem> CheckListItems { get; set; }

        public static readonly string ITEM_SEPARATOR = ">";

        public CheckListNote()
        {
            NoteType = 1;
        }

        public CheckListNote(NoteDTO decryptedNote, List<CheckListItem> checkListItems)
        {
            Symbol = SYMBOL_CHECKLIST;
            Id = decryptedNote.Id;
            UserId = decryptedNote.UserId;
            Title = decryptedNote.Title;
            TopicId = decryptedNote.TopicId;
            NoteType = 1;
            DateCreated = decryptedNote.DateCreated;
            DateModified = decryptedNote.DateModified;
            DateDeleted = decryptedNote.DateDeleted;
            GloballyPinned = decryptedNote.GloballyPinned;
            Pinned = decryptedNote.Pinned;
            Options = decryptedNote.Options;
            IVKey = decryptedNote.IVKey;
            CheckListItems = checkListItems;
        }

        public CheckListNote(AbstractNote decryptedNote, List<CheckListItem> checkListItems)
        {
            Symbol = SYMBOL_CHECKLIST;
            Id = decryptedNote.Id;
            UserId = decryptedNote.UserId;
            Title = decryptedNote.Title;
            TopicId = decryptedNote.TopicId;
            NoteType = 1; 
            DateCreated = decryptedNote.DateCreated;
            DateModified = decryptedNote.DateModified;
            DateDeleted = decryptedNote.DateDeleted;
            GloballyPinned = decryptedNote.GloballyPinned;
            Pinned = decryptedNote.Pinned;
            Options = decryptedNote.Options;
            IVKey = decryptedNote.IVKey;
            CheckListItems = checkListItems;
        }

        public CheckListNote(Guid userId, Guid topicId)
        {
            Symbol = SYMBOL_CHECKLIST;
            UserId = userId;
            TopicId = topicId;
            Title = "";
            Options = "";
            CheckListItems = new List<CheckListItem>();
            NoteType = 1;
            GloballyPinned = false;
            DateTime now = DateTime.UtcNow;
            DateCreated = now;
            DateModified = now;
            IVKey = null;
        }

        public CheckListNote(Guid userId, Guid topicId, string title, List<CheckListItem> items)
        {
            Symbol = SYMBOL_CHECKLIST;
            UserId = userId;
            TopicId = topicId;
            Title = title;
            CheckListItems = items;
            NoteType = 1;
            Options = "";
            GloballyPinned = false;
            DateTime now = DateTime.UtcNow;
            DateCreated = now;
            DateModified = now;
            IVKey = null;
        }

        public CheckListNote(Guid id, Guid userId, Guid topicId, string title, string content, Int16 noteType, bool pinned,
            bool globallyPinned, string options, DateTime dateCreated, DateTime dateModifiedHeader, 
            DateTime dateModified, DateTime? dateDeleted, string ivkey)
        {
            Symbol = SYMBOL_CHECKLIST;
            Id = id;
            UserId = userId;
            TopicId = topicId;
            Title = title;
            CheckListItems = CustomDeserializeItems(content);
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

        public override void UpdateFrom(AbstractNote other)
        {
            base.UpdateFrom(other);
            CheckListNote note = other as CheckListNote;
            Title = note.Title;
            CheckListItems = note.CheckListItems;
        }

        public static List<CheckListItem> CustomDeserializeItems(string items)
        {
            if (string.IsNullOrWhiteSpace(items))
                return new List<CheckListItem>();
            return items.Split(ITEM_SEPARATOR).ToList().ConvertAll(
                serialized => new CheckListItem(serialized));
        }

        public static string CustomSerializeItems(List<CheckListItem> items)
        {
            if (items.Count == 0)
                return "";
            return string.Join(ITEM_SEPARATOR, items.ConvertAll(i => i.CustomSerialize()));
        }

        /// <summary>
        /// Returns true if the encrypted parts of the notes are equal, that is the Title and Content.
        /// </summary>
        public override bool BodyEquals(AbstractNote other)
        {
            if (other == null || !(other is CheckListNote cln))
                return false;
            return base.BodyEquals(other) && CheckListItem.AreListsEqual(CheckListItems, cln.CheckListItems);
        }

        /// <summary>
        /// Returns true if the unencrypted parts of the notes are equal, that is the Id and the Options field.
        /// </summary>
        public override bool BodyMetadataEquals(Entity other)
        {
            if (other == null || !(other is CheckListNote cln))
                return false;
            return Options == cln.Options && base.BodyMetadataEquals(other);
        }

        /// <summary>
        /// Returns true if BodyEquals && BodyMetadataEquals both return true.
        /// </summary>
        public override bool Equals(object obj)
        {
            Entity entity = obj as Entity;
            return BodyEquals(entity) && BodyMetadataEquals(entity);
        }

        public override object Clone()
        {
            List<CheckListItem> ClonedCheckListItems = CheckListItems
                .ToList().ConvertAll(i => (CheckListItem)i.Clone());
            return new CheckListNote(this, ClonedCheckListItems);
        }

        public override string GetPreviewString()
        {
            string begin = $"{Title}: ";
            string previewitems = CheckListItems.Count > 0
                ? CheckListItems[0].Text != null
                    ? CheckListItems[0].Text.Length > 30
                        ? CheckListItems[0].Text[0..27] + "..."
                        : CheckListItems[0].Text
                    : ""
                : "";
            string more = CheckListItems.Count > 1 ? ", ..." : "";
            string fullpreview = $"{begin}{previewitems}{more}";
            string preview = fullpreview.Length > 50 ? $"{fullpreview[0..47]}..." : fullpreview;
            return preview;
        }

        public override string GetSerializedContent()
        {
            return string.Join(Environment.NewLine, CheckListItems.ConvertAll((item) => item.ToString()).ToArray());
        }

        public override void RemoveKeyAndDataFields()
        {
            base.RemoveKeyAndDataFields();
            CheckListItems.Clear();
        }
    }
}
