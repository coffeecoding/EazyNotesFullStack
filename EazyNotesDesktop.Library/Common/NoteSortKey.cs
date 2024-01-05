using System.Collections.Generic;

namespace EazyNotesDesktop.Library.Common
{
    public enum SortKey
    {
        Title,
        TitleDescending,
        LastEdited,
        LastEditedDescending,
        Created,
        CreatedDescending
    }

    public class NoteSortKey
    {
        public SortKey SortKey { get; set; }
        public string Name { get; set; }

        public NoteSortKey()
        {
            // for Json deserialization
        }

        NoteSortKey(SortKey key, string name)
        {
            SortKey = key; Name = name;
        }

        private static List<NoteSortKey> _noteSortKeys;
        public static List<NoteSortKey> NoteSortKeys => _noteSortKeys ??= new List<NoteSortKey>
        {
            new NoteSortKey(SortKey.Title, "Sort notes by Title"),
            new NoteSortKey(SortKey.TitleDescending, "Sort notes by Title descending"),
            new NoteSortKey(SortKey.LastEdited, "Sort notes by Time last edited"),
            new NoteSortKey(SortKey.LastEditedDescending, "Sort notes by Time last edited descending"),
            new NoteSortKey(SortKey.Created, "Sort notes by Time created"),
            new NoteSortKey(SortKey.CreatedDescending, "Sort notes by Time created descending"),
        };

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            NoteSortKey other = obj as NoteSortKey;
            if (other == null)
                return false;
            return other.SortKey.Equals(SortKey) && other.Name.Equals(Name);
        }
    }
}
