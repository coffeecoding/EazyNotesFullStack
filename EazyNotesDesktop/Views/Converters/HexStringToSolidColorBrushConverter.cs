using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace EazyNotesDesktop.Views
{
    public class HexStringToSolidColorBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string hex = value.ToString();
            byte[] argb = StringToByteArray(hex.StartsWith('#') ? hex[1..] : hex);
            return new SolidColorBrush(Color.FromArgb(argb[0], argb[1], argb[2], argb[3]));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Brushes.Gray;
            string hex = value.ToString();
            byte[] argb = StringToByteArray(hex.StartsWith('#') ? hex[1..] : hex);
            return new SolidColorBrush(Color.FromArgb(argb[0], argb[1], argb[2], argb[3]));
            //SolidColorBrush color = value as SolidColorBrush;
            //return BitConverter.ToString(new byte[] { color.Color.A, 
            //    color.Color.R, color.Color.G, color.Color.B }).Replace("-", "");
        }

        //https://stackoverflow.com/a/311179/12213872
        private byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = System.Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
    }
}
