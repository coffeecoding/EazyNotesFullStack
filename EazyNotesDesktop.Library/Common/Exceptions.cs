using System;
using System.Collections.Generic;
using System.Text;

namespace EazyNotesDesktop.Library.Common
{
    public class InvalidPathException : Exception 
    {
        public InvalidPathException(string message) : base(message) { }
    }

    public class NotFullyQualifiedPathException : Exception
    {

    }
}
