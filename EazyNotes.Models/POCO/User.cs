using EazyNotes.Common;
using EazyNotes.Common.JsonConverters;
using EazyNotes.CryptoServices;
using EazyNotes.Models.DTO;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EazyNotes.Models.POCO
{
    // used for registering with the DI service; like this it can be mocked for tests as well
    public interface IUser
    {
        Guid Id { get; set; }
        string Username { get; set; }
        string License { get; set; }
        string Email { get; set; }
        bool EmailVerified { get; set; }
        string DisplayName { get; set; }
        [JsonConverter(typeof(CustomDateTimeConverter))]
        DateTime RegistrationDate { get; set; }
        string PasswordSalt { get; set; }
        string PasswordHash { get; set; }
        string RSAPublicKey { get; set; }
        string RSAPrivateKeyCrypt { get; set; }
    }

    public class User : IUser
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string License { get; set; }
        public string Email { get; set; }
        public bool EmailVerified { get; set; }
        public string DisplayName { get; set; }
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime RegistrationDate { get; set; }
        public string PasswordSalt { get; set; }
        public string PasswordHash { get; set; }
        public string RSAPublicKey { get; set; }
        public string RSAPrivateKeyCrypt { get; set; }
        public AlgorithmIdentifier AlgorithmIdentifier { get; set; }

        public User()
        {
            // For deserialization using System.Text.Json
        }

        public User(Guid id, string username, string license, string email, bool emailVerified, string displayName,
            string pwHash, string salt, string privateKey, string publicKey, DateTime registrationDate, AlgorithmIdentifier algorithmIdentifier)
        {
            Id = id;
            Username = username;
            License = license;
            Email = email;
            EmailVerified = emailVerified;
            DisplayName = displayName;
            PasswordHash = pwHash;
            PasswordSalt = salt;
            RSAPublicKey = publicKey;
            RSAPrivateKeyCrypt = privateKey;
            RegistrationDate = registrationDate;
            AlgorithmIdentifier = algorithmIdentifier;
        }

        public static bool IsValidNamesAndEmail(string name, string username, string email)
        {
            // TODO: Verify
            return true;
        }

        public bool IsValid()
        {
            // TODO: IMPLEMENT
            return true;
        }

        public UserDTO ToUserDTO()
        {
            return new UserDTO(Id, Username, License, Email, EmailVerified, DisplayName, PasswordHash, PasswordSalt,
                    RSAPrivateKeyCrypt, RSAPublicKey, RegistrationDate, JsonSerializer.Serialize(AlgorithmIdentifier));
        }

        public string ToJson()
        {
            return JsonSerializer.Serialize(this);
        }

        public User CloneWith(UserProfile user)
        {
            return new User(Id, user.Username, License, user.Email, EmailVerified, user.DisplayName, PasswordHash,
                PasswordSalt, RSAPrivateKeyCrypt, RSAPublicKey, RegistrationDate, AlgorithmIdentifier);
        }

        public UserProfile GetUserProfile()
        {
            return new UserProfile(Id, Username, DisplayName, Email, EmailVerified);
        }

        public override bool Equals(object obj)
        {
            User u = obj as User;
            if (obj == null || u == null)
                return false;
            string thisJson = ToJson();
            string otherJson = u.ToJson();
            return thisJson.Equals(otherJson);
        }
    }
}
