using System.Security.Cryptography;
using System.Text;

namespace EazyNotes.CryptoServices
{
    public class RSAHelper
    {
        public static (string, string) CreateRSAKeyPair(string passwordToEncryptPrivKey, string salt)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(AlgorithmIdentifier.RSA_KEYLEN);

            string rsaPubKeyXml = rsa.ToXmlString(false);
            string rsaPrivKeyXml = rsa.ToXmlString(true);
            byte[] rsaPrivKeyXmlBytes = Encoding.UTF8.GetBytes(rsaPrivKeyXml);
            string rsaPrivKeyCryptBase64 = RFC2898Helper.EncryptWithDerivedKey(passwordToEncryptPrivKey, salt, rsaPrivKeyXmlBytes);

            return (rsaPubKeyXml, rsaPrivKeyCryptBase64);
        }

        public static byte[] Encrypt(byte[] data, string keyXml)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(keyXml);
            byte[] encryptedBytes = rsa.Encrypt(data, RSAEncryptionPadding.Pkcs1);
            return encryptedBytes;
        }

        public static byte[] Decrypt(byte[] data, string privKeyXml)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(privKeyXml);
            byte[] decryptedBytes = rsa.Decrypt(data, RSAEncryptionPadding.Pkcs1);
            return decryptedBytes;
        }
    }
}
