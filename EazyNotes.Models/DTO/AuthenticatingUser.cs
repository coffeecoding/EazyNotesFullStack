namespace EazyNotes.Models.DTO
{
    public class AuthenticatingUser
    {
        public string EmailOrUsername { get; set; }
        public string PasswordHash { get; set; }

        public AuthenticatingUser() { /* FOR JSON */}

        public AuthenticatingUser(string emailOrUsername, string pwHash)
        {
            EmailOrUsername = emailOrUsername;
            PasswordHash = pwHash;
        }
    }
}
