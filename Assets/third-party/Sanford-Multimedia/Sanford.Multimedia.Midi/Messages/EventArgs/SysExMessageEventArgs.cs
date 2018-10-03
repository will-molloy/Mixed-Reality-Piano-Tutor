using System;

namespace Sanford.Multimedia.Midi
{
    public class SysExMessageEventArgs : EventArgs
    {
        public SysExMessageEventArgs(SysExMessage message)
        {
            Message = message;
        }

        public SysExMessage Message { get; }
    }
}