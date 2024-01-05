using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace EazyNotes.CryptoServices
{
    public class StatefulAES
    {
        public byte[] Key => _aes.Key;
        public byte[] IV => _aes.IV;
        private AesCryptoServiceProvider _aes;

        public StatefulAES()
        {
            InitAes();
            _aes.GenerateKey();
            _aes.GenerateIV();
        }

        public StatefulAES(byte[] key, byte[] iv)
        {
            InitAes();
            _aes.Key = key;
            _aes.IV = iv;
        }

        private void InitAes()
        {
            _aes = new AesCryptoServiceProvider
            {
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7,
                KeySize = 256
            };
        }

        public string EncryptToBase64(string plainText)
        {
            return Convert.ToBase64String(Encrypt(plainText));
        }

        private byte[] Encrypt(string plainText)
        {
            byte[] encrypted;
            ICryptoTransform encryptor = _aes.CreateEncryptor();

            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
                using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                {
                    swEncrypt.Write(plainText);
                }
                encrypted = msEncrypt.ToArray();
            }
            return encrypted;
        }

        public string DecryptFromBase64(string base64CipherText)
        {
            return Decrypt(Convert.FromBase64String(base64CipherText));
        }

        private string Decrypt(byte[] cipherText)
        {
            string plaintext = null;

            ICryptoTransform decryptor = _aes.CreateDecryptor();
            StringBuilder log = new StringBuilder("[ => BEGIN LOG AES DECRYPT]");
            log.AppendLine($"Decryption of: {BitConverter.ToString(cipherText)}");
            log.AppendLine($" With AES Key: {BitConverter.ToString(_aes.Key)}");
            log.AppendLine($" And IV:       {BitConverter.ToString(_aes.IV)}");

            using (MemoryStream msDecrypt = new MemoryStream(cipherText))
            {
                using CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
                try
                {
                    using StreamReader srDecrypt = new StreamReader(csDecrypt, Encoding.UTF8);
                    
                    plaintext = srDecrypt.ReadToEnd();
                    log.AppendLine("AES SUCCESSFUL OPERATION -- END LOG <= ]");
                    Debug.WriteLine(log.ToString());
                }
                catch (CryptographicException e)
                {
#if DEBUG
                    log.AppendLine($"Failure decrypting {BitConverter.ToString(cipherText)}");
                    string err = $"Exception in AESDecrypt: {e.GetType()}: {e.Message}; Stack: {e.StackTrace}";
                    log.AppendLine(err);
                    log.AppendLine("AES FATAL ERROR -- END LOG <= ]");
                    throw (new Exception(log.ToString()));
#endif
                }
            }
            return plaintext;
        }
    }
}
