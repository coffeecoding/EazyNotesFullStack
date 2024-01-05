using System;
using System.Collections.Generic;
using System.Text;

namespace EazyNotes.Common
{
    public class ColorRef
    {
        public static List<string> FlutterColors = new List<string>()
        {
            "fff44336", "ffe91e63", "ff9c27b0", "ff673ab7", "ff3f51b5", "ff2196f3", "ff03a9f4", "ff00bcd4", "ff009688", "ff4caf50",
            "ff8bc34a", "ffcddc39", "ffffeb3b", "ffffc107", "ffff9800", "ffff5722", "ff795548", "ff9e9e9e", "ff607d8b", "ff000000"
        };

        public static List<string> Colors => FlutterColors;

        public static string DefaultTopicColor => Colors[0];

        public static bool HasColor(string color) => FlutterColors.Contains(color.StartsWith('#') ? color.Substring(1).ToLower() : color.ToLower());

        public static bool AreColorsEqual(string c1, string c2)
        {
            if (c1.StartsWith('#')) c1 = c1.Substring(1);
            if (c2.StartsWith('#')) c2 = c2.Substring(1);
            return c1.ToUpper().Equals(c2.ToUpper());
        }
    }
}
