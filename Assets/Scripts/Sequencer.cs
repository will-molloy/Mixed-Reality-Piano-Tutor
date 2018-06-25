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
    private readonly float notesSpeed = 0.1f;

    private float startTime = -1;
    private float deltaTime;
    private List<NoteDuration> ndrl;
    private TimeSignature ts;
    private float ttp;

    void Start()
    {
        piano = GetComponent<PianoBuilder>();
        ndrl = new List<NoteDuration>();
        instance = this;
    }

    public void LoadMidiFile(string file)
    {
        Debug.Log("Start with MIDI file: " + file);
        midiFile = MidiFile.Read(file);
    }

    public void LoadMidiFile()
    {
        var file = RuntimeSettings.MIDI_FILE_NAME;
        if (file != null)
        {
            LoadMidiFile(file);
        }
        else
        {
            LoadMidiFile("Assets/MIDI/Another_Love.mid");
        }

    }

    public void SpawnNotes()
    {
        if (midiFile == null)
        {
            throw new System.Exception("No midifile loaded, use LoadMidiFile() to load");
        }
        this.tempoMapManager = midiFile.ManageTempoMap();

        var tempomap = tempoMapManager.TempoMap;

        Debug.Log(tempomap.TimeDivision);
        this.ts = tempomap.TimeSignature.AtTime(0);
        this.ttp = ((float)ts.Numerator / ts.Denominator) / notesSpeed;
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
        ndrl.Clear();
    }

    private float calcX(float y)
    {
        return Mathf.Sqrt((y * y) / 26f);
    }
    private void SpawnNotesDropDown(List<Note> notes)
    {
        Debug.Log("Spawning piano roll notes");
        ClearPianoRoll();
        this.startTime = Time.time;
        notes.ForEach(e =>
        {

            var number = e.NoteNumber;
            var start = e.Time;
            var dur = e.Length;
            float y;
            var key = PianoKeys.GetKeyFor(number);
            if (key == null)
            {
                return;
            }
            var startMusical = (MusicalTimeSpan)e.TimeAs(TimeSpanType.Musical, this.tempoMapManager.TempoMap);
            var lengthMusical = e.LengthAs(TimeSpanType.Musical, this.tempoMapManager.TempoMap);
            y = ((float)startMusical.Numerator / startMusical.Denominator) * this.notesScale;
            Debug.Log(y);
            var delta = (MusicalTimeSpan)lengthMusical;
            var scale = ((float)delta.Numerator / delta.Denominator) * this.notesScale / 2;
            Debug.Log(scale);
            var lmraway = piano.GetLMRAwayVectorsForKey(key);
            var obj = Instantiate(pianoRollObject);
            var awayVector = lmraway.away;
            var lmraway2 = piano.GetLMRAwayVectorsForKey(key, calcX(y + scale));
            var keyPos = lmraway.centre;
            //Debug.Log(number + "  " + keyPos.ToString("F4"));
            pianoRollObjects.Add(obj);
            var dropdownScale = obj.transform.localScale;
            obj.transform.localScale = new Vector3(dropdownScale.x, scale, dropdownScale.z);
            //Debug.DrawLine(keyPos, lmraway2.away, Color.cyan, 99999, false);
            var rotation = Quaternion.LookRotation(keyPos - awayVector);
            obj.transform.rotation = rotation;
            //obj.transform.rotation *= Quaternion.Euler(0, -90 ,0); 
            obj.transform.Rotate(0, -90f, 90f);
            obj.transform.position = lmraway2.away;

            var renderer = obj.GetComponent<Renderer>();
            renderer.material.color = key.color == KeyColor.Black ? Color.black : Color.white;
            var rb = obj.GetComponent<Rigidbody>();
            rb.velocity = (keyPos - lmraway2.away).normalized * notesSpeed;

            var expectTime = ((lmraway2.away - keyPos).magnitude + scale / 2) / rb.velocity.magnitude;
            var expectEnd = scale / rb.velocity.magnitude;

            this.ndrl.Add(new NoteDuration(expectTime, expectEnd, key));
        });
    }

    public void Update()
    {
        if (this.startTime < 0f)
        {
            return;
        }
        var deltaT = Time.time - this.startTime;
        foreach (var item in ndrl)
        {
            if (deltaT > item.start && deltaT < item.end)
            {
                piano.ActivateKey(item.key.keyNum);
                Debug.Log("Activate" + item.key.keyNum);
            }
        }
        // TODO: Sync pulse timing
        if (deltaT % ttp <= 0.5)
        {
            piano.Pulse();
        }
    }
}

public struct NoteDuration
{
    public float start { get; }
    public float end { get; }
    public PianoKey key { get; }
    public NoteDuration(float start, float dur, PianoKey key)
    {
        this.start = start;
        this.end = start + dur;
        this.key = key;
    }
}