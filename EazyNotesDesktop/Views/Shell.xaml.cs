using Prism.Regions;
using System.Windows;

namespace EazyNotesDesktop.Views
{
    public partial class Shell : Window
    {
        private IRegionManager _regionManager { get; }

        public Shell(IRegionManager regionManager)
        {
            InitializeComponent();
            _regionManager = regionManager;
            _regionManager.RegisterViewWithRegion("ContentRegion", typeof(Login));
            _regionManager.RegisterViewWithRegion("ContentRegion", typeof(SignUp));

            Closed += (s, e) =>
            {
                App.Current.Shutdown();
            };
        }
    }
}
