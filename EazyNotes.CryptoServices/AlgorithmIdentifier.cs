using System.Text.Json;

namespace EazyNotes.CryptoServices
{
    /// <summary>
    /// Algorithm Identifier to wrap the parameters used for PBES2 with PBKDF2 in accordance with Rfc2898.
    /// See https://www.ietf.org/rfc/rfc2898.txt, in particular section 6.2.1
    /// </summary>
    public class AlgorithmIdentifier
    {
        public const int ITERATIONS = 10000;
        // rsa key length in bit
        public const int RSA_KEYLEN = 2048;
        // desired length of hashed secret (e.g. password) in bytes
        public const int HASHLEN = 32;
        // salt length in bytes
        public const int SALTLEN = 20;
        // derived length in bytes
        public const int DKLEN = 32;
        // aes iv length in bytes, typically 16; used in pbe
        public const int AESIVLEN = 16;

        public string Name { get; set; }
        public int Iterations { get; set; }
        public int SaltLen { get; set; }
        public int HashLen { get; set; }
        public int RSAKeyLen { get; set; }
        public int DKLen { get; set; }
        public int AESIVLen { get; set; }

        public AlgorithmIdentifier()
        {

        }

        public AlgorithmIdentifier(string name, int iterations, int hashLen, int saltlen, int rsaKeyLen, int dklen, int aes_ivlen)
        {
            Name = name;
            Iterations = iterations;
            HashLen = hashLen;
            SaltLen = saltlen;
            RSAKeyLen = rsaKeyLen;
            DKLen = dklen;
            AESIVLen = aes_ivlen;
        }

        public static AlgorithmIdentifier Default()
        {
            return new AlgorithmIdentifier("PBES2", ITERATIONS, HASHLEN, SALTLEN, RSA_KEYLEN, DKLEN, AESIVLEN);
        }

        public static string DefaultJson()
        {
            return JsonSerializer.Serialize(Default());
        }

        public string ToJson()
        {
            return JsonSerializer.Serialize(this);
        }

        public static AlgorithmIdentifier FromJson(string json)
        {
            return JsonSerializer.Deserialize<AlgorithmIdentifier>(json);
        }
    }

    /// <summary>
    /// AesParameters for subsequent symmetric cryptography on top of pbkdf2. Not used currently as instead we
    /// are using the PbeParams class.
    /// </summary>
    public class AesParameters
    {
        public const int AES_IVLEN = 16;
        public const string AES_PADDING = "PKCS7";
        public const string AES_CIPHERMODE = "CBC";

        public string IV { get; }
        public int IVLen { get; }
        public string Padding { get; }
        public string CipherMode { get; }

        public AesParameters(string iv, int ivlen, string padding, string ciphermode)
        {
            IV = iv;
            IVLen = ivlen;
            Padding = padding;
            CipherMode = ciphermode;
        }
    }
}
