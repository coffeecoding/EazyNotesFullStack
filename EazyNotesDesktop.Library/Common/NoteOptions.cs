namespace EazyNotesDesktop.Library.Common
{
    public class NoteOptions
    {
        public const char SEPARATOR = ';';
        public const char WRAP_TEXT = 'W';

        public static void Add(ref string options, object flag) {
            if (options == null)
                options = "";
            else if (options.Contains(flag.ToString()))
                return;
            options += flag.ToString() + SEPARATOR;
        }

        public static void Remove(ref string options, object flag) {
            if (options == null || !options.Contains(flag.ToString()))
                return;
            options = options.Replace($"{flag}{SEPARATOR}", "");
        }
    }
}
