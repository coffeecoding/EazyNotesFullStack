using MvvmHelpers.Commands;
using System;

namespace EazyNotesDesktop.ViewModels
{
    public enum EntityState
    {
        Default,
        Modified,
        New
    }

    public enum EntityType
    {
        SimpleNote,
        CheckListNote,
        Topic
    }

    public interface IEntityViewModel
    {
        string PersistedTitle { get; }
        string PersistedSymbol { get; }
        string PersistedColor { get; }
        DateTime? DateDeleted { get; }

        public AsyncCommand TrashUntrashCommand { get; set; }
        public AsyncCommand DeleteCommand { get; set; }
    }
}
