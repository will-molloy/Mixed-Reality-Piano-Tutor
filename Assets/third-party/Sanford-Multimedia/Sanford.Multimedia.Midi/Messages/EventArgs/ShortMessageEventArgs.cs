using System;

namespace Sanford.Multimedia.Midi
{
    /// <summary>
    ///     Raw short message as int or byte array, useful when working with VST.
    /// </summary>
    public class ShortMessageEventArgs : EventArgs
    {
        public ShortMessageEventArgs(ShortMessage message)
        {
            Message = message;
        }

        public ShortMessageEventArgs(int message, int timestamp = 0)
        {
            Message = new ShortMessage(message);
            Message.Timestamp = timestamp;
        }

        public ShortMessageEventArgs(byte status, byte data1, byte data2)
        {
            Message = new ShortMessage(status, data1, data2);
        }

        public ShortMessage Message { get; }

        public static ShortMessageEventArgs FromChannelMessage(ChannelMessageEventArgs arg)
        {
            return new ShortMessageEventArgs(arg.Message);
        }

        public static ShortMessageEventArgs FromSysCommonMessage(SysCommonMessageEventArgs arg)
        {
            return new ShortMessageEventArgs(arg.Message);
        }

        public static ShortMessageEventArgs FromSysRealtimeMessage(SysRealtimeMessageEventArgs arg)
        {
            return new ShortMessageEventArgs(arg.Message);
        }
    }
}