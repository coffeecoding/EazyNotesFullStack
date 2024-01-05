using EazyNotesDesktop.Util;
using EazyNotesDesktop.ViewModels;
using EazyNotesDesktop.Views.Util;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace EazyNotesDesktop.Views
{
    public partial class TrashBin : UserControl
    {
        private Spinner SpinnerDlg;

        public TrashBin()
        {
            InitializeComponent();

            Loaded += (s, e) =>
            {
                TrashBinViewModel vm = DataContext as TrashBinViewModel;
                // After 2 hours of trying, this seems to be the best possible way to ensure the eventhandlers are only added once3
                if (vm == null)
                    return;
                if (vm.GetRequestCloseSpinnerHandlerCount() > 0)
                    return;
                Window parent = GetParentWindow();
                vm.RequestShowSpinner += (s, e) =>
                {
                    SpinnerDlg = new Spinner(s.ToString());
                    UIUtils.ShowSpinnerAndBlurBackground(SpinnerDlg, parent);
                };
                vm.RequestCloseSpinner += (s, e) => UIUtils.HideSpinnerAndUndoBlurBackground(SpinnerDlg, parent);
            };
        }

        private Window GetParentWindow()
        {
            FrameworkElement parentCandidate = Parent as FrameworkElement;
            while (!(parentCandidate is Window))
            {
                parentCandidate = parentCandidate.Parent as FrameworkElement;
            }
            return parentCandidate as Window;
        }

        // Called 1st: DataContext is null
        private void UserControl_Initialized(object sender, System.EventArgs e)
        {
        }

        // Called 2nd as well as after IsVisibleChanged
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
        }

        // Called 3rd
        private void TrashBin_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == true)
            {
                var uiContext = SynchronizationContext.Current;
                TrashBinViewModel vm = DataContext as TrashBinViewModel;
                if (vm.IsDataDownloaded)
                {
                    vm.RefreshTrashedDataFromRepo();
                    return;
                }
                Window parent = GetParentWindow();
                SpinnerDlg = new Spinner("Loading ...");
                UIUtils.ShowSpinnerAndBlurBackground(SpinnerDlg, parent);
                vm.FetchTrashedItems().Await(() =>
                {
                    // https://stackoverflow.com/a/9732853/12213872
                    Dispatcher.Invoke(() =>
                    {
                        UIUtils.HideSpinnerAndUndoBlurBackground(SpinnerDlg, parent);
                        vm.MainVM.SettingsViewModel.IsNotBusy = true;
                    });
                },
                (e) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        new AlertBox(GetParentWindow(), new AlertBoxArgs(e)).ShowDialog();
                        UIUtils.HideSpinnerAndUndoBlurBackground(SpinnerDlg, parent);
                        vm.MainVM.SettingsViewModel.IsNotBusy = true;
                    });
                });
            }
        }

        // Called 4th
        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
        }
    }
}
