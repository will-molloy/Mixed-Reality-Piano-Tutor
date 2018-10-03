using System;
using System.Collections;

namespace Sanford.Multimedia.Midi
{
    public class StoppedEventArgs : EventArgs
    {
        public StoppedEventArgs(ICollection messages)
        {
            Messages = messages;
        }

        public ICollection Messages { get; }
    }
}