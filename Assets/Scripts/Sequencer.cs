using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Melanchall.DryWetMidi;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;
using Melanchall.DryWetMidi.Common;
using System.Linq;

/// <summary>  
/// - Builds piano roll from MIDI file after virtual piano is built
/// </summary>  
[RequireComponent(typeof(PianoBuilder))]
public class Sequencer : MonoBehaviour
{
    [SerializeField]
    private string midiFileName;
    private MidiFile midiFile;
    private NotesManager noteManager;
    private TempoMapManager tempoMapManager;
    [SerializeField]
    private GameObject pianoRollObject;
    private List<GameObject> pianoRollObjects = new List<GameObject>();
    public static Sequencer instance;
    private PianoBuilder piano;

    [SerializeField]
    private readonly float notesScale = 1f;
    [SerializeField]
    private readonly float notesSpeed = 0.2f;

    private float startTime;

    void Start()
    {
        piano = GetComponent<PianoBuilder>();
        instance = this;
    }

    public void LoadMidiFile(string file) {
        midiFile = MidiFile.Read(midiFileName);
    }

    public void LoadMidiFile() {
        LoadMidiFile(this.midiFileName);
    }

    public void SpawnNotes()
    {
        if (midiFile == null) {
            throw new System.Exception("No midifile loaded, use LoadMidiFile() to load");
        }
        this.tempoMapManager = midiFile.ManageTempoMap();

        var tempomap = tempoMapManager.TempoMap;

        Debug.Log(tempomap.TimeDivision);
        Debug.Log(tempomap.TimeSignature.AtTime(0));
        Debug.Log(tempomap.Tempo.AtTime(0));

        if (noteManager == null)
        {
            for (int i = 0; i < midiFile.Chunks.Count; i++)
            {
                MidiChunk chunk = midiFile.Chunks[i];
                if (chunk.GetType().Equals(typeof(TrackChunk)))
                {
                    using (var nm = new NotesManager(((TrackChunk)chunk).Events))
                    {
                        this.noteManager = nm;
                    }
                    break;
                }
            }
        }
        //SpawnNotesDropDown(noteManager.Notes.ToList());
        SpawnNotesDropDown(midiFile.GetNotes().ToList());
    }

    private void ClearPianoRoll()
    {
        Debug.Log("Clearing piano roll");
        pianoRollObjects.ForEach(o => GameObject.Destroy(o));
        pianoRollObjects.Clear();
    }

    private float calcX(float y) {
        return Mathf.Sqrt(y * y / 26);
    }
    private void SpawnNotesDropDown(List<Note> notes) {
        Debug.Log("Spawning piano roll notes");
        ClearPianoRoll();
        this.startTime = Time.time;
        notes.ForEach(e => {
            var number = e.NoteNumber;
            var start = e.Time;
            var dur = e.Length;
            float y;
            var key = PianoKeys.GetKeyFor(number);
            if(key == null) {
                return;
            }
            var startMusical = (MusicalTimeSpan)e.TimeAs(TimeSpanType.Musical, this.tempoMapManager.TempoMap);
            var lengthMusical = e.LengthAs(TimeSpanType.Musical, this.tempoMapManager.TempoMap);
            y = ((float)startMusical.Numerator / startMusical.Denominator)* this.notesScale;
            var delta = (MusicalTimeSpan)lengthMusical;
            var scale = ((float)delta.Numerator / delta.Denominator) * notesScale;
            Debug.Log(scale);
            var lmraway = piano.GetLMRAwayVectorsForKey(key);
            var obj = Instantiate(pianoRollObject);
            var awayVector = lmraway.away;
            var lmraway2 = piano.GetLMRAwayVectorsForKey(key, calcX(y + scale));
            var scaled = new Vector3(awayVector.x, awayVector.y, awayVector.z);
            var keyPos = lmraway.centre;
            var actualAway = (awayVector - keyPos).normalized;
            var rot = Quaternion.LookRotation(awayVector);
            //Debug.Log(number + "  " + keyPos.ToString("F4"));
            pianoRollObjects.Add(obj);
            var dropdownScale = obj.transform.lossyScale;
            obj.transform.localScale = new Vector3(dropdownScale.x, scale, dropdownScale.z);
            Debug.DrawLine(keyPos, lmraway2.away, Color.cyan, 99999, false);
            obj.transform.position = lmraway2.away;
            var rotation = Quaternion.LookRotation(keyPos - awayVector);
            obj.transform.rotation = rotation;
            obj.transform.rotation *= Quaternion.Euler(0, -90 ,0); 
            obj.transform.Rotate(0,0,90f);
            var renderer = obj.GetComponent<Renderer>();
            renderer.material.color = key.color == KeyColor.Black ? Color.black : Color.white;
            var rb = obj.GetComponent<Rigidbody>();
            rb.velocity = (keyPos - lmraway2.away).normalized * notesSpeed;
        });
    }

}