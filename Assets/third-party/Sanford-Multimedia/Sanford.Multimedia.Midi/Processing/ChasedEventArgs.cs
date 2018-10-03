using System;
using System.Collections;

namespace Sanford.Multimedia.Midi
{
    public class ChasedEventArgs : EventArgs
    {
        public ChasedEventArgs(ICollection messages)
        {
            Messages = messages;
        }

        public ICollection Messages { get; }
    }
}