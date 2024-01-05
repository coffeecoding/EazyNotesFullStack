using EazyNotes.Models.DTO;
using EazyNotesAPI.Library.Internal.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EazyNotesAPI.Library.DataAccess
{
    public class ClientAccess
    {
        private static string ConnectionStringName => SqlDataAccess.ConnectionStringName;

        public IEnumerable<Client> Get()
        {
            SqlDataAccess sql = new SqlDataAccess();

            var result = sql.LoadData<Client, dynamic>("spClientsGetAll", null, ConnectionStringName);
            return result;
        }

        public Client GetExactBy(string username, string deviceName, string platform)
        {
            SqlDataAccess sql = new SqlDataAccess();

            var p = new
            {
                Username = username,
                DeviceName = deviceName,
                Platform = platform
            };

            var result = sql.LoadData<Client, dynamic>("spClientGetExact", p, ConnectionStringName);
            return result.ToList().SingleOrDefault();
        }

        public void Insert(Client client)
        {
            SqlDataAccess sql = new SqlDataAccess();

            var p = new
            {
                client.Id,
                client.Username,
                client.DeviceName,
                client.Platform,
                client.Country,
                client.Registered
            };
            sql.SaveDataSingle<dynamic>("spClientInsert", p, ConnectionStringName);
        }

        public dynamic Delete(long id)
        {
            SqlDataAccess sql = new SqlDataAccess();

            var p = new
            {
                Id = id,
            };
            return sql.SaveData<dynamic>("spClientDelete", p, ConnectionStringName);
        }
    }
}
