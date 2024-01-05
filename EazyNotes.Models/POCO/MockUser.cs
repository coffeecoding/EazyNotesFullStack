using EazyNotes.CryptoServices;
using System;

namespace EazyNotes.Models.POCO
{
    public class MockUser : IUser
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string License { get; set; }
        public string Email { get; set; }
        public bool EmailVerified { get; set; }
        public string DisplayName { get; set; }
        public DateTime RegistrationDate { get; set; }
        public string Token { get; set; }
        public string PasswordSalt { get; set; }
        public string PasswordHash { get; set; }
        public string RSAPublicKey { get; set; }
        public string RSAPrivateKeyCrypt { get; set; }
        public AlgorithmIdentifier AlgorithmIdentifier { get; set; }

        public MockUser()
        {
            Id = Guid.Empty;
            Username = "dev";
            License = null;
            Email = "dev@asdf.com";
            EmailVerified = true;
            DisplayName = "Developer";
            PasswordHash = "SP3r1RIsBt/u67HhxxG8bzfD11iHPiHUuzJ6Imy32hA=";
            PasswordSalt = "AR2p1+EN5flEOKRP3fGtAb4MwOP/U8Mvr2gHxti419o=";
            RSAPrivateKeyCrypt = "MIIFNTBfBgkqhkiG9w0BBQ0wUjAxBgkqhkiG9w0BBQwwJAQQqTiKq3/AagfqGinohoFY7QICJxAwDAYIKoZIhvcNAgkFADAdBglghkgBZQMEASoEEDhd06mibiTANjqzMAfaIQwEggTQPB/Nvdrt4Da+tshfYNg6MaEU4wV6i46rseD2+IkR+OUlWCss78rhT+GonhUAqU9dCG6daa1PFGHC646KR3DMu2JTsM2uIpvqjXL3IpkNO0jBFAj5zVbwLD8QEwUHcSgnOUyQ8kf6kk71LgxMkyf8FpaH7uiDJQhZxVUhUUbStSvUNWaQt6y3VUorAayZhtQl2kIIFOeypZsIf63WKzLBH5YXBIWotma6j+ksm2rn1ZOlixvldu0G1OS+vprBRxtO9C7ChQRCr5uFphMkIe0QNetTz4Q8qsHaPAbf7fNpy8vp1qoypZbyJhPUExGjV6DIEzuzqYFqXWp8lExo/a7fe+GWopGqTYpK68oTDCWoPIvPIeJi16TtIkP84MrZzfdaUJozCgWACFDVZcQbU7Es9+EbexGz/1qyMXHMnnk8twUESEPPsQT49MAAfEPO3S2QhJLnlMUuBXFDwEgc+riB/as/rkOru7H5L2IVS+N0a7kduE+LaA9FN1ACBS/goGGf5awW8iQDopf6GkIjfneQ23YL/TMFKx9x+1+SuSC2URVxhf6qN8z4XUCfTGHT+BrIN7fQLOlynvQL9Qn5pJ1lxsFv72Q518/yrgQKMz2xq5AoiqHnCY7rPrXQAbRpCjrKflz6cSMJLaxRRMN13e2jxq7YTG0TwR3BtAyR91h4QtxYfQUeT93yUx16oDrv78F4RR1Z/25SMJUxiruLwLecG9uvATzNvPNR4/FlZUTkR6wWSPovrH5joypbAH2euTa2bVfOH7EnCwXDoKc7fAuadt0EDgYdSk68/YhbzpVyu8DChNsrcN2Xqojo0GnVFXqgdyvJCv0gA6QM+NXaEr6saEwbtazjBQ/uei22hk35W6QFjqJ5A41W58vAGwZ3GBkUhYbeMVvcEbvAuCG3V6bp2FoijOxq2cGXfCNEEJP4D4xWYGZdUTkEfoFZGTkYXCNL4QaTNaAQXkMjfA984QxYF18MxUhotBdW0vMjAVwUwgjiY0NbxMUHFlX2gpaH1bBdMrue7oKHSlV0ifbjiYQ6iDdtXZFfwTfvUxytHO4p6my2n4ENsT2HTkss9ecdIb14B46TP2mf7PCaSwtUcIrYyU7A7aId5700PjSmIouqfgy0r04e0vM+uptaurrcplpx+lF2cwlvbQVa/rSeB2TY9U1YAnLvR38JIqVnodIIbVUsjAiEja2sirWJsNvT0s/QbDR2IQzSrHhXUT2W3g51Pk3733NYFwhtOZdE/LjxdA7J42XICJQixjF/LgA5Kvyb/qzo1biZmayTKyssvCYXowarElS8rKsoXbmRyiFqSCCI8PU3NhmqlloBSTd1nxvEkDX07dWu1f4OTMDUlqj0ie+vuRQECrCHtEWFXV4YcmQjEGIKxhYy+DmqBeRZfFiygTujuVyA/tfU73hxfdTjdl9NaNqG4mFzUI19lTaDx3juqiWKoGqxKwiv/D6SF7PV+1z23ffop9m4wA0hSR4w2wcAN6NKg+yciwlHAOBfNFwGqQl9+03lMrjUZi+fyFllONxHSgINzMXXdKGwP2JlWLahH/IZwACAgsnU8ywVgNU1fZFvwTN5v1zOZaHZj2+1+Gghroh9/5IYQAQ2mREuafL7clB6q8d2xvuijVmQayA=";
            RSAPublicKey = "MIIBCgKCAQEA7Q23QQ5APgDmbHpwUN1p7uk1HSpf86JX0KdcFPhaE3DIQrFyKBZHL8MMh7UCE9jlJr4JYabfEnwI8M+PJWtOP8PzIgTxIqVxsdEEe9RqsZq5HMCa78klO28ROWWzHk3+hxjqldHzP5o7g2wFYb62qMu/Fgw1xQnXSqqb03y0n11NPKuPDiHq7J9vQh4onYa8C9XDSIrkbDgT3lTfTh1IRgbdL6GP3kyswM+Vmnhmee3CPJ+Lyir+6TRoitEJJ3sw2Z4srR29gwYX+DPn07mVQjAqJN4qqX+IuZE7WzSrDTuhI5KVEQZMpNt8pYgg0OvmH3YS3cDylfSXBsJvBLNc3QIDAQAB";
            RegistrationDate = DateTime.UtcNow;
            AlgorithmIdentifier = AlgorithmIdentifier.Default();
        }
    }
}
