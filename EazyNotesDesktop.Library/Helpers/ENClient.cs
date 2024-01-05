using EazyNotes.Common;
using EazyNotes.Models.POCO;
using System;
using System.IO;
using System.Threading.Tasks;

namespace EazyNotesDesktop.Library.Helpers
{
    public class ENClient
    {
        public static readonly string USERS_BASE_PATH = $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\EazyNotes\Users\";
        // This is a static method as it needs to be used without state as well, when signing up and initializing a user locally
        private static string GetUserFoldername(string username) => $"{username}";
        public static string GetUserPath(string username) => Path.Combine(USERS_BASE_PATH, GetUserFoldername(username));
        public static string GetUserDBPath(string username) => Path.Combine(USERS_BASE_PATH, GetUserFoldername(username), SQLiteClient.GetSQLiteDBFilenameByUser(username));

        public IUserData UserData { get; private set; }
        public SQLiteClient SqliteClient { get; set; }
        public APIClient ApiClient { get; set; }

        public IUser User => UserData.User;

        public ENClient(IUserData userData, SQLiteClient sqliteClient, APIClient apiClient)
        {
            UserData = userData;
            SqliteClient = sqliteClient;
            ApiClient = apiClient;
        }

        public async Task<AuthenticationResult> LoginApiClient(string username, string password)
            => await ApiClient.AuthenticateUser(username, password);
        
        public async Task<AuthenticationResult> LoginSQLiteClient(string username, string password)
            => await SqliteClient.AuthenticateUser(username, password);

        public void Logout()
        {
            UserData.Clear();
            SqliteClient.Logout();
            ApiClient.Logout();
        }

        public async Task RegisterClientDevice() => await ApiClient.RegisterClientDevice();
    }
}
