using EazyNotes.Models.DTO;
using EazyNotesAPI.Library.Internal.DataAccess;
using System.Collections.Generic;
using System.Linq;

// This namespace is our data access interface for our api to talk to the database
namespace EazyNotesAPI.Library.DataAccess
{
    public class UserDataAccess
    {
        private static string ConnectionStringName => SqlDataAccess.ConnectionStringName;

        public UserDTO GetUserById(string Id)
        {
            // TODO: Use DI for injection
            SqlDataAccess sql = new SqlDataAccess();

            var p = new { Id };

            var result = sql.LoadData<UserDTO, dynamic>("spUserGetById", p, ConnectionStringName);
            return result.SingleOrDefault();
        }

        public UserDTO GetUserByEmailOrUsername(string emailOrUsername)
        {
            List<UserDTO> user = GetUserByEmail(emailOrUsername);
            if (user.SingleOrDefault() == null)
                return GetUserByUsername(emailOrUsername).SingleOrDefault();
            return user.SingleOrDefault();
        }

        private List<UserDTO> GetUserByUsername(string username)
        {
            // TODO: Use DI for injection
            SqlDataAccess sql = new SqlDataAccess();

            var p = new { Username = username };

            var result = sql.LoadData<UserDTO, dynamic>("spUserGetByUsername", p, ConnectionStringName);
            return result;
        }

        private List<UserDTO> GetUserByEmail(string email)
        {
            // TODO: Use DI for injection
            SqlDataAccess sql = new SqlDataAccess();

            var p = new { Email = email };

            var result = sql.LoadData<UserDTO, dynamic>("spUserGetByEmail", p, ConnectionStringName);
            return result;
        }

        public UserDTO InsertUser(UserDTO user)
        {
            SqlDataAccess sql = new SqlDataAccess();
            return sql.SaveData("spUserInsert", user, ConnectionStringName);
        }

        public UserDTO UpdateUser(UserDTO user)
        {
            SqlDataAccess sql = new SqlDataAccess();
            return sql.SaveData("spUserUpdate", user, ConnectionStringName);
        }
    }
}
