using System.Collections.Generic;
using System.Runtime.Serialization;
using System.IO;
using System;

///<summary>
/// Record of a midi track session
///</summary>
[DataContract]
public class MidiSessionDto
{
    public MidiSessionDto() { } // For json framework

    public MidiSessionDto(string FileName, double Accuracy, List<NoteDuration> midiEvents, List<NoteDuration> durs, float noteScale, float velocityIn, float offsetStartTime)
    {
        this.FileName = FileName;
        this.FormattedTrackName = formatTrackName(FileName);
        this.GameMode = RuntimeSettings.IS_PLAY_MODE ? Mode.SpaceInvader : Mode.Practice;
        this.Accuracy = Accuracy;
        this.SessionDateTime = DateTime.Now;
        this.User = RuntimeSettings.USER;
        this.midiEvents = midiEvents;
        this.noteDurations = durs;
        this.noteScale = noteScale;
        this.velocityIn = velocityIn;
        this.offsetStartTime = offsetStartTime;
    }

    // Creates a dummy session
    public MidiSessionDto(string midiPath) : this(midiPath, 0, new List<NoteDuration>(), new List<NoteDuration>(), 0, 0, 0) { }

    private static string formatTrackName(string midiPath)
    {
        return Path.GetFileNameWithoutExtension(midiPath).Replace("_", " ");
    }

    [DataMember] public string FileName { get; set; } // id

    [DataMember] public string FormattedTrackName { get; set; }

    [DataMember] public Mode GameMode { get; set; }

    [DataMember] public double Accuracy { get; set; }

    [DataMember] public DateTime SessionDateTime { get; set; }

    [DataMember] public string User { get; set; }

    // Midi events for recreating feedback view in history mode
    [DataMember] public List<NoteDuration> midiEvents { get; set; }

    [DataMember] public List<NoteDuration> noteDurations { get; set; }

    [DataMember] public float noteScale { get; set; }

    [DataMember] public float velocityIn { get; set; }

    [DataMember] public float offsetStartTime { get; set; }

    public override string ToString()
    {
        return "MidiSession - FileName: " + FileName + ", ";
    }

    public enum Mode
    {
        Practice,
        SpaceInvader,
    }

}

