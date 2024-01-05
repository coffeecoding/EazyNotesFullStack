using System.Collections.Generic;

namespace EazyNotes.Models.DTO
{
    public class ClientStateOfNotesInTopic
    {
        public TopicHeaderDTO TopicMetadata { get; set; }
        public List<NoteHeaderDTO> NoteMetadatas { get; set; }

        public ClientStateOfNotesInTopic()
        {
            NoteMetadatas = new List<NoteHeaderDTO>();
        }

        public ClientStateOfNotesInTopic(TopicHeaderDTO topic, List<NoteHeaderDTO> noteMetadatas)
        {
            TopicMetadata = topic;
            NoteMetadatas = noteMetadatas;
        }
    }

    public class APIStateOfNotesInTopic
    {
        public bool TopicDeleted { get; set; }
        public TopicDTO Topic { get; set; }
        public List<NoteDTO> UpdatedNotes { get; set; }
        public List<NoteHeaderDTO> DeletedNotes{ get; set; }

        public APIStateOfNotesInTopic()
        {
            UpdatedNotes = new List<NoteDTO>();
            DeletedNotes = new List<NoteHeaderDTO>();
        }

        public APIStateOfNotesInTopic(TopicDTO topic = null)
        {
            Topic = topic;
            UpdatedNotes = new List<NoteDTO>();
            DeletedNotes = new List<NoteHeaderDTO>();
        }
    }
}
