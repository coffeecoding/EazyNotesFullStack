using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace EazyNotes.CryptoServices
{
    public class HashingHelper
    {
        public static int CURRENT_VERSION = 1;

        public static string PerformHash(string inputBase64, int version)
        {
            return version switch
            {
                1 => PerformSHA512(inputBase64, 10000),
                _ => PerformSHA512(inputBase64, 10000),
            };
        }

        private static string PerformSHA512(string inputBase64, int iterations)
        {
            SHA512 sha = SHA512.Create();
            byte[] hash = Convert.FromBase64String(inputBase64);
            for (int i = 0; i < iterations; i++) {
                hash = sha.ComputeHash(hash);
            }
            return Convert.ToBase64String(hash);
        }
    }
}
