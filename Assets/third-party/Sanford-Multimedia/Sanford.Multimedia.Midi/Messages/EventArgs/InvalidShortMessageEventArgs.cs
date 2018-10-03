using System;

namespace Sanford.Multimedia.Midi
{
    public class InvalidShortMessageEventArgs : EventArgs
    {
        public InvalidShortMessageEventArgs(int message)
        {
            Message = message;
        }

        public int Message { get; }
    }
}