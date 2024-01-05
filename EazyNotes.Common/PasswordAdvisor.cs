﻿using System.Text.RegularExpressions;

namespace EazyNotes.Common
{
    public class PasswordAdvisor
    {
        public enum PasswordScore
        {
            Blank = 0,
            VeryWeak = 1,
            Weak = 2,
            Medium = 3,
            Strong = 4,
            VeryStrong = 5
        }

        public static PasswordScore CheckStrength(string password)
        {
            int score = 0;

            if (password.Length < 1)
                return PasswordScore.Blank;
            if (password.Length < 4)
                return PasswordScore.VeryWeak;
            if (password.Length >= 8)
                score++;
            if (password.Length >= 12)
                score++;
            if (Regex.IsMatch(password, @"\d+", RegexOptions.ECMAScript))
                score++;
            if (Regex.IsMatch(password, @"[a-z]", RegexOptions.ECMAScript) &&
              Regex.IsMatch(password, @"[A-Z]", RegexOptions.ECMAScript))
                score++;
            if (Regex.IsMatch(password, @".[!,@,#,$,%,^,&,*,?,_,~,-,£,(,)]", RegexOptions.ECMAScript))
                score++;

            return (PasswordScore)score;
        }
    }
}
