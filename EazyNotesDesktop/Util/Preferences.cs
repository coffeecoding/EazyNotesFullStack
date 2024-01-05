using EazyNotesDesktop.Library.Common;
using System.Collections.Generic;

namespace EazyNotesDesktop.Util
{
    public class Preferences
    {
        public Theme Theme { get; set; }
        public List<long> CollectionsOrder { get; set; }
        public bool SaveImportedNotesInstantly { get; set; }
        public bool SelectedImportIntoCurrentTopic { get; set; }
        public bool SelectedUseExistingOrCreateNewTopic { get; set; }
        public bool ShowTopicTitles { get; set; }
        public NoteSortKey NoteSortKey { get; set; }

        public static Preferences GetDefaultSettings()
        {
            return new Preferences
            {
                Theme = Theme.Dark,
                SaveImportedNotesInstantly = true,
                SelectedImportIntoCurrentTopic = true,
                SelectedUseExistingOrCreateNewTopic = false,
                ShowTopicTitles = true,
                NoteSortKey = NoteSortKey.NoteSortKeys[3]
            };
        }
    }
}
