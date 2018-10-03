using Midi_Session;

/// <summary>
///  - Hack messaging system for sending information on scene changes. -> Bad OO Design!
///  - Also some default settings e.g. MIDI directory, game speed, etc.
/// </summary>
public class RuntimeSettings
{
    public static string MIDI_FILE_NAME = "Assets/MIDI\\Beethoven - Für Elise.mid";

    public static string MIDI_DIR = "Assets/MIDI";

    public static bool IS_PLAY_MODE = true; // false = practice mode

    public static MidiDifficultyDto.Difficulty DIFFICULTY = MidiDifficultyDto.Difficulty.Medium;

    public static string USER = "Beethoven";

    public static float GAME_SPEED = 0.2f;

    public static bool LOAD_SAVED_SESSION_AT_STARTUP = false;

    public static MidiSessionDto CACHED_SESSION = null;
}