using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using EazyNotesDesktop.Util;
using EazyNotesDesktop.ViewModels;
using EazyNotesDesktop.Views.Util;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace EazyNotesDesktop.Views
{
    public partial class Settings : Window
    {
        private Spinner SpinnerDlg;

        public Settings(SettingsViewModel settingsVM)
        {
            DataContext = settingsVM;
            SettingsViewModel vm = DataContext as SettingsViewModel;

            vm.RequestShowAlertBox += (s, e) =>
            {
                AlertBox alertBox = new AlertBox(this.IsActive ? this : null, s as AlertBoxArgs);
                alertBox.ShowDialog();
            };
            vm.RequestShowSpinner += (s, e) =>
            {
                UIUtils.BlurDarkenBackground(this);
                SpinnerDlg = new Spinner(s.ToString());
                SpinnerDlg.Show();
            };
            vm.RequestCloseSpinner += (s, e) =>
            {
                SpinnerDlg?.Hide();
                UIUtils.UndoBlurDarkenBackground(this);
            };

            Closing += (s, e) => {
                e.Cancel = true;
                this.Hide();
            };
            InitializeComponent();
        }

        private async void Logout_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Refactor this flow of control: This caused the Logout method to be called twice:
            // 1. User clicks on Logout button -> Logout() is called
            // 2. When Owner.Close() is called, wherebey Owner is MainWindow, its Closing eventhandler also calls Logout
            // => temporary solution: Comment out the following line
            //(DataContext as SettingsViewModel).Logout();
            Close();
            Owner.Close();
            GetShellWindow().Show();
        }

        private Window GetShellWindow()
        {
            // This is the Shell Window
            return Application.Current.Windows[0];
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            (DataContext as SettingsViewModel).SaveSettings();
            Close();
        }
        public delegate void ImportNotesDelegate(List<string> filenames);

        private async void ImportBtn_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new CommonOpenFileDialog()
            {
                EnsurePathExists = true,
                IsFolderPicker = true,
                Multiselect = true
            };
            CommonFileDialogResult dlgResult = dlg.ShowDialog();
            if (dlgResult == CommonFileDialogResult.Ok)
            {
                SettingsViewModel settingsVM = (DataContext as SettingsViewModel);
                await settingsVM.ImportNotes(dlg.FileNames.ToList());
            }
        }

        private void SelectExportPathBtn_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new CommonOpenFileDialog()
            {
                EnsurePathExists = true,
                IsFolderPicker = true,
            };
            CommonFileDialogResult dlgResult = dlg.ShowDialog();
            if (dlgResult == CommonFileDialogResult.Ok)
            {
                SettingsViewModel settingsVM = (DataContext as SettingsViewModel);
                settingsVM.SetExportPath(dlg.FileName);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string msg = EazyNotes.Language.Resources.ErrorNoInternet;
            AlertBox alert = new AlertBox(this, sender as AlertBoxArgs);
            alert.ShowDialog();
        }
    }
}
