using System;

namespace Sanford.Multimedia.Midi
{
    public class ChannelMessageEventArgs : EventArgs
    {
        public ChannelMessageEventArgs(ChannelMessage message)
        {
            Message = message;
        }

        public ChannelMessage Message { get; }
    }
}