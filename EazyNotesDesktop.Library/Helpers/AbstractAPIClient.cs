using EazyNotes.Models.POCO;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace EazyNotesDesktop.Library.Helpers
{
    public class AuthenticationResult
    {
        public bool Success { get; }
        public object Result { get; }
        public string Message { get; }
        public User User { get; }

        public AuthenticationResult(bool success, object result, string msg, User user = null)
        {
            Success = success;
            Result = result;
            Message = msg;
            User = user;
        }
    }

    public abstract class DataClient
    {
        protected IUserData _userData;

        public DataClient(IUserData userData)
        {
            _userData = userData;
        }

        public virtual void Logout()
        {

        }

        public Guid GetLoggedInUserId()
        {
            return _userData.User.Id;
        }

        public string GetLoggedInUsername()
        {
            return _userData.User.Username;
        }

        public abstract Task RegisterClientDevice();

        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="HttpRequestException"/>
        public abstract Task<AuthenticationResult> AuthenticateUser(string username, string password);

        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="HttpRequestException"/>
        public virtual Task<HttpResponseMessage> GetAsync(string requestUri) { throw new NotImplementedException(); }
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="HttpRequestException"/>
        public virtual Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent body) { throw new NotImplementedException(); }
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="HttpRequestException"/>
        public virtual Task<HttpResponseMessage> PutAsync(string requestUri, HttpContent body) { throw new NotImplementedException(); }
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="HttpRequestException"/>
        /// <exception cref="InvalidOperationException"/>
        public virtual Task<HttpResponseMessage> DeleteAsync(string requestUri) { throw new NotImplementedException(); }
    }
}