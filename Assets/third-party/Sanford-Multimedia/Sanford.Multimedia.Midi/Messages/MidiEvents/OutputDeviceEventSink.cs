using System;

namespace Sanford.Multimedia.Midi
{
    /// <summary>
    ///     Event sink that sends midi messages to an output device
    /// </summary>
    public class OutputDeviceEventSink : IDisposable
    {
        private readonly MidiEvents FEventSource;
        private readonly OutputDevice FOutDevice;

        public OutputDeviceEventSink(OutputDevice outDevice, MidiEvents eventSource)
        {
            FOutDevice = outDevice;
            FEventSource = eventSource;

            RegisterEvents();
        }

        public int DeviceID
        {
            get
            {
                if (FOutDevice != null)
                    return FOutDevice.DeviceID;
                return -1;
            }
        }

        /// <summary>
        ///     Disposes the underying output device and removes the events from the source
        /// </summary>
        public void Dispose()
        {
            UnRegisterEvents();
            FOutDevice.Dispose();
        }

        private void RegisterEvents()
        {
            FEventSource.MessageReceived += FEventSource_MessageReceived;
            FEventSource.ShortMessageReceived += EventSource_RawMessageReceived;
            FEventSource.ChannelMessageReceived += EventSource_ChannelMessageReceived;
            FEventSource.SysCommonMessageReceived += EventSource_SysCommonMessageReceived;
            FEventSource.SysExMessageReceived += EventSource_SysExMessageReceived;
            FEventSource.SysRealtimeMessageReceived += EventSource_SysRealtimeMessageReceived;
        }


        private void UnRegisterEvents()
        {
            FEventSource.MessageReceived -= FEventSource_MessageReceived;
            FEventSource.ShortMessageReceived -= EventSource_RawMessageReceived;
            FEventSource.ChannelMessageReceived -= EventSource_ChannelMessageReceived;
            FEventSource.SysCommonMessageReceived -= EventSource_SysCommonMessageReceived;
            FEventSource.SysExMessageReceived -= EventSource_SysExMessageReceived;
            FEventSource.SysRealtimeMessageReceived -= EventSource_SysRealtimeMessageReceived;
        }

        private void FEventSource_MessageReceived(IMidiMessage message)
        {
            var shortMessage = message as ShortMessage;
            if (shortMessage != null)
            {
                FOutDevice.SendShort(shortMessage.Message);
                return;
            }

            var sysExMessage = message as SysExMessage;
            if (sysExMessage != null)
                FOutDevice.Send(sysExMessage);
        }


        private void EventSource_SysRealtimeMessageReceived(object sender, SysRealtimeMessageEventArgs e)
        {
            FOutDevice.Send(e.Message);
        }

        private void EventSource_SysExMessageReceived(object sender, SysExMessageEventArgs e)
        {
            FOutDevice.Send(e.Message);
        }

        private void EventSource_SysCommonMessageReceived(object sender, SysCommonMessageEventArgs e)
        {
            FOutDevice.Send(e.Message);
        }

        private void EventSource_ChannelMessageReceived(object sender, ChannelMessageEventArgs e)
        {
            FOutDevice.Send(e.Message);
        }

        private void EventSource_RawMessageReceived(object sender, ShortMessageEventArgs e)
        {
            FOutDevice.SendShort(e.Message.Message);
        }

        public static OutputDeviceEventSink FromDeviceID(int deviceID, MidiEvents eventSource)
        {
            var deviceCount = OutputDeviceBase.DeviceCount;
            if (deviceCount > 0)
            {
                deviceID %= deviceCount;
                return new OutputDeviceEventSink(new OutputDevice(deviceID), eventSource);
            }

            return null;
        }
    }
}