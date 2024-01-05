using System;
using System.Text;
using System.Security.Cryptography;
using EazyNotes.CryptoServices;

namespace EazyNotes.Models.POCO
{
    public interface IUserData
    {
        User User { get; }
        string GetRSAPublicKeyXml();
        string GetRSAPrivateKeyXml();
        void ProtectPassword(string password);
        void SetUserData(User loggedInUser, string password);
        void Clear();
    }

    /// <summary>
    /// This class uses the Windows ProtectedData API to securely store user secrets on the device.
    /// For more info check out 
    /// https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.protecteddata
    /// </summary>
    public class UserData : IUserData
    {
        // Create byte array for additional entropy when using Protect method.
        private readonly byte[] _additionalEntropy;
        private byte[] _password;
        private byte[] _rsaPrivateKey;

        public UserData()
        {
            // Init entropy
            _additionalEntropy = new byte[128];
            Random rnd = new Random();
            rnd.NextBytes(_additionalEntropy);
        }

        private User _user;
        public User User => _user;

        public void Clear()
        {
            _user = null;
            _password = null;
            _rsaPrivateKey = null;
        }

        public void SetUserData(User loggedInUser, string password)
        {
            // TODO: Mapping the props from the Interface to the object can be done 
            // by so called "Automapper" as well
            _user = loggedInUser;

            byte[] decryptedRSAPrivateKey = RFC2898Helper.DecryptWithDerivedKey(password,
                loggedInUser.PasswordSalt, loggedInUser.RSAPrivateKeyCrypt, loggedInUser.AlgorithmIdentifier);
            ProtectPrivateKey(decryptedRSAPrivateKey);
            ProtectPassword(password);
        }

        public string GetRSAPublicKeyXml() => User.RSAPublicKey;

        public string GetRSAPrivateKeyXml()
        {
            if (_password == null)
                throw new Exception("Password needed to decrypt RSA Private key");
            if (_rsaPrivateKey == null)
                throw new Exception("Private key is null");
            byte[] privateKey = Unprotect(_rsaPrivateKey);
            return Encoding.UTF8.GetString(privateKey);
        }

        public void ProtectPrivateKey(byte[] rsaPrivateKeyXmlUtf8)
        {
            _rsaPrivateKey = Protect(rsaPrivateKeyXmlUtf8);
        }

        public void ProtectPassword(string password)
        {
            _password = Protect(Encoding.UTF8.GetBytes(password));
        }

        private byte[] GetPassword()
        {
            byte[] pw = Unprotect(_password);
            return pw;
        }

        #region ProtectedData API Methods
        private byte[] Protect(byte[] data)
        {
            try
            {
                // Encrypt the data using DataProtectionScope.CurrentUser. The result can be decrypted
                // only by the same current user.
                return ProtectedData.Protect(data, _additionalEntropy, DataProtectionScope.CurrentUser);
            }
            catch (CryptographicException e)
            {
                Console.WriteLine("Data was not encrypted. An error occurred.");
                Console.WriteLine(e.ToString());
                return null;
            }
        }

        private byte[] Unprotect(byte[] data)
        {
            try
            {
                //Decrypt the data using DataProtectionScope.CurrentUser.
                return ProtectedData.Unprotect(data, _additionalEntropy, DataProtectionScope.CurrentUser);
            }
            catch (CryptographicException e)
            {
                Console.WriteLine("Data was not decrypted. An error occurred.");
                Console.WriteLine(e.ToString());
                return null;
            }
        }
        #endregion
    }
}