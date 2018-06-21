using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using Newtonsoft.Json;

///<summary>
/// Midi session API
///</summary>
public class MidiSessionController
{

    private const string JSON_PATH = "Assets/midi-sessions.json";

    public static List<MidiSessionDto> getMidiSessions(string midiFileName)
    {
        Debug.Log("Getting MIDI sessions: " + midiFileName);
        return getAllSessions().Where(x => x.FileName.Equals(midiFileName)).ToList();
    }

    private static List<MidiSessionDto> getAllSessions()
    {
        if (File.Exists(JSON_PATH) && File.ReadAllText(JSON_PATH).Trim().Length > 0)
        {
            var json = File.ReadAllText(JSON_PATH);
			return JsonConvert.DeserializeObject<List<MidiSessionDto>>(json);
        }
		return new List<MidiSessionDto>();
    }

    public static void putMidiSession(MidiSessionDto midiSession)
    {
        Debug.Log("Writing MIDI session: " + midiSession);
        // Better way to append into json collection?
        var savedSessions = getAllSessions();
        savedSessions.Add(midiSession);
        savedSessions.Sort((a, b) => a.FileName.CompareTo(b.FileName));
        var json = JsonConvert.SerializeObject(savedSessions, Formatting.Indented);
        File.WriteAllText(JSON_PATH, json);
    }

    // TODO remove once standard game mode is implemented
    public static void putDummyMidiSession(string midiPath)
    {
        putMidiSession(new MidiSessionDto(midiPath));
    }

}
