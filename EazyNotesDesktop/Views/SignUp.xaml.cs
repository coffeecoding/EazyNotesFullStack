using EazyNotesDesktop.Util;
using EazyNotesDesktop.ViewModels;
using Prism.Regions;
using System.Windows;
using System.Windows.Controls;

namespace EazyNotesDesktop.Views
{
    public partial class SignUp : UserControl
    {
        private IRegionManager _regionManager { get; }

        public SignUp(IRegionManager regionManager)
        {
            InitializeComponent();
            _regionManager = regionManager;
            (DataContext as SignUpViewModel).RequestShowAlertBox += (s, e) =>
                new AlertBox(GetParentWindow(), s as AlertBoxArgs).Show();
        }

        private Window GetParentWindow()
        {
            // This is the Shell Window
            return Application.Current.Windows[0];
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            SignUpViewModel vm = DataContext as SignUpViewModel;
            vm.FieldPassword = PasswordBox.Password;
        }

        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            SignUpViewModel vm = DataContext as SignUpViewModel;
            vm.FieldConfirmPassword = ConfirmPasswordBox.Password;
        }

        private void TextBoxEmail_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            // This ensures that pasting text into the textbox updates the underlying property from the vm
            SignUpViewModel vm = DataContext as SignUpViewModel;
            vm.FieldEmail = TextBoxEmail.Text;
        }
    }
}
