using EazyNotes.CryptoServices;
using EazyNotes.Models.DTO;
using EazyNotes.Models.POCO;
using EazyNotesDesktop.Library.DAO;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EazyNotesDesktop.Library.Helpers
{
    public enum SQLiteStatus
    {
        LoginSuccess = 0,
        ErrorWrongCredentials,
        ErrorUserNotFound,
        ErrorDBFileNotFound,
        ErrorDBDirectoryNotFound,
        ErrorParsingUserData,
        ErrorUnknown
    }

    public static class SQLiteConfig
    {
        // TODO: Verify this is a reliable path to copy the db to
        public static readonly string BUILD_PATH = @"SQL";
        public static readonly string TEMPLATEDB_FILENAME = "LocalSQLiteDB.db";
    }

    public class SQLiteClient : DataClient
    {
        public static string GetSQLiteDBFilenameByUser(string username)
        {
            return $"{username}.db";
        }

        public SQLiteClient(IUserData userData) : base(userData)
        {

        }
        /// <summary>
        /// Creates new, empty SQlite db for user with given name.
        /// </summary>
        public static (bool, string) Init(IUser user)
        {
            StringBuilder log = new StringBuilder();
            log.AppendLine("Initializing database...");
            try
            {
                log.AppendLine($"SQLite deployment path {ENClient.USERS_BASE_PATH}");
                string destinationDB = ENClient.GetUserDBPath(user.Username);
                log.AppendLine("Checking if local db already exists");
                if (File.Exists(destinationDB))
                    log.AppendLine("Positive");
                else
                {
                    log.AppendLine("Negative");
                    string sourceDB = Path.Combine(Environment.CurrentDirectory, SQLiteConfig.BUILD_PATH, SQLiteConfig.TEMPLATEDB_FILENAME);
                    log.AppendLine("Copying template db to deployment path");
                    File.Copy(sourceDB, destinationDB, false);
                    log.AppendLine("Finished copying template db.");
                }
                return (true, log.ToString());
            }
            catch (Exception e)
            {
                log.AppendLine(e.ToString());
                return (false, log.ToString());
            }
        }

        public override Task<AuthenticationResult> AuthenticateUser(string username, string password)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            User user = null;
            try
            {
                string dbPath = ENClient.GetUserDBPath(username);
                UserDTO userDTO = SQLiteDataSource.GetUser(username);
                if (userDTO == null)
                    return Task.FromResult(new AuthenticationResult(false, SQLiteStatus.ErrorUserNotFound, 
                        "User information for the given username not found.", null));
                user = userDTO.ToUser();

                // check credentials:
                if (user.Username != username)
                {
                    response.StatusCode = HttpStatusCode.NotFound;
                    return Task.FromResult(new AuthenticationResult(false, 
                        SQLiteStatus.ErrorWrongCredentials, "Invalid credentials!"));
                }

                string pwSubmittedHash = RFC2898Helper.ComputePasswordHash(password, user.PasswordSalt, 
                    user.AlgorithmIdentifier.Iterations, user.AlgorithmIdentifier.HashLen);

                if (!user.PasswordHash.Equals(pwSubmittedHash))
                {
                    response.StatusCode = HttpStatusCode.Unauthorized;
                    return Task.FromResult(new AuthenticationResult(false,
                        SQLiteStatus.ErrorWrongCredentials, "Wrong password!"));
                }

                _userData.SetUserData(user, password);
            }
            catch (FileNotFoundException)
            {
                response.StatusCode = HttpStatusCode.NotFound;
                return Task.FromResult(new AuthenticationResult(false,
                    SQLiteStatus.ErrorDBFileNotFound, "Couldn't find user data. DB not found."));
            }
            catch (DirectoryNotFoundException)
            {
                response.StatusCode = HttpStatusCode.NotFound;
                return Task.FromResult(new AuthenticationResult(false,
                    SQLiteStatus.ErrorDBDirectoryNotFound, "Couldn't find user data. DB directory not found."));
            }
            catch (Exception e)
            {
                if (e is JsonException || e is ArgumentNullException)
                {
                    response.StatusCode = HttpStatusCode.BadRequest;
                    return Task.FromResult(new AuthenticationResult(false,
                        SQLiteStatus.ErrorParsingUserData, "Error reading user data."));
                }
                else throw;
            }
            response.StatusCode = HttpStatusCode.OK;
            return Task.FromResult(new AuthenticationResult(true,
                SQLiteStatus.LoginSuccess, "Login successful.", user));
        }

        public static async Task SaveUserLocally()
        {

        }

        public override async Task RegisterClientDevice()
        {

        }
    }
}
