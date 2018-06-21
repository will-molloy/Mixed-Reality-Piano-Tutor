using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.IO;

///<summary>
/// Record of a midi track session
///</summary>
[DataContract]  
public class MidiSessionDto
{
    public MidiSessionDto(){} // For json framework

    public MidiSessionDto(string FileName, Difficulty TrackDifficulty, Mode GameMode, double Accuracy){
        this.FileName = FileName;
        this.FormattedTrackName = formatTrackName(FileName);
        this.TrackDifficulty = TrackDifficulty;
        this.GameMode = GameMode;
        this.Accuracy = Accuracy;
    }

    // Creates a dummy session
    public MidiSessionDto(string midiPath) : this(midiPath, Difficulty.Easy, Mode.Standard, 0){}

    private static string formatTrackName(string midiPath)
    {
		return Path.GetFileNameWithoutExtension(midiPath).Replace("_", " ");
    }

    [DataMember]  
    public string FileName { get; set; } // id

    [DataMember]  
    public string FormattedTrackName { get; set; }

    [DataMember]  
    public Difficulty TrackDifficulty { get; set; }

    [DataMember]  
    public Mode GameMode { get; set; }

    [DataMember]  
    public double Accuracy { get; set; }

    public enum Difficulty
    {
        Easy,
        Medium,
        Hard,
    }

    public enum Mode
    {
        Standard,
        Laser,
        SpaceInvader,
    }

    public override string ToString()
    {
        return "MidiSession - FileName: " + FileName + ", ";
    }

}
