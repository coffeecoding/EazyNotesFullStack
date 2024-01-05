using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace EazyNotes.CryptoServices
{
    static class DateTimeExtensions
    {
        /// <summary>
        /// Interprets the time as UTC and returns its String representatino according to ISO8601.
        /// ISO8601 format: "YYYY-MM-DDTHH:MM:DD.<FRACTION>Z"
        /// </summary>
        public static string ToISO8601UTCString(this DateTime time)
        {
            return $"{time.Year}-{time.Month.ToString().PadLeft(2, '0')}-{time.Day.ToString().PadLeft(2, '0')}T{time.Hour.ToString().PadLeft(2, '0')}:{time.Minute.ToString().PadLeft(2, '0')}:{time.Second.ToString().PadLeft(2, '0')}.{time.Millisecond}Z";
        }
    }

    class MyClassyClass
    {
        public int Id { get; set; }
        public DateTime DateCreated { get; set; }

        public MyClassyClass() { }
        public MyClassyClass(int id, DateTime dateTime) { Id = id; DateCreated = dateTime; }
    }

    public class Program
    { 
        public static void Main(string[] args)
        {
            string salt = "ppwz5QyU4Mcb4+H2kVoqDEk/Pl8=";
            string pw = "asdf";

            string hash = RFC2898Helper.ComputePasswordHash(pw, salt, 100000, 32);
            Console.WriteLine("Password Hash:");
            Console.WriteLine(hash);
            Console.ReadKey();
        }

        public static void MainXYZ(string[] args)
        {
            DateTime time = DateTime.Parse("2021-09-13T17:47:52.110Z");
            String nowIsoString = DateTime.UtcNow.ToISO8601UTCString();
            Console.WriteLine("Parsed from iso 8601 utc:");
            Console.WriteLine(time);
            Console.WriteLine("Converted to iso 8601 utc:");
            Console.WriteLine(nowIsoString);
            DateTime back = DateTime.Parse(nowIsoString);
            Console.WriteLine("Converted back from own implementatnio of iso 8601 utc:");
            Console.WriteLine(back);

            // Json (De)serialization of DateTime in form of string, does it work automatically? => Yes
            MyClassyClass myObj = new MyClassyClass(2310, DateTime.Now.ToUniversalTime());
            string toJson = JsonConvert.SerializeObject(myObj, Formatting.Indented);
            Console.WriteLine(toJson);

            myObj = JsonConvert.DeserializeObject<MyClassyClass>(toJson);
            string againToJson = JsonConvert.SerializeObject(myObj, Formatting.Indented);
            Console.WriteLine(againToJson);
            Console.WriteLine(myObj.DateCreated.ToString());
            Console.WriteLine(myObj.DateCreated.ToISO8601UTCString());

            Console.ReadKey();
        }

        public static void Main2323(string[] args)
        {
            // Test parse dart exported RSA Priv key where the chinese remainder params are missing.
            // Does that work?
            string dartXmlPrivKey = "<RSAKeyValue><Modulus>thUzyft/A1os8aY0KOgoeNqcWezTOOTiZqSj0bYRHGwrNL/7Jmkkj/OkONHIeZNO1xCTe7funMIWBYAffnZA+15o8xAltSjHB4kVY1B9GjwUpxs/KOYFYWAb8FuKxkervGiTJI6Zi/TLmULPb1qqOMq/zsEhxw+EIBxqrgatpSxUCa+eFiwlCFRmxFN8STtwjDd+VV0qkoxzF3mZJNPIZPCSBj51JrKc6QloWUHCrI1vcbYLwOdLdDirsenruBLLc5sjFqjsOliBkp41YPQeoG6RQnCSukskAJ74NyB9U+ezctcStaK5UNJbBHMz6XYP9V7XGx7glrD+Pb/5tBIH1w==</Modulus><Exponent>AQAB</Exponent><P>/4JsUQ6SNqkJ9S97viSxcHWtFrSQkqr9FKCRTJqK8QBHRaCW4wLdktNhQrq+EOa+EdMU2nKr76aEV0qeDmxsGVMSzYx0tsRFuqIfcdmUjTaeK5xBOknuaC+YH8I1S5v9YrnNYLUIQQallr/oSCtF8ksP6/k3JegcjKBBeAiL0Wc=</P><Q>tm6xFK2Pb2unu9OrD4YjPjYgl0W31EtIuNvsozqRytRE2gl310BUkWoyHmgapgBXNTKkZRIB5gDfEcgc02JO6PgypP6E0cu0WikpfeCnZXHeXngHzl+x/l8LzJrSXOIeWQW5dE+BHPrVtiKa1hIPOoL+DPxEDiVbj0XBtBOZ4BE=</Q><DP>NTSkd5CQM9+pWiwkGWkspd1rSLIa9N+0SapR92CrFBpZvQ7+vJDpMNzhgPLAB2b1J2MMEZ0VXpcPMdow5ZIARJm+7ZyU5UYjuwRthdioDWvVLgGgt5GTmpBmnh5j2LXH4toMAVpROLE4pBacOQMB0r18uEeyCyu1Xjc0ytkEvHs=</DP><DQ>qR72tJetd/pSNfKlCzAr7dlZ30K3h2v/T7g5qJx8WY7tAUBzw62UeHX9XmjeA3QfZGa9NiE6d1Hped+MsX3ab2jf8kstMBxq5oZwO4yPQqEMJ4GYov3d0VWlZ5lDWO4aeiavZR7rL8VtZ3qvFbMKQRD8mWcnZAHH3ospZlohdTE=</DQ><InverseQ>gfaVdA6B1waXAlNfY5LSYuIVNU37mDmfFZfGTJQv1O4KYNVfDPYuDVFqSVGSD8zDE5p91jr55hDV7yk6aE2k2DVldT2zN/geRghKq370dz620IL9aa/PEC2TPNMrN36TKlODdS0JeDOTnN7bCqW24B6GEC3rEzvFbSZYwp3mUL0=</InverseQ><D>jLD/Tk+MybtQxPuGjCEuvIs2imlqoCiDI4qNpkoHGK4gCGdbGUfe33gD6X93ZzFClv0Q6GSr8+Yn++IO87lN5q0BbOaKLFuwxgrPQuj7cV0BHEn6WJC7kY0gqW0iYqkYcsFfCL1CuZjSBL9R5xnoM2++G3Sxs80ya1lpuMQiQmAWvqek8TXw+DI+PGBZIL4Him5YdzgpGUShS4qvPZAUHjHvB3o97vHiy6GsWwd5+AArQ52W9eX0NuyFyNgg0XNqVEc53gUMfqTtamxiIDtohfI2SBx3Gv/C2CKB+uh69CPhZmVW9dPA8SI0IlcPd+wFeOiMbgr1EyaoBeE1/vC3oQ==</D></RSAKeyValue>";
            RSACryptoServiceProvider rsa2 = new RSACryptoServiceProvider(2048);
            rsa2.FromXmlString(dartXmlPrivKey);
            Console.WriteLine(rsa2.ToXmlString(true));
            Console.ReadKey();
        }

        public static void MainDateTime(string[] args)
        {
            // just some datetime tests
            DateTime now = DateTime.Now.ToUniversalTime();
            Console.WriteLine($"Common String: {now.ToISO8601UTCString()}");
            Console.WriteLine("UTC time string:");
            Console.WriteLine(now.ToUniversalTime());
            Console.WriteLine(now.ToUniversalTime().ToLocalTime());
            Console.WriteLine(now);

            Console.WriteLine(now.ToLongDateString());
            Console.WriteLine("This is the kind of time that now is:");
            Console.WriteLine(now.Kind);
            Console.WriteLine("Ok, a test ... :");
            DateTime time1 = DateTime.Parse("20.08.2021 14:00:23");
            bool success2 = DateTime.TryParse("20.08.2021 14:00:23Z", out DateTime time2);
            Console.WriteLine(time1);
            if (success2)
                Console.WriteLine(time2);
            else
                Console.WriteLine("the one with trailing 'Z' failsed");

            Console.ReadKey();
        }

        public static void MainOld1(string[] args)
        {
            // Performance test: PBKDF2 based symmetric decryptino vs UnprotectData API from Windows:
            // Sample key was generated with PBES2 10000 iterations
            string pw = "asdf";
            string pwSalt = "h7/o0xRJO/0iybOHaGv/1d9uj8k=";
            string privCrypt = "P1P6vxQ72IlgbAeAKdIKe7oFFvRJp3ack4lIT4YdaPx/3qA2\u002BOejIbF5Er\u002BixjyBtyBvOciqaKlNQ5tQq6nj/9xiaZRhO8kzb0IM7D43d3ThkKBsHfMok\u002BUHA9CqmkUxCKClAtQmv1x7moMS2AbPMcP3/UIr4v1fk8VLn7Eorw\u002Bc0/v0ugGIFJ\u002B2dayuWuUCC0lol3Fa4YPshRZvBqNIO33JF/h1RCotK7Dqa3setbA6YScRuHKq6nipf07\u002BTs7C0o52bop12ErqqYri9kgelJbNJ\u002BucgTYnhYfqhTrMRtZ9NB7fAgwzC/uGgFxHtwumEoxN5pm8AzTWhkX9pcb6C4OBhJ1\u002BWma8moryDpGjwJpGHdGOTQAQ8F7iMJYxmrRLsvhl28WmN6W2ffOQnH/QJm8m2Py6RC4LUkhR0CrpVI2fc0x\u002BWrkFgjqsY3kxDr0nPyj\u002BfFmOoOvAdLqy2oyNjQxv66yOS4kf7nfhPGhRF9MxvhRpRLMPR4lAZO5s4QxUEEqtS8jdbf6sg1iouEfsOCNrK\u002BuX16vdoK/7UnejGUyAttiVnavZsV2NEunzCAGlefCUWxl3SPJP0TgLr3YlTlv2gArAOtV2OsoLeS6eiXAjrKb\u002BOrtfEs5fPNOgSVKbVp9VdLohcy/sP6jBY0RmJSVZ/AEmBuMlN32vZ0G8mKzD6aQo1NLMRh7d2tbqHNXGStowofmijw4yUUrB8v0EEHUw0V5ns4iorxkx6oJlGhupwfRsZ6iOk3Odm6ionLF6DfSnGGhQtUQj00lDqNxSr/v8igdXUJNJmZl3dq1xOsCsLIKg\u002BXViw3mJ72aD7Mh9iyg0T95vDw9Y\u002BcErZ5B4Z/HZcgivKX9M0zMo9chiRlBF0N3itIsqtyD1qgwVAsTWKIF33ZeCIftiByevul2\u002BXYJ/8477UxSUYFV8wxt9UyN\u002B5x5BXTgRcaavz0gfCF3K5qX3yKSXAyqJ2K41ihc9XRKrTRn9ZTyDO3b8nZa7MS1CNgeyzGBNHOVXIpx9RQM\u002BXZFJA1dQRVS23y\u002B0O4lbkX93o2jj3x2s8QNhpXVqzcwZYhQCwZzeY1DjWcLQCQaovU1yXryKTIfgvnJ1p76h6Awo0Kqzfwe55kq9K050wUsPZfE0gL83p5szwruOWfP7i98d7MzA8FYsaAVPJWnP\u002BMVYL\u002BFPgYRwKI0oxehDG1YS8MepbCd\u002Bspf04isJHclujhoQj5p\u002BEUmVFBRIPpBFfTgHteoqDbfq3ycuflhFc06rXnEp2C2N0/3YrsTgdCakJ65JF\u002Bge4ozcUXWymHmwEMzrM0SAAd0\u002B6Oi20GPdv8APHhmHPhHuHluprRu0VhcNGmYE8I8rI\u002BghtspEFFV5lgFj3FUGxnppXXLXajD6mK4tuUDkfnFa3Hl4qzwd5JspvZi8uGgZ29PFdxtKgmzCABaOT5dOF4STrdqBLhK5g30ixdk47VOGmMY4psL7JjK0lnNJcrx2N0SYfJ5AGhUVkqm9rbVpC4LXt/nu/x7kS6xlWTItmdXQr9ahoT3/9ngwR02aD\u002BmpvaHqJq\u002B5lkmiy3zgR4X170GNbl607BSf8LWaYlU6e7RrcA/Nv3B5Bqj7W3Fasrsh3NqFdE3mRMi59S/PimYwprNlOLh0sQ3EA\u002BVfHpoDicgQiuXcCZQBV\u002BtOVGOiY98MPSmXLYSIaNR9gpzpTZswcaPKJ30dmoQ\u002BFGpYMJf6Nyx18TtMioUoUrU22ryW7Idxh5EOwL34HOt9Xrq\u002BxJBvKNW1cH38JqYXb\u002BQcB4M31ljn0IN9pRdWdigrk4U31M0\u002BysA2lYfsWJLGyersvvuWvTB6TnIeJT77bXYSGwquIlU1M/\u002BmFpFK\u002BMRXCOXhFv1d2cfj5CkhnM7lp434HZDZ1FZk2DFz2MNWQb9Co2P3tLtkRaxba\u002BOM\u002BczNH7ZO8Yczb9pV7hSmvTNzKAM3fIKwHWru0zE\u002BNWOVrFlZr7bZZvDkcF7k/bbGcAMSQ1vwTNa\u002BebH7QXwabpuNKI/maitntoZDjmGS5p6nEDKJ8U6XJZnQ1Z81XbT2NStaa4F06Jq4xN3fKt/2MLRKOu2P9qo5eUH1KbW3qu\u002BNVZfuZ3dFAADufJQP7PEQZXIC0FZdEzYuAUPtOzDmaCrACXz5KCZSsB5KinaZrVfvO2uQy7mdMd1SsuQVDc3F\u002B950TowRgq5eSGVxJD6v5ryP\u002BMUE3czaxnaAY43xXflZPjbTVNbaLAG9I4xpF6q3KYY5";
            byte[] saltBytes = Convert.FromBase64String(pwSalt);
            byte[] privKeyBytes = Convert.FromBase64String(privCrypt);

            int runs = 100;

            Stopwatch stopwatch = new Stopwatch();
            Console.WriteLine("Running PBKDF2 based symmetric decryption test ...");
            stopwatch.Start();
            for (int i = 0; i < runs; i++)
            {
                RFC2898Helper.DecryptWithDerivedKey(pw, pwSalt, privCrypt, null);
            }
            stopwatch.Stop();
            Console.WriteLine($"Finished! Elapsed ms: {stopwatch.ElapsedMilliseconds}\n");

            byte[] protectedData = ProtectedData.Protect(privKeyBytes, saltBytes, DataProtectionScope.CurrentUser);
            stopwatch.Reset();
            Console.WriteLine("Running ProtectedData.Unprotect based decryption test ...");
            stopwatch.Start();
            for (int i = 0; i < runs; i++)
            {
                ProtectedData.Unprotect(protectedData, saltBytes, DataProtectionScope.CurrentUser);
            }
            stopwatch.Stop();
            Console.WriteLine($"Finished! Elapsed ms: {stopwatch.ElapsedMilliseconds}\n");

            /// Test Result for 100 runs:
            /// PBKDF2 decryption:          ~2110 ms
            /// ProtectedData.Unprotect      25 ms

            Console.ReadKey();
        }

        public static void MainOld(string[] args)
        {
            LocalLog localLog = new LocalLog();

            string pw = "asdf";
            string pwSalt = "h7/o0xRJO/0iybOHaGv/1d9uj8k=";
            (string pub, string privCrypt) = RSAHelper.CreateRSAKeyPair(pw, pwSalt);

            localLog.Log = new StringBuilder();
            localLog.Logline("\nBEGIN LOG");
            localLog.Logline("Public RSA key");
            localLog.Logline(pub);
            RFC2898Helper.DecryptWithDerivedKey(pw, pwSalt, privCrypt, null);
            Console.WriteLine(localLog.Log.ToString());
            Console.ReadKey();
        }

        public static string BytesToString(byte[] val)
        {
            string result = "";
            foreach (var v in val)
                result += $"{v.ToString("X").PadLeft(2)} ";
            return result;
        }
    }

    public class LocalLog
    {
        public StringBuilder Log { get; set; }

        public void Logline(string line)
        {
            Log.AppendLine(line);
        }

        public LocalLog() { Log = new StringBuilder(); }
    }
}
