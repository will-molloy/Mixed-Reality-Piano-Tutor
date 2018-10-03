using System;

namespace Sanford.Multimedia.Midi
{
    public class SysCommonMessageEventArgs : EventArgs
    {
        public SysCommonMessageEventArgs(SysCommonMessage message)
        {
            Message = message;
        }

        public SysCommonMessage Message { get; }
    }
}