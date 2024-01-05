using Prism.Mvvm;
using System.Collections.ObjectModel;

namespace EazyNotesDesktop.ViewModels
{
    public class SearchResultsViewModel : BindableBase
    {
        public SearchResultsViewModel()
        {
            NoteViewModels = new ObservableCollection<BaseNoteViewModel>();
        }

        private ObservableCollection<BaseNoteViewModel> _noteViewModels;
        public ObservableCollection<BaseNoteViewModel> NoteViewModels
        {
            get { return _noteViewModels; }
            set { SetProperty(ref _noteViewModels, value); }
        }

        private BaseNoteViewModel _selectedNoteVM;
        public BaseNoteViewModel SelectedNoteVM
        {
            get { return _selectedNoteVM; }
            set { SetProperty(ref _selectedNoteVM, value); }
        }

        private string _searchTerm;
        public string SearchTerm
        {
            get { return _searchTerm; }
            set { SetProperty(ref _searchTerm, value); }
        }
    }
}
