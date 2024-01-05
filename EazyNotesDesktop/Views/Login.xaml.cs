using EazyNotesDesktop.Util;
using EazyNotesDesktop.ViewModels;
using Prism.Regions;
using System.Windows;
using System.Windows.Controls;

namespace EazyNotesDesktop.Views
{
    public partial class Login : UserControl
    {
        IRegionManager _regionManager;

        public Login(IRegionManager regionManager)
        {
            _regionManager = regionManager;
            InitializeComponent();
            AddCloseEventHandler();
            TxtBoxUsername.Text = "";
            PwBox.Password = "";
            (DataContext as LoginViewModel).RequestShowAlertBox += (s,e) =>
                new AlertBox(GetParentWindow(), s as AlertBoxArgs).Show();
        }

        private void AddCloseEventHandler()
        {
            (DataContext as LoginViewModel).RequestParentClose += (s, e) =>
            {
                PwBox.Password = "";
                GetParentWindow().Hide();
            };
        }

        private Window GetParentWindow()
        {
            // This is the Shell Window
            return Application.Current.Windows[0];
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            (DataContext as LoginViewModel).Password = PwBox.Password;
        }
    }
}
