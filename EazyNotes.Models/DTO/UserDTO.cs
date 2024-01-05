using EazyNotes.Common.JsonConverters;
using EazyNotes.CryptoServices;
using EazyNotes.Models.POCO;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EazyNotes.Models.DTO
{
    public class UserDTO : ICloneable, IUser
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string License { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public bool EmailVerified { get; set; }
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime RegistrationDate { get; set; }
        public string PasswordSalt { get; set; }
        public string PasswordHash { get; set; }
        public string RSAPublicKey { get; set; }
        public string RSAPrivateKeyCrypt { get; set; }
        public string AlgorithmIdentifier { get; set; }

        public UserDTO() { }

        public UserDTO(Guid id, string username, string license, string email, bool emailVerified, string displayName, string pwHash,
            string salt, string privateKey, string publicKey, DateTime registrationDate, string algorithmIdentifier)
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

        public object Clone()
        {
            return new UserDTO(Id, Username, License, Email, EmailVerified, DisplayName, PasswordHash,
                PasswordSalt, RSAPrivateKeyCrypt, RSAPublicKey, RegistrationDate, AlgorithmIdentifier);
        }

        public User ToUser()
        {
            return new User(Id, Username, License, Email, EmailVerified, DisplayName, PasswordHash, PasswordSalt,
                    RSAPrivateKeyCrypt, RSAPublicKey, RegistrationDate, JsonSerializer.Deserialize<AlgorithmIdentifier>(AlgorithmIdentifier));
        }

        public List<APIValidationError> IsValid()
        {
            if (Id == Guid.Empty)
                return new List<APIValidationError>() { new APIValidationError("User", "Id", "may not be empty") };
            if (Username.Length > Constraints.USER_USERNAME_MAXLEN)
                return new List<APIValidationError>() { new APIValidationError("User", "Username", "is too long") };
            if (!Constraints.USER_USERNAME_ISVALID(Username, out string msg))
                return new List<APIValidationError>() { new APIValidationError("User", "Username", msg) };
            if (DisplayName.Length > Constraints.USER_DISPLAYNAME_MAXLEN)
                return new List<APIValidationError>() { new APIValidationError("User", "Display Name", "is too long") };
            if (Email.Length > Constraints.USER_EMAIL_MAXLEN)
                return new List<APIValidationError>() { new APIValidationError("User", "Email", "is too large") };
            if (RSAPublicKey.Length >= Constraints.USER_RSAPUBKEY_MAXLEN)
                return new List<APIValidationError>() { new APIValidationError("User", "PublicKey", "is too long") };
            if (RSAPrivateKeyCrypt.Length >= Constraints.USER_RSAPRIVKEY_MAXLEN)
                return new List<APIValidationError>() { new APIValidationError("User", "encrypted PrivateKey", "is too long") };
            if (AlgorithmIdentifier.Length >= Constraints.USER_ALGORITHMIDENTIFIER_MAXLEN)
                return new List<APIValidationError>() { new APIValidationError("User", "Algorithm Identifier", "is too long") };
            return null;
        }

        public static UserDTO CreateUser(string username, string email, string displayname, string password)
        {
            string saltBase64 = RNGHelper.GenerateSalt();

            AlgorithmIdentifier algorithmIdentifier = EazyNotes.CryptoServices.AlgorithmIdentifier.Default();
            string serializedAlgoId = JsonSerializer.Serialize(algorithmIdentifier);

            string passwordHash = RFC2898Helper.ComputePasswordHash(password, saltBase64,
                algorithmIdentifier.Iterations, algorithmIdentifier.HashLen);

            (string rsaPubKeyXml, string rsaPrivKeyXmlCrypt) = RSAHelper.CreateRSAKeyPair(password, saltBase64);

            UserDTO NewUser = new UserDTO(Guid.NewGuid(), username, null, email, false, displayname, passwordHash,
                saltBase64, rsaPrivKeyXmlCrypt, rsaPubKeyXml, DateTime.UtcNow, serializedAlgoId);
            return NewUser;
        }
    }
}
