using System;
using System.Collections.Generic;
using System.Text;

namespace EazyNotes.Models.DTO
{
    public class DbSyncInfo
    {
        public List<TopicDTO> UpdatedTopics { get; set; }
        public List<NoteDTO> UpdatedNotes { get; set; }
        public List<long> DeletedTopics { get; set; }
        public List<long> DeletedNotes { get; set; }

        public DbSyncInfo()
        {
            UpdatedTopics = new List<TopicDTO>();
            UpdatedNotes = new List<NoteDTO>();
            DeletedTopics = new List<long>();
            DeletedNotes = new List<long>();
        }
    }
}
