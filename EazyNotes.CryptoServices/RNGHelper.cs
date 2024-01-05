using System;
using System.Security.Cryptography;

namespace EazyNotes.CryptoServices
{
    public class RNGHelper
    {
        public static string GenerateSalt()
        {
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] saltBytes = new byte[AlgorithmIdentifier.SALTLEN];
            rng.GetBytes(saltBytes);
            return Convert.ToBase64String(saltBytes);
        }
    }
}
