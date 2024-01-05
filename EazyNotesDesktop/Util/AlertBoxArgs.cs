using System;
using System.Windows;

namespace EazyNotesDesktop.Util
{
    public enum AlertImage
    {
        Error,
        Information,
        Warning,
        Question
    }

    public enum AlertButton
    {
        OK,
        OKCancel,
        YesNo,
        YesNoCancel
    }

    public class AlertBoxArgs
    {
        public string Caption { get; }
        public string Message { get; }
        public AlertImage Image { get; }
        public AlertButton Button { get; }

        public AlertBoxArgs(string message, 
            AlertImage img = AlertImage.Information,
            AlertButton button = AlertButton.OK)
        {
            Message = message;
            Caption = img.ToString();
            Image = img;
            Button = button;
        }

        public AlertBoxArgs(Exception exception,
            AlertImage img = AlertImage.Error,
            AlertButton button = AlertButton.OK)
        {
            Message = exception.ToString();
            Caption = img.ToString();
            Image = img;
            Button = button;
        }
    }
}
