using EazyNotes.Models.DTO;
using EazyNotesAPI.Library.Internal.DataAccess;
using System.Collections.Generic;

namespace EazyNotesAPI.Library.DataAccess
{
    public class FeedbackAccess
    {
        private static string ConnectionStringName => SqlDataAccess.ConnectionStringName;

        public IEnumerable<FeedbackDTO> Get()
        {
            SqlDataAccess sql = new SqlDataAccess();

            var result = sql.LoadData<FeedbackDTO, dynamic>("spFeedbackGetAll", null, ConnectionStringName);
            return result;
        }

        public long Insert(FeedbackDTO feedback)
        {
            SqlDataAccess sql = new SqlDataAccess();

            var p = new
            {
                feedback.Id,
                feedback.Title,
                feedback.Body,
                feedback.Category,
                feedback.AppVersion,
                feedback.AddressedInVersion,
                feedback.DeviceName,
                feedback.Platform,
                feedback.Submitted
            };
            long id = sql.SaveDataSingle<dynamic>("spFeedbackInsert", p, ConnectionStringName);
            return id;
        }

        public dynamic Delete(long id)
        {
            SqlDataAccess sql = new SqlDataAccess();

            var p = new
            {
                Id = id,
            };
            return sql.SaveData<dynamic>("spFeedbackDelete", p, ConnectionStringName);
        }

        public void SetAddressed(long id, bool newValue)
        {
            SqlDataAccess sql = new SqlDataAccess();

            var p = new
            {
                Id = id,
                Addressed = newValue
            };
            sql.SaveData<dynamic>("spFeedbackToggleAddressed", p, ConnectionStringName);
        }
    }
}
