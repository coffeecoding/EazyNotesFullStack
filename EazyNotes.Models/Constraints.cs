using System;
using System.Collections.Generic;
using System.Text;

namespace EazyNotes.Models
{
    public static class Constraints
    {
        public static int CLIENT_DEVICENAME_MAXLEN = 127;
        public static int CLIENT_PLATFORM_MAXLEN = 63;
        public static int CLIENT_COUNTRY_MAXLEN = 63;

        public static int FEEDBACK_TITLE_MAXLEN = 95;
        public static int FEEDBACK_BODY_MAXLEN = 4095;
        public static ushort FEEDBACK_CATEGORY_MAX = 2;
        public static int FEEDBACK_APPVERSION_MAXLEN = 32;
        public static int FEEDBACK_ADDRESSEDINVERSION_MAXLEN = 32;
        public static int FEEDBACK_DEVICENAME_MAXLEN = 127;
        public static int FEEDBACK_PLATFORM_MAXLEN = 63;

        public static int NOTE_TITLE_MAXLEN = 511;
        public static int NOTE_OPTIONS_MAX_LEN = 127;
        public static int NOTE_IVKEY_MAXLEN = 511;
        public static ushort[] NOTE_VALIDE_NOTETYPES = new ushort[] { 0, 1 };
        public static bool NOTE_ISVALIDTYPE(ushort type) => type == 0 || type == 1;

        public static int TOPIC_TITLE_MAXLEN = 511;
        public static int TOPIC_SYMBOL_MAXLEN = 4;
        public static int TOPIC_IVKEY_MAXLEN = 511;
        public static int TOPIC_COLOR_MAXLEN = 31;
        public static int TOPIC_POSITION_MAX = 65535;

        public static int USER_USERNAME_MAXLEN = 255;
        public static int USER_DISPLAYNAME_MAXLEN = 255;
        public static int USER_EMAIL_MAXLEN = 255;
        public static int USER_PASSWORDSALT_MAXLEN = 127;
        public static int USER_PASSWORDHASH_MAXLEN = 127;
        public static int USER_RSAPUBKEY_MAXLEN = 2047;
        public static int USER_RSAPRIVKEY_MAXLEN = 4095;
        public static int USER_ALGORITHMIDENTIFIER_MAXLEN = 511;
        static readonly string VALID_CHARS = "abcdefghijklmnopqrstuvxyzäöüABCDEFGHIJKLMNOPQRSTUVWXYZÄÖÜ";
        static readonly string VALID_OTHERS = "_0123456789";
        public static bool USER_USERNAME_ISVALID(string username, out string message)
        {
            if (VALID_CHARS.IndexOf(username[0]) < 0)
            {
                message = "has to begin with a character";
                return false;
            }
            foreach (char c in username)
                if (!(VALID_CHARS.IndexOf(username[0]) >= 0 || VALID_OTHERS.IndexOf(c) >= 0))
                {
                    message = "may only contain characters, numbers and _";
                    return false;
                }
            message = "";
            return true;
        }
            
    }
}
