using EazyNotes.Common;
using System;

namespace EazyNotesDesktop.Library.Common
{
    public class DataChangedEventArgs : EventArgs
    {
        public object ChangeData { get; }
        public SyncAction SyncAction { get; set; }

        public DataChangedEventArgs(SyncAction syncAction, object changeData)
        {
            SyncAction = syncAction;
            ChangeData = changeData;
        }
    }
}
