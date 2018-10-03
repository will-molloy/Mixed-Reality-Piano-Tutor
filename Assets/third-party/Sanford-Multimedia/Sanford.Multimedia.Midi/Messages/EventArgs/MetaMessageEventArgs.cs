using System;

namespace Sanford.Multimedia.Midi
{
    public class MetaMessageEventArgs : EventArgs
    {
        public MetaMessageEventArgs(MetaMessage message)
        {
            Message = message;
        }

        public MetaMessage Message { get; }
    }
}