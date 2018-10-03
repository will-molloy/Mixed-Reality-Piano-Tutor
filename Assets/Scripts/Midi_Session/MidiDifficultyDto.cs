using System.Runtime.Serialization;

namespace Midi_Session
{
    /// <summary>
    ///     - Represents MIDI files difficulty
    ///     - Acts as a sort of 'join table' for MidiSessions
    /// </summary>
    [DataContract]
    public class MidiDifficultyDto
    {
        public enum Difficulty
        {
            Beginner, // ONE HANDED
            Easy,
            Medium,
            Hard,
            Expert
        }

        public MidiDifficultyDto() // For json framework
        {
        } 

        public MidiDifficultyDto(string FileName, Difficulty Difficulty)
        {
            this.FileName = FileName;
            difficulty = Difficulty;
        }

        [DataMember] public string FileName { get; set; }

        [DataMember] public Difficulty difficulty { get; set; }
    }
}