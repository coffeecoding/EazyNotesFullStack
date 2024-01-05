
using EazyNotes.Models.POCO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EazyNotesDesktop.Library.Transport
{
    public class ImportResult
    {
        public List<Topic> ImportedTopics { get; set; }
        public Dictionary<string, List<AbstractNote>> ImportedNotesByTopicTitle { get; set; }

        public ImportResult()
        {
            ImportedTopics = new List<Topic>();
            ImportedNotesByTopicTitle = new Dictionary<string, List<AbstractNote>>();
        }

        public void AddTopic(Topic topic) => ImportedTopics.Add(topic);

        public bool HasTopic(string topicTitle) => ImportedTopics.Any(t => t.Title == topicTitle);

        public void AddNote(string topicTitle, AbstractNote note)
        {
            if (ImportedNotesByTopicTitle.TryGetValue(topicTitle, out List<AbstractNote> list))
                list.Add(note);
            else ImportedNotesByTopicTitle.Add(topicTitle, new List<AbstractNote> { note });
        }

        public void AddNotes(string topicTitle, List<AbstractNote> notes)
        {
            if (ImportedNotesByTopicTitle.TryGetValue(topicTitle, out List<AbstractNote> list))
                list.AddRange(notes);
            else ImportedNotesByTopicTitle.Add(topicTitle, notes);
        }

        public void MergeWith(ImportResult other)
        {
            ImportedTopics.AddRange(other.ImportedTopics);
            // Remove potential duplicate topics, title-wise
            int i = ImportedTopics.Count-1;
            while (i >= 0)
            {
                var all = ImportedTopics.Where(t => t.Title == ImportedTopics[i].Title);
                if (all != null && all.Count() > 1)
                    ImportedTopics.RemoveAt(i);
                i--;
            }
            other.ImportedNotesByTopicTitle.ToList().ForEach(kvp => AddNotes(kvp.Key, kvp.Value));
        }

        public void RemoveEmptyTopics()
        {
            ImportedTopics.RemoveAll(t => ImportedNotesByTopicTitle.ContainsKey(t.Title) == false);
        }
    }
}
