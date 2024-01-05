using EazyNotes.CryptoServices;

namespace EazyNotes.Models.DTO
{
    public class UserPWParams
    {
        public string PasswordSalt { get; set; }
        public AlgorithmIdentifier AlgorithmIdentifier { get; set; }
    }
}
