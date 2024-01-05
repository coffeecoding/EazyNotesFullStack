using System;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;

namespace EazyNotes.CryptoServices
{
    public class RFC2898Helper
    {
        /// <summary>
        /// Computes PBKDF2 to produce a password based cryptographic key that can be used as a password hash for authentication.
        /// </summary>
        private static Rfc2898DeriveBytes DeriveKey(string secret, string salt, int iterations, HashAlgorithmName hashAlg)
        {
            byte[] saltBytes = Convert.FromBase64String(salt);
            Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(secret, saltBytes, iterations, hashAlg);
            return pbkdf2;
        }

        public static string ComputePasswordHash(string password, string salt, int iterations, int hashLen)
        {
            Rfc2898DeriveBytes dk = DeriveKey(password, salt, iterations, HashAlgorithmName.SHA512);
            byte[] hash = dk.GetBytes(hashLen);
            return Convert.ToBase64String(hash);
        }

        /// <summary>
        /// Encrypts secret using derived key in accordance with PBES2 RFC2898.
        /// </summary>
        public static string EncryptWithDerivedKey(string password, string salt, byte[] secret)
        {
            HashAlgorithmName hashAlgorithm = HashAlgorithmName.SHA512;
            Rfc2898DeriveBytes dk = DeriveKey(password, salt, AlgorithmIdentifier.ITERATIONS, hashAlgorithm);

            // skip dklen bytes, as those are used as password hash
            byte[] skipped = dk.GetBytes(AlgorithmIdentifier.HASHLEN);

            Aes aes = Aes.Create();
            aes.Key = dk.GetBytes(AlgorithmIdentifier.DKLEN);
            aes.IV = dk.GetBytes(AlgorithmIdentifier.AESIVLEN);
            aes.Padding = PaddingMode.PKCS7;
            aes.Mode = CipherMode.CBC;

            ICryptoTransform encryptor = aes.CreateEncryptor();
            byte[] secret_cipher;
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    byte[] cipher_buffer = new byte[secret.Length];
                    Array.Copy(secret, cipher_buffer, secret.Length);
                    cs.Write(cipher_buffer, 0, cipher_buffer.Length);
                    cs.FlushFinalBlock();
                }
                secret_cipher = ms.ToArray();
            }
            return Convert.ToBase64String(secret_cipher);
        }

        public static byte[] DecryptWithDerivedKey(string password, string salt, string cipherText, AlgorithmIdentifier algorithmIdentifier = null)
        {
            HashAlgorithmName hashAlgorithm = HashAlgorithmName.SHA512;

            if (algorithmIdentifier == null)
                algorithmIdentifier = AlgorithmIdentifier.Default();

            Rfc2898DeriveBytes dk = DeriveKey(password, salt, algorithmIdentifier.Iterations, hashAlgorithm);

            // skip dklen bytes, as those are used as password hash
            dk.GetBytes(AlgorithmIdentifier.HASHLEN);

            Aes aes = Aes.Create();
            aes.Key = dk.GetBytes(algorithmIdentifier.DKLen);
            aes.IV = dk.GetBytes(algorithmIdentifier.AESIVLen);
            aes.Padding = PaddingMode.PKCS7;
            aes.Mode = CipherMode.CBC;

            ICryptoTransform decryptor = aes.CreateDecryptor();
            byte[] privkey;
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write))
                {
                    byte[] cipher_buffer = Convert.FromBase64String(cipherText);
                    cs.Write(cipher_buffer, 0, cipher_buffer.Length);
                    cs.Flush();
                }
                privkey = ms.ToArray();
            }
            return privkey;
        }

        public static byte[] DecryptPrivateKey(string password, string salt, string cipherText, string algorithmIdentifierJson = null)
        {
            AlgorithmIdentifier algorithmIdentifier = JsonSerializer.Deserialize<AlgorithmIdentifier>(algorithmIdentifierJson);
            return DecryptWithDerivedKey(password, salt, cipherText, algorithmIdentifier);
        }
    }
}
