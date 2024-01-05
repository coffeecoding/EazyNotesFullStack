using EazyNotesDesktop.ViewModels;
using System.Windows;

namespace EazyNotesDesktop.Views
{
    public partial class SearchResults : Window
    {
        private MainViewModel MainVM;

        public SearchResults(MainViewModel mainVM, SearchResultsViewModel searchResultsViewModel)
        {
            InitializeComponent();
            DataContext = searchResultsViewModel;
            MainVM = mainVM;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void SearchResultsList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SearchResultsViewModel searchResultsVM = DataContext as SearchResultsViewModel;
            MainVM.HandleSelectedNoteChanged(searchResultsVM.SelectedNoteVM);
            Close_Click(this, null);
        }
    }
}
