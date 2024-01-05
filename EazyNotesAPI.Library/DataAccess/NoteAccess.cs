using EazyNotes.Models.DTO;
using EazyNotesAPI.Library.Internal.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EazyNotesAPI.Library.DataAccess
{
    public class NoteAccess
    {
        private static string ConnectionStringName => SqlDataAccess.ConnectionStringName;

        public IEnumerable<NoteDTO> GetByUserId(Guid userId)
        {
            SqlDataAccess sql = new SqlDataAccess();
            var p = new { UserId = userId };
            var result = sql.LoadData<NoteDTO, dynamic>("spNotesGetUntrashedByUserId", p, ConnectionStringName);
            return result;
        }

        public NoteDTO GetById(Guid id)
        {
            SqlDataAccess sql = new SqlDataAccess();
            var p = new { Id = id };
            var result = sql.LoadData<NoteDTO, dynamic>("spNoteGetById", p, ConnectionStringName);
            return result.FirstOrDefault();
        }

        public IEnumerable<NoteDTO> GetNotesByTopicId(Guid topicId)
        {
            SqlDataAccess sql = new SqlDataAccess();
            var p = new { TopicId = topicId };
            var result = sql.LoadData<NoteDTO, dynamic>("spNotesGetByTopicId", p, ConnectionStringName);
            return result;
        }

        public IEnumerable<NoteDTO> GetTrashedByUserId(Guid userId)
        {
            SqlDataAccess sql = new SqlDataAccess();
            var p = new { UserId = userId };
            var result = sql.LoadData<NoteDTO, dynamic>("spNotesGetTrashedByUserId", p, ConnectionStringName);
            return result;
        }

        public Guid InsertOrUpdate(NoteDTO note)
        {
            SqlDataAccess sql = new SqlDataAccess();
            sql.SaveDataSingle<dynamic>("spNoteInsertOrUpdate", note, ConnectionStringName);
            return note.Id;
            //if (!note.IsPersistedGlobally())
            //{
            //    note.SyncedCreation = true;
            //    sql.SaveDataSingle<dynamic>("spNoteInsert", note, ConnectionStringName);
            //} else
            //    UpdateBody(note);
            //return note.Id;
        }

        public void UpdateBody(NoteDTO noteBody)
        {
            SqlDataAccess sql = new SqlDataAccess();
            var p = new
            {
                noteBody.Id,
                noteBody.Title,
                noteBody.Content,
                noteBody.Options,
                noteBody.DateModifiedHeader,
                noteBody.DateModified,
            };
            sql.SaveDataSingle<dynamic>("spNoteUpdateBody", p, ConnectionStringName);
        }

        public void UpdateHeader(NoteHeaderDTO noteHeaderDTO)
        {
            SqlDataAccess sql = new SqlDataAccess();

            var p = new
            {
                noteHeaderDTO.Id,
                noteHeaderDTO.TopicId,
                noteHeaderDTO.Options,
                noteHeaderDTO.Pinned,
                noteHeaderDTO.GloballyPinned,
                noteHeaderDTO.DateModifiedHeader,
                noteHeaderDTO.DateDeleted,
            };
            sql.SaveDataSingle<dynamic>("spNoteUpdateHeader", p, ConnectionStringName);
        }

        public void UpdateNoteTopic(Guid id, Guid topicId, DateTime dateModifiedHeader)
        {
            SqlDataAccess sql = new SqlDataAccess();
            var p = new
            {
                Id = id, 
                TopicId = topicId,
                DateModifiedHeader = dateModifiedHeader
            };
            sql.SaveDataSingle<dynamic>("spNoteUpdateTopicId", p, ConnectionStringName);
        }

        public void TrashUntrash(Guid id, DateTime? date, DateTime dateModifiedHeader)
        {
            SqlDataAccess sql = new SqlDataAccess();
            var p = new
            {
                Id = id,
                DateDeleted = date,
                DateModifiedHeader = dateModifiedHeader
            };
            sql.SaveDataSingle<dynamic>("spNoteTrashUntrash", p, ConnectionStringName);
        }

        public void TogglePinned(Guid id, bool pinned, DateTime dateModifiedHeader)
        {
            SqlDataAccess sql = new SqlDataAccess();
            var p = new
            {
                Id = id,
                Pinned = pinned,
                DateModifiedHeader = dateModifiedHeader
            };
            sql.SaveDataSingle<dynamic>("spNoteTogglePinned", p, ConnectionStringName);
        }

        public void ToggleGloballyPinned(Guid id, bool newValue, DateTime dateModifiedHeader)
        {
            SqlDataAccess sql = new SqlDataAccess();
            var p = new
            {
                Id = id,
                GloballyPinned = newValue,
                DateModifiedHeader = dateModifiedHeader
            };
            sql.SaveDataSingle<dynamic>("spNoteToggleGloballyPinned", p, ConnectionStringName);
        }

        public void Delete(Guid id)
        {
            SqlDataAccess sql = new SqlDataAccess();
            var p = new
            {
                Id = id,
            };
            sql.SaveDataSingle<dynamic>("spNoteDelete", p, ConnectionStringName);
        }

        public Guid GetUserIdByNoteId(Guid id)
        {
            SqlDataAccess sql = new SqlDataAccess();
            var p = new
            {
                Id = id
            };
            var result = sql.LoadData<Guid, object>("spGetUserIdByNoteId", p, ConnectionStringName);
            return result[0];
        }
    }
}
