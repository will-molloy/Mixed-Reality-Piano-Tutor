using System.Collections.Generic;
using System.Runtime.Serialization;
using System.IO;
using System;

[DataContract]
public class MidiDifficultyDto
{
    public MidiDifficultyDto() { } // For json framework

    public MidiDifficultyDto(string FileName, Difficulty Difficulty)
    {
        this.FileName = FileName;
        this.difficulty = Difficulty;
    }

    [DataMember]
    public string FileName { get; set; }

    [DataMember]
    public Difficulty difficulty { get; set; }

    public enum Difficulty
    {
        Easy,
        Medium,
        Hard,
    }

}
