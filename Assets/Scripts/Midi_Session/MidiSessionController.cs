﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace Midi_Session
{
    /// <summary>
    ///     - Midi session API; write sessions to disk via JSON
    /// </summary>
    public class MidiSessionController
    {
        private static readonly string MIDI_SESSIONS_JSON_PATH = "Assets/Resources/midi-sessions.json";

        private static readonly string MIDI_DIFFICULTY_TABLE_PATH = "Assets/Resources/midi-difficulties.json";

        public List<MidiSessionDto> getMidiSessions(string midiFileName)
        {
            Debug.Log("Getting MIDI sessions: " + midiFileName);
            return getAllSessions().Where(x => x.FileName.Equals(midiFileName)).ToList();
        }

        public List<MidiSessionDto> getAllSessions()
        {
            if (File.Exists(MIDI_SESSIONS_JSON_PATH))
            {
                var json = File.ReadAllText(MIDI_SESSIONS_JSON_PATH);
                if (json.Trim().Length > 0) // empty file cause problems
                    return JsonConvert.DeserializeObject<List<MidiSessionDto>>(json);
            }

            return new List<MidiSessionDto>();
        }

        public void putMidiSession(MidiSessionDto midiSession)
        {
            Debug.Log("Writing MIDI session: " + midiSession);
            // Better way than getting all sessions each time to append into json collection?
            var savedSessions = getAllSessions();
            savedSessions.Add(midiSession);
            savedSessions.Sort((a, b) => -a.SessionDateTime.CompareTo(b.SessionDateTime)); // earliest first
            var json = JsonConvert.SerializeObject(savedSessions, Formatting.Indented);
            File.WriteAllText(MIDI_SESSIONS_JSON_PATH, json);
        }

        public MidiDifficultyDto.Difficulty GetDifficultyFor(string midiPath)
        {
            Debug.Log("Retrieving difficulty for: " + midiPath);
            var table = getAllDifficulties().Where(x => x.FileName.Equals(midiPath));
            if (table.Count() > 0)
            {
                return table.First().difficulty;
            }

            var dto = new MidiDifficultyDto(midiPath, MidiDifficultyDto.Difficulty.Easy);
            putDifficultyEntry(dto);
            return dto.difficulty;
        }

        private List<MidiDifficultyDto> getAllDifficulties()
        {
            if (File.Exists(MIDI_DIFFICULTY_TABLE_PATH))
            {
                var json = File.ReadAllText(MIDI_DIFFICULTY_TABLE_PATH);
                if (json.Trim().Length > 0) // empty file cause problems
                    return JsonConvert.DeserializeObject<List<MidiDifficultyDto>>(json);
            }

            return new List<MidiDifficultyDto>();
        }

        private void putDifficultyEntry(MidiDifficultyDto dto)
        {
            Debug.Log("Creating difficulty entry for: " + dto.FileName);
            var savedSessions = getAllDifficulties();
            savedSessions.Add(dto);
            var json = JsonConvert.SerializeObject(savedSessions, Formatting.Indented);
            File.WriteAllText(MIDI_DIFFICULTY_TABLE_PATH, json);
        }
    }
}