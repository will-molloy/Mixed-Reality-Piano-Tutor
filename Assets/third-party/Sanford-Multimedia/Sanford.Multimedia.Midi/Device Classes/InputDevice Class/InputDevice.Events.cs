using System;

namespace Sanford.Multimedia.Midi
{
    public delegate void MidiMessageEventHandler(IMidiMessage message);

    public partial class InputDevice
    {
        /// <summary>
        ///     Gets or sets a value indicating whether the midi events should be posted on the same synchronization context as the
        ///     device constructor was called.
        ///     Default is <c>true</c>. If set to <c>false</c> the events are fired on the driver callback or the thread of the
        ///     driver callback delegate queue, depending on the PostDriverCallbackToDelegateQueue property.
        /// </summary>
        /// <value>
        ///     <c>true</c> if midi events should be posted on the same synchronization context as the device constructor was
        ///     called; otherwise, <c>false</c>.
        /// </value>
        public bool PostEventsOnCreationContext { get; set; }

        /// <summary>
        ///     Occurs when any message was received. The underlying type of the message is as specific as possible.
        ///     Channel, Common, Realtime or SysEx.
        /// </summary>
        public event MidiMessageEventHandler MessageReceived;

        public event EventHandler<ShortMessageEventArgs> ShortMessageReceived;

        public event EventHandler<ChannelMessageEventArgs> ChannelMessageReceived;

        public event EventHandler<SysExMessageEventArgs> SysExMessageReceived;

        public event EventHandler<SysCommonMessageEventArgs> SysCommonMessageReceived;

        public event EventHandler<SysRealtimeMessageEventArgs> SysRealtimeMessageReceived;

        public event EventHandler<InvalidShortMessageEventArgs> InvalidShortMessageReceived;

        public event EventHandler<InvalidSysExMessageEventArgs> InvalidSysExMessageReceived;

        protected virtual void OnShortMessage(ShortMessageEventArgs e)
        {
            var handler = ShortMessageReceived;

            if (handler != null)
            {
                if (PostEventsOnCreationContext)
                    context.Post(delegate { handler(this, e); }, null);
                else
                    handler(this, e);
            }
        }

        protected void OnMessageReceived(IMidiMessage message)
        {
            var handler = MessageReceived;

            if (handler != null)
            {
                if (PostEventsOnCreationContext)
                    context.Post(delegate { handler(message); }, null);
                else
                    handler(message);
            }
        }

        protected virtual void OnChannelMessageReceived(ChannelMessageEventArgs e)
        {
            var handler = ChannelMessageReceived;

            if (handler != null)
            {
                if (PostEventsOnCreationContext)
                    context.Post(delegate { handler(this, e); }, null);
                else
                    handler(this, e);
            }
        }

        protected virtual void OnSysExMessageReceived(SysExMessageEventArgs e)
        {
            var handler = SysExMessageReceived;

            if (handler != null)
            {
                if (PostEventsOnCreationContext)
                    context.Post(delegate { handler(this, e); }, null);
                else
                    handler(this, e);
            }
        }

        protected virtual void OnSysCommonMessageReceived(SysCommonMessageEventArgs e)
        {
            var handler = SysCommonMessageReceived;

            if (handler != null)
            {
                if (PostEventsOnCreationContext)
                    context.Post(delegate { handler(this, e); }, null);
                else
                    handler(this, e);
            }
        }

        protected virtual void OnSysRealtimeMessageReceived(SysRealtimeMessageEventArgs e)
        {
            var handler = SysRealtimeMessageReceived;

            if (handler != null)
            {
                if (PostEventsOnCreationContext)
                    context.Post(delegate { handler(this, e); }, null);
                else
                    handler(this, e);
            }
        }

        protected virtual void OnInvalidShortMessageReceived(InvalidShortMessageEventArgs e)
        {
            var handler = InvalidShortMessageReceived;

            if (handler != null)
            {
                if (PostEventsOnCreationContext)
                    context.Post(delegate { handler(this, e); }, null);
                else
                    handler(this, e);
            }
        }

        protected virtual void OnInvalidSysExMessageReceived(InvalidSysExMessageEventArgs e)
        {
            var handler = InvalidSysExMessageReceived;

            if (handler != null)
            {
                if (PostEventsOnCreationContext)
                    context.Post(delegate { handler(this, e); }, null);
                else
                    handler(this, e);
            }
        }
    }
}