using System;

namespace Sanford.Multimedia
{
    public class ErrorEventArgs : EventArgs
    {
        public ErrorEventArgs(Exception ex)
        {
            Error = ex;
        }

        public Exception Error { get; }
    }
}