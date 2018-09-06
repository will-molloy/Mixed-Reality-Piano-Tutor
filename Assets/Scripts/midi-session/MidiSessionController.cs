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
    private string jsonPath;

    public MidiSessionController(string jsonPath)
    {
        this.jsonPath = jsonPath;
    }

    public List<MidiSessionDto> getMidiSessions(string midiFileName)
    {
        Debug.Log("Getting MIDI sessions: " + midiFileName);
        return getAllSessions().Where(x => x.FileName.Equals(midiFileName)).ToList();
    }

    private List<MidiSessionDto> getAllSessions()
    {

        if (File.Exists(jsonPath))
        {
            var json = File.ReadAllText(jsonPath);
            if (json.Trim().Length > 0) // empty file cause problems
            {
                return JsonConvert.DeserializeObject<List<MidiSessionDto>>(json);
            }
        }
        return new List<MidiSessionDto>();
    }

    public void putMidiSession(MidiSessionDto midiSession)
    {
        Debug.Log("Writing MIDI session: " + midiSession);
        // Better way than getting all sessions each time to append into json collection?
        var savedSessions = getAllSessions();
        savedSessions.Add(midiSession);
        savedSessions.Sort((a, b) => a.FileName.CompareTo(b.FileName));
        var json = JsonConvert.SerializeObject(savedSessions, Formatting.Indented);
        File.WriteAllText(jsonPath, json);
    }

    // Useful for testing
    public void putDummyMidiSession(string midiPath)
    {
        putMidiSession(new MidiSessionDto(midiPath));
    }

}
