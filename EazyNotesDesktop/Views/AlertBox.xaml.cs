using EazyNotesDesktop.Util;
using System.Drawing;
using System.Windows;

namespace EazyNotesDesktop.Views
{
    public partial class AlertBox : Window
    {
        public string Caption { get; }
        public string Message { get; }
        public string Symbol { get; private set; }
        public AlertButton Buttons 
        { 
            set
            {
                switch (value)
                {
                    case AlertButton.OK:
                        OKBtn.Visibility = Visibility.Visible;
                        YesBtn.Visibility = Visibility.Collapsed;
                        NoBtn.Visibility = Visibility.Collapsed;
                        CancelBtn.Visibility = Visibility.Collapsed;
                        break;
                    case AlertButton.OKCancel:
                        OKBtn.Visibility = Visibility.Visible;
                        YesBtn.Visibility = Visibility.Collapsed;
                        NoBtn.Visibility = Visibility.Collapsed;
                        CancelBtn.Visibility = Visibility.Visible;
                        break;
                    case AlertButton.YesNo:
                        OKBtn.Visibility = Visibility.Collapsed;
                        YesBtn.Visibility = Visibility.Visible;
                        NoBtn.Visibility = Visibility.Visible;
                        CancelBtn.Visibility = Visibility.Collapsed;
                        break;
                    case AlertButton.YesNoCancel:
                        OKBtn.Visibility = Visibility.Collapsed;
                        YesBtn.Visibility = Visibility.Visible;
                        NoBtn.Visibility = Visibility.Visible;
                        CancelBtn.Visibility = Visibility.Visible;
                        break;
                }
            }
        }
        public MessageBoxResult Result { get; private set; }

        public AlertBox(Window owner, AlertBoxArgs args)
        {
            DataContext = this;
            Owner = owner;
            Caption = args.Caption;
            Message = args.Message;
            Symbol = args.Image.ToString();
            InitializeComponent();
            Buttons = args.Button;
        }

        private void OKBtn_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.OK;
            Close();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.Cancel;
            Close();
        }

        private void NoBtn_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.No;
            Close();
        }

        private void YesBtn_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.Yes;
            Close();
        }
    }
}
