using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using Midi_Sequencer;

namespace Midi_Session
{
    /// <summary>
    ///     - Record of a midi track session
    /// </summary>
    [DataContract]
    public class MidiSessionDto
    {
        public enum Mode
        {
            Practice,
            SpaceInvader
        }

        public MidiSessionDto() // For json framework
        {
        } 

        public MidiSessionDto(string FileName, double Accuracy, List<CompressedNoteDuration> midiEvents,
            List<CompressedNoteDuration> durs, float noteScale, float velocityIn, float offsetStartTime)
        {
            this.FileName = FileName;
            FormattedTrackName = formatTrackName(FileName);
            GameMode = RuntimeSettings.IS_PLAY_MODE ? Mode.SpaceInvader : Mode.Practice;
            this.Accuracy = Accuracy;
            SessionDateTime = DateTime.Now;
            User = RuntimeSettings.USER;
            userNoteDurations = midiEvents;
            trackNoteDurations = durs;
            this.noteScale = noteScale;
            this.velocityIn = velocityIn;
            this.offsetStartTime = offsetStartTime;
        }

        // Creates a dummy session
        public MidiSessionDto(string midiPath) : this(midiPath, 0, new List<CompressedNoteDuration>(),
            new List<CompressedNoteDuration>(), 0, 0, 0)
        {
        }

        [DataMember] public string FileName { get; set; } // id

        [DataMember] public string FormattedTrackName { get; set; }

        [DataMember] public Mode GameMode { get; set; }

        [DataMember] public double Accuracy { get; set; }

        [DataMember] public DateTime SessionDateTime { get; set; }

        [DataMember] public string User { get; set; }

        // Midi events for recreating feedback view from history UI
        [DataMember] public float noteScale { get; set; }

        [DataMember] public float velocityIn { get; set; }

        [DataMember] public float offsetStartTime { get; set; }

        [DataMember] public List<CompressedNoteDuration> userNoteDurations { get; set; }

        [DataMember] public List<CompressedNoteDuration> trackNoteDurations { get; set; }

        private static string formatTrackName(string midiPath)
        {
            return Path.GetFileNameWithoutExtension(midiPath).Replace("_", " ");
        }

        public override string ToString()
        {
            return "MidiSession - FileName: " + FileName + ", ";
        }
    }
}