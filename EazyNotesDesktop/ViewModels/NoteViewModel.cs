using EazyNotes.Models.POCO;
using EazyNotesDesktop.Library.DAO;
using EazyNotesDesktop.Library.Common;
using System;

namespace EazyNotesDesktop.ViewModels
{
    public class NoteViewModel : BaseNoteViewModel
    {
        public NoteViewModel(DataAccess dataAccess, MainViewModel mainVM, TopicViewModel topicVM, Guid id)
            : base(dataAccess, mainVM, topicVM, id)
        {
            UpdateStateFromRepo();
        }

        public NoteViewModel(DataAccess dataAccess, MainViewModel mainVM, TopicViewModel topicVM, SimpleNote note)
            : base(dataAccess, mainVM, topicVM, note.Id)
        {
            UpdateStateWithObject(note);
        }

        protected override AbstractNote CreateChangedNote()
        {
            SimpleNote changedNote = (SimpleNote)Note.Clone();
            changedNote.Title = Title;
            changedNote.Content = Content;
            changedNote.Options = Options;
            return changedNote;
        }

        public override void Discard()
        {
            if (State == EntityState.Modified)
            {
                Title = Note.Title;
                Content = ((SimpleNote)Note).Content;
                State = EntityState.Default;
            }
            else if (State == EntityState.New)
            {
                base.Discard();
            }
        }

        public override void UpdateStateWithObject(AbstractNote note)
        {
            WrapText = note.HasFlag(NoteOptions.WRAP_TEXT);
            Content = (note as SimpleNote).Content;
            base.UpdateStateWithObject(note);
        }

        public override void UpdateStateFromRepo()
        {
            WrapText = Note.HasFlag(NoteOptions.WRAP_TEXT);
            Content = Note == null ? "" : (Note as SimpleNote).Content;
            base.UpdateStateFromRepo();
        }

        public override void RaiseNoteChangedEvents()
        {
            base.RaiseNoteChangedEvents();
        }

        public override bool ContainsSearchKey(string uppercaseKey)
        {
            return (Title?.ToUpper().Contains(uppercaseKey)) == true
                || (Content?.ToUpper().Contains(uppercaseKey)) == true;
        }

        private string _content;
        public string Content
        {
            get { return _content; }
            set 
            { 
                SetProperty(ref _content, value);
                RaisePropertyChanged(PreviewText);
                if (State == EntityState.Default)
                {
                    State = EntityState.Modified;
                }
            }
        }

        private bool _wrapText;
        public bool WrapText
        {
            get { return _wrapText; }
            set { 
                SetProperty(ref _wrapText, value);
                if (value)
                    NoteOptions.Add(ref Options, NoteOptions.WRAP_TEXT);
                else NoteOptions.Remove(ref Options, NoteOptions.WRAP_TEXT);
                if (State == EntityState.Default)
                {
                    State = EntityState.Modified;
                }
            }
        }

        public override string PreviewText
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Content))
                    return "";
                string previewContent = Content.Replace("\r\n", "");
                previewContent = previewContent.Replace("\n", "");
                bool contentLongerThanCutoff = previewContent.Length > 50;
                return contentLongerThanCutoff
                    ? previewContent.Substring(0, 50) + "..."
                    : previewContent;
            }
        }

        private double _verticalOffset;
        public double VerticalOffset
        {
            get { return _verticalOffset; }
            set { SetProperty(ref _verticalOffset, value); }
        }
    }
}
