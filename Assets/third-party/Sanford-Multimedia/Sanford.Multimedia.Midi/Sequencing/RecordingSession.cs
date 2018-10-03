using System.Collections.Generic;

namespace Sanford.Multimedia.Midi
{
    public class RecordingSession
    {
        private readonly List<TimestampedMessage> buffer = new List<TimestampedMessage>();
        private readonly IClock clock;

        public RecordingSession(IClock clock)
        {
            this.clock = clock;
        }

        public Track Result { get; private set; } = new Track();

        public void Build()
        {
            Result = new Track();

            buffer.Sort(new TimestampComparer());

            foreach (var tm in buffer) Result.Insert(tm.ticks, tm.message);
        }

        public void Clear()
        {
            buffer.Clear();
        }

        public void Record(ChannelMessage message)
        {
            if (clock.IsRunning) buffer.Add(new TimestampedMessage(clock.Ticks, message));
        }

        public void Record(SysExMessage message)
        {
            if (clock.IsRunning) buffer.Add(new TimestampedMessage(clock.Ticks, message));
        }

        private struct TimestampedMessage
        {
            public readonly int ticks;

            public readonly IMidiMessage message;

            public TimestampedMessage(int ticks, IMidiMessage message)
            {
                this.ticks = ticks;
                this.message = message;
            }
        }

        private class TimestampComparer : IComparer<TimestampedMessage>
        {
            #region IComparer<TimestampedMessage> Members

            public int Compare(TimestampedMessage x, TimestampedMessage y)
            {
                if (x.ticks > y.ticks)
                    return 1;
                if (x.ticks < y.ticks)
                    return -1;
                return 0;
            }

            #endregion
        }
    }
}