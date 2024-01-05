using System.IO;

namespace EazyNotesDesktop.Library.Common
{
    public class PathUtil
    {
        public static string RemoveInvalidChars(string filename)
            => string.Concat(filename.Split(Path.GetInvalidFileNameChars()));
    }
}
