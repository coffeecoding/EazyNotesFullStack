using System.Windows;
using System.Windows.Media.Effects;

namespace EazyNotesDesktop.Views.Util
{
    public class UIUtils
    {
        public static void BlurDarkenBackground(Window window)
        {
            window.Opacity = 0.5;
            window.Effect = new BlurEffect();
        }

        public static void UndoBlurDarkenBackground(Window window)
        {
            window.Effect = null;
            window.Opacity = 1;
        }

        public static void ShowSpinnerAndBlurBackground(Spinner spinner, Window parent)
        {
            BlurDarkenBackground(parent);
            spinner.Show();
        }

        public static void HideSpinnerAndUndoBlurBackground(Spinner spinner, Window parent)
        {
            spinner?.Hide();
            UndoBlurDarkenBackground(parent);
        }
    }
}
