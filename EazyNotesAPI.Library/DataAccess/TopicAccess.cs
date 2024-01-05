using EazyNotes.Models.DTO;
using EazyNotesAPI.Library.Internal.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EazyNotesAPI.Library.DataAccess
{
    public class TopicAccess
    {
        private static string ConnectionStringName => SqlDataAccess.ConnectionStringName;

        public IEnumerable<TopicDTO> GetByUserId(Guid userId)
        {
            SqlDataAccess sql = new SqlDataAccess();
            var p = new { UserId = userId };
            var result = sql.LoadData<TopicDTO, dynamic>("spTopicsGetByUserId", p, ConnectionStringName);
            return result;
        }

        public IEnumerable<TopicDTO> GetUntrashedByUserId(Guid userId)
        {
            SqlDataAccess sql = new SqlDataAccess();
            var p = new { UserId = userId };
            var result = sql.LoadData<TopicDTO, dynamic>("spTopicsGetUntrashedByUserId", p, ConnectionStringName);
            return result;
        }

        public TopicDTO GetById(Guid id)
        {
            SqlDataAccess sql = new SqlDataAccess();

            var p = new { Id = id };

            var result = sql.LoadData<TopicDTO, dynamic>("spTopicGetById", p, ConnectionStringName);
            return result.FirstOrDefault();
        }

        public IEnumerable<TopicDTO> GetTrashedByUserId(Guid userId)
        {
            SqlDataAccess sql = new SqlDataAccess();
            var p = new { UserId = userId };
            var result = sql.LoadData<TopicDTO, dynamic>("spTopicsGetTrashedByUserId", p, ConnectionStringName);
            return result;
        }

        public Guid InsertOrUpdate(TopicDTO topic)
        {
            SqlDataAccess sql = new SqlDataAccess();
            sql.SaveDataSingle<dynamic>("spTopicInsertOrUpdate", topic, ConnectionStringName);
            return topic.Id;
        }

        public void UpdateBody(TopicDTO topic)
        {
            SqlDataAccess sql = new SqlDataAccess();
            var p = new
            {
                topic.Id,
                topic.Title,
                topic.Symbol,
                topic.DateModifiedHeader,
                topic.DateModified,
                topic.Color,
            };
            sql.SaveDataSingle<dynamic>("spTopicUpdateBody", p, ConnectionStringName);
        }

        public void UpdateHeader(TopicHeaderDTO topicHeaderDTO)
        {
            SqlDataAccess sql = new SqlDataAccess();
            var p = new
            {
                topicHeaderDTO.Id,
                topicHeaderDTO.Symbol,
                topicHeaderDTO.DateModifiedHeader,
                topicHeaderDTO.DateModified,
                topicHeaderDTO.Color,
                topicHeaderDTO.Position,
            };
            sql.SaveDataSingle<dynamic>("spTopicUpdateHeader", p, ConnectionStringName);
        }

        public void UpdatePosition(Guid id, int newPosition, DateTime dateModifiedHeader)
        {
            SqlDataAccess sql = new SqlDataAccess();

            var p = new
            {
                Id = id,
                Position = newPosition,
                dateModifiedHeader = dateModifiedHeader
            };
            sql.SaveDataSingle<dynamic>("spTopicUpdatePosition", p, ConnectionStringName);
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

            sql.SaveDataSingle<dynamic>("spTopicTrashUntrash", p, ConnectionStringName);
        }

        public void Delete(Guid id)
        {
            SqlDataAccess sql = new SqlDataAccess();
            var p = new
            {
                Id = id,
            };
            sql.SaveDataSingle<dynamic>("spTopicDelete", p, ConnectionStringName);
        }

        public Guid GetUserIdByTopicId(Guid id)
        {
            SqlDataAccess sql = new SqlDataAccess();
            var p = new
            {
                Id = id
            };
            var result = sql.LoadData<Guid, object>("spGetUserIdByTopicId", p, ConnectionStringName);
            return result[0];
        }
    }
}
