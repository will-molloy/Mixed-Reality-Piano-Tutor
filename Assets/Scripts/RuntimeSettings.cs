///<summary>
/// Hack messaging system.
///</summary>
public class RuntimeSettings {

	public static string MIDI_FILE_NAME = "Assets/MIDI/Für Elise.mid";

	public static string MIDI_DIR = "Assets/MIDI";

	public static bool IS_PLAY_MODE = true; // false = practice mode

    public static MidiDifficultyDto.Difficulty DIFFICULTY = MidiDifficultyDto.Difficulty.Medium;

	public static string USER = "Beethoven"; 

	public static float GAME_SPEED = 0.2f;

	public static bool LOAD_SAVED_SESSION_AT_STARTUP = false;

	public static MidiSessionDto CACHED_SESSION = null;

}
