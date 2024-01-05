using System;

namespace EazyNotes.Models.POCO
{
    public class UserProfile
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public bool EmailVerified { get; set; }

        public UserProfile()
        {
            // for json
        }

        public UserProfile(Guid id, string username, string displayname, string email, bool emailverified)
        {
            Id = id;
            Username = username;
            DisplayName = displayname;
            Email = email;
            EmailVerified = emailverified;
        }
    }

    public class UserKeys
    {
        public Guid Id { get; set; }
        public string PasswordHash { get; set; }
        public string RSAPrivateKeyCrypt { get; set; }
        public string AlgorithmIdentifier { get; set; }
    }
}
