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
[RequireComponent(typeof(MidiController))]
[RequireComponent(typeof(ScoreView))]
public class Sequencer : MonoBehaviour
{

    private MidiFile midiFile;
    private NotesManager noteManager;
    private TempoMapManager tempoMapManager;
    [SerializeField]
    private GameObject pianoRollObject;
    [SerializeField]
    private GameObject fineLine;
    private List<GameObject> pianoRollObjects = new List<GameObject>();
    public static Sequencer instance;
    private PianoBuilder piano;

    [SerializeField]
    private readonly float notesScale = 1f;
    [SerializeField]
    private readonly float notesSpeed = 0.1f;

    private float startTime = -1;
    private float deltaTime;
    private List<NoteDuration> noteDurations;
    private TimeSignature ts;
    private float ttp;

    private MidiController midiController;
    private ScoreView scoreView;

    void Start()
    {
        piano = GetComponent<PianoBuilder>();
        midiController = GetComponent<MidiController>();
        scoreView = GetComponent<ScoreView>();
        noteDurations = new List<NoteDuration>();
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
            Debug.LogError("No MIDI file set in RuntimeSettings.");
        }
    }

    public void SpawnNotes()
    {
        if (midiFile == null)
        {
            throw new System.Exception("No midifile loaded, use LoadMidiFile() to load");
        }
        this.tempoMapManager = midiFile.ManageTempoMap();
        //MidiController.clearMidiEventStorage();
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
        SpawnNotesDropDown(midiFile.GetNotes().ToList());
    }

    private void ClearPianoRoll()
    {
        Debug.Log("Clearing piano roll");
        pianoRollObjects.ForEach(o => GameObject.Destroy(o));
        pianoRollObjects.Clear();
        noteDurations.Clear();
        midiController.ClearMidiEventStorage();
    }

    public static float calcX(float y)
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
            // Debug.Log(y);
            var delta = (MusicalTimeSpan)lengthMusical;
            var scale = ((float)delta.Numerator / delta.Denominator) * this.notesScale;
            // Debug.Log(scale);
            var lmraway = piano.GetLMRAwayVectorsForKey(key);
            var obj = Instantiate(pianoRollObject);
            var awayVector = lmraway.away;
            var lmraway2 = piano.GetLMRAwayVectorsForKey(key, calcX(y + scale / 2f));
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
            renderer.material.color = key.color == KeyColor.Black ? Color.blue : Color.white;
            var rb = obj.GetComponent<Rigidbody>();
            rb.velocity = (keyPos - lmraway2.away).normalized * notesSpeed;

            var expectTime = ((lmraway2.away - keyPos).magnitude + scale / 2) / rb.velocity.magnitude;
            var expectEnd = scale / rb.velocity.magnitude;
            
            //Debug.Log("Scale: " + scale + "  y:" + y);

            this.noteDurations.Add(new NoteDuration(expectTime, expectEnd, key));
        });

        // Spawn fine lines
        var lastNoteMusicalStart = (MusicalTimeSpan)notes.Last().TimeAs(TimeSpanType.Musical, this.tempoMapManager.TempoMap);
        var lastNoteMusicalLen = (MusicalTimeSpan)notes.Last().LengthAs(TimeSpanType.Musical, this.tempoMapManager.TempoMap);
        var lastNoteMuscialEnd = lastNoteMusicalStart + lastNoteMusicalLen;

        var lastBeatY = ((float)lastNoteMuscialEnd.Numerator / lastNoteMusicalLen.Denominator) * this.notesScale;
        var beatDelta = ((float)1f / lastNoteMusicalLen.Denominator) * this.notesScale;

        var midKey = PianoKeys.GetKeyFor(PianoBuilder.CENTRE);
        var totalBeatsI = (int)(lastBeatY / beatDelta);

        for(int i = 0; i <= totalBeatsI; i++) {
            var line = Instantiate(fineLine);
            var v = piano.GetLMRAwayVectorsForKey(midKey, calcX(beatDelta * i));
            this.pianoRollObjects.Add(line);
            line.transform.position = v.away;
            var rotation = Quaternion.LookRotation(v.centre - v.away);
            line.transform.rotation = rotation;
            line.transform.Rotate(0, 0f, 90f);
            var rb = line.GetComponent<Rigidbody>();
            rb.velocity = (v.centre - v.away).normalized * this.notesSpeed;
        }

        StartCoroutine(PulseCoroutine(beatDelta / this.notesSpeed, totalBeatsI));

    }

    IEnumerator PulseCoroutine(float time, int totalBeats) {
        for(int i = 0; i <= totalBeats; i++) {
            piano.Pulse();
            yield return new WaitForSeconds(time);
        }
    }

    public void Update()
    {
        if (this.startTime < 0f)
        {
            return;
        }
        var deltaT = Time.time - this.startTime;

        noteDurations.ForEach(note =>
        {
            if (!note.hasKeyBeenActivated && deltaT >= (note.start - note.duration) && deltaT < (note.end - note.duration))
            {
                piano.ActivateKey(note.key.keyNum, Color.red, note.duration);
                note.hasKeyBeenActivated = true;
            }
        });
        // TODO: Sync pulse timing
        if (deltaT % ttp <= 0.5)
        {
            piano.Pulse();
        }
        if (noteDurations.Last().hasKeyBeenActivated || Input.GetKeyDown(KeyCode.Escape))
        {
            scoreView.DisplayScores(midiController.GetMidiEvents(), this.noteDurations, this.notesScale);
            this.ClearPianoRoll();
        }
    }
}

public struct NoteDuration
{
    public bool hasKeyBeenActivated { get; set; }
    public float duration { get; }
    public float start { get; }
    public float end { get; }
    public PianoKey key { get; }
    public NoteDuration(float start, float dur, PianoKey key)
    {
        this.hasKeyBeenActivated = false;
        this.start = start;
        this.end = start + dur;
        this.key = key;
        this.duration = dur;
    }
}
