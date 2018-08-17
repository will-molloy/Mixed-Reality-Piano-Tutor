using System.Collections.Generic;
using UnityEngine;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;
using System.Linq;
using System.Collections;

/// <summary>  
/// - Builds piano roll from MIDI file after virtual piano is built
/// </summary>  
[RequireComponent(typeof(PianoBuilder))]
[RequireComponent(typeof(MidiController))]
[RequireComponent(typeof(ScoreView))]
sealed public class Sequencer : MonoBehaviour
{
    private MidiFile midiFile;
    private TempoMapManager tempoMapManager;
    [SerializeField]
    private GameObject pianoRollObject;
    [SerializeField]
    private GameObject fineLine;
    private List<GameObject> pianoRollObjects = new List<GameObject>();
    private List<GameObject> fineLines = new List<GameObject>();
    public static Sequencer instance;
    private PianoBuilder piano;

    private Dictionary<PianoKey, List<GameObject>> pianoRollDict = new Dictionary<PianoKey, List<GameObject>>();
    private Dictionary<PianoKey, GameObject> keyAwayDir = new Dictionary<PianoKey, GameObject>();

    [SerializeField]
    public readonly float notesScale = 1f;
    [SerializeField]
    public readonly float notesSpeed = 0.2f;

    private float startTime = -1;
    private float deltaTime;
    private readonly float offsetStart = (0.5f * 1f) / 0.5f;
    private List<NoteDuration> noteDurations;
    private TimeSignature ts;
    private float ttp;

    private MidiController midiController;
    private ScoreView scoreView;
    private int totalChecked;
    private int totalMissed;

    private List<Coroutine> crtHolder;

    private bool gameStarted;

    private float timeBetweenBeats;
    private int totalBeats;

    void Start()
    {
        piano = GetComponent<PianoBuilder>();
        crtHolder = new List<Coroutine>();
        midiController = GetComponent<MidiController>();
        scoreView = GetComponent<ScoreView>();
        noteDurations = new List<NoteDuration>();
        instance = this;
        foreach (var item in PianoKeys.GetAllKeys())
        {
            pianoRollDict.Add(item, new List<GameObject>());
        }
    }

    public void LoadMidiFile(string file)
    {
        Debug.Log("Loading MIDI file: " + file);
        midiFile = MidiFile.Read(file);
        SpawnNotes();
    }

    public void LoadMidiFile()
    {
        Debug.Log("User: " + RuntimeSettings.CURRENT_USER);
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
        SpawnNotesDropDown(midiFile.GetNotes().ToList());
    }

    private void ClearPianoRoll()
    {
        gameStarted = false;
        Debug.Log("Clearing piano roll");
        pianoRollObjects.ForEach(o => GameObject.Destroy(o));
        pianoRollObjects.Clear();
        foreach (var i in pianoRollDict)
        {
            i.Value.ForEach(o => GameObject.Destroy(o));
            i.Value.Clear();
        }
        noteDurations.Clear();
        midiController.ClearMidiEventStorage();
        crtHolder.ForEach(e => StopCoroutine(e));
    }

    public void StartGame()
    {
        if (!gameStarted)
        {
            gameStarted = true;
            this.startTime = Time.time;
            crtHolder.Add(StartCoroutine(PulseCoroutine(timeBetweenBeats, totalBeats)));
            crtHolder.Add(StartCoroutine(TriggerChecks(timeBetweenBeats, totalBeats)));
            this.noteDurations.ForEach(e => {
                e.end += this.startTime;
                e.start += this.startTime;
            });
        }
    }

    public static float calcX(float y)
    {
        return Mathf.Sqrt((y * y) / 26f);
    }
    private void SpawnNotesDropDown(List<Note> notes)
    {
        Debug.Log("Spawning piano roll notes");
        ClearPianoRoll();
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
            var dummy = new GameObject("dummy");
            var awayVector = lmraway.away;
            var lmraway2 = piano.GetLMRAwayVectorsForKey(key, calcX(y + scale / 2f));
            if (!keyAwayDir.ContainsKey(key))
            {
                var newO = new GameObject();
                newO.transform.position = piano.GetLMRAwayVectorsForKey(key, -1).away;
                newO.transform.SetParent(piano.GetKeyObj(key).transform);
                keyAwayDir[key] = newO;
            }
            var keyPos = lmraway.centre;
            dummy.transform.position = lmraway2.away;
            dummy.transform.SetParent(piano.GetKeyObj(key).transform);
            obj.transform.SetParent(dummy.transform);
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
            //rb.velocity = (keyPos - lmraway2.away).normalized * notesSpeed;

            var expectTime = ((lmraway2.away - keyPos).magnitude + scale / 2) / notesSpeed;
            var expectEnd = scale / notesSpeed;

            //Debug.Log("Scale: " + scale + "  y:" + y);
            this.pianoRollDict[key].Add(dummy);
            this.noteDurations.Add(new NoteDuration(expectTime, expectEnd, key));
        });

        // Spawn fine lines (horizontal)
        var lastNoteMusicalStart = (MusicalTimeSpan)notes.Last().TimeAs(TimeSpanType.Musical, this.tempoMapManager.TempoMap);
        var lastNoteMusicalLen = (MusicalTimeSpan)notes.Last().LengthAs(TimeSpanType.Musical, this.tempoMapManager.TempoMap);
        var lastNoteMuscialEnd = lastNoteMusicalStart + lastNoteMusicalLen;

        var lastBeatY = ((float)lastNoteMuscialEnd.Numerator / lastNoteMusicalLen.Denominator) * this.notesScale;
        var beatDelta = ((float)1f) * this.notesScale;

        var midKey = PianoKeys.GetKeyFor(PianoBuilder.CENTRE);
        var totalBeatsI = (int)(lastBeatY / beatDelta);

        for (int i = 1; i <= totalBeatsI; i++)
        {
            var line = Instantiate(fineLine);
            var v = piano.GetLMRAwayVectorsForKey(midKey, calcX(beatDelta * i));
            var dummy = new GameObject("dummy Fineline");
            dummy.transform.position = v.away;
            dummy.transform.SetParent(piano.GetKeyObj(midKey).transform);
            line.transform.SetParent(dummy.transform);
            line.transform.position = v.away;
            var rotation = Quaternion.LookRotation(v.centre - v.away);
            line.transform.rotation = rotation;
            line.transform.Rotate(0, 0f, 90f);
            this.fineLines.Add(dummy);
        }
        timeBetweenBeats = beatDelta / this.notesSpeed;
        totalBeats = totalBeatsI;
    }

    private IEnumerator TriggerChecks(float time, int totalBeats)
    {
        const int freq = 8;
        for (int i = 0; i <= totalBeats; i++)
        {
            var total = 0;
            var totalMiss = 0;
            for (int j = 0; j < freq; j++)
            {
                // Check for accuracy
                var ons = midiController.GetOnKeys();
                var eligible = this.noteDurations.FindAll(e => e.start >= Time.time && e.end <= Time.time);
                eligible.ForEach(e =>
                {
                    total++;
                    if (!ons.Contains(e.key))
                    {
                        totalMiss++;
                    }
                });
                yield return new WaitForSeconds(time / (float)freq);
            }
            piano.PutInstantFeedback(total, totalMiss, totalBeats);
        }
    }

    private IEnumerator PulseCoroutine(float time, int totalBeats)
    {
        for (int i = 0; i <= totalBeats; i++)
        {
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
        if (noteDurations.Last().hasKeyBeenActivated || Input.GetKeyDown(KeyCode.Escape))
        {
            scoreView.DisplayScores(midiController.GetMidiEvents(), this.noteDurations, this.notesScale, this.notesSpeed, this.startTime);
            this.ClearPianoRoll();
            this.startTime = -1f;
        }

        if (gameStarted)
        {

            float step = notesSpeed * Time.deltaTime;
            var minDistDict = new Dictionary<PianoKey, float>();
            foreach (var item in pianoRollDict)
            {
                if (item.Value.Count == 0)
                {
                    continue;
                }
                var lmr = piano.GetLMRAwayVectorsForKey(item.Key);
                foreach (var obj in item.Value)
                {
                    //obj.transform.position = Vector3.MoveTowards(obj.transform.position, lmr.centre, step);
                    obj.transform.position = Vector3.MoveTowards(obj.transform.position, keyAwayDir[item.Key].transform.position, step);
                    var newD = obj.transform.position - lmr.centre;
                    if(!minDistDict.ContainsKey(item.Key)) {
                        minDistDict[item.Key] = 10000f;
                    }
                    if (newD.magnitude < minDistDict[item.Key]) {
                        minDistDict[item.Key] = newD.magnitude;
                    }
                }
            }
            foreach (var item in minDistDict) {
                if (item.Value < 1f) 
                    piano.UpdateDiskColor(item.Key, 1 - item.Value);

            }
            var lmr2 = piano.GetLMRAwayVectorsForKey((PianoKeys.GetKeyFor(PianoBuilder.CENTRE)));

            foreach (var obj in fineLines)
            {
                obj.transform.position = Vector3.MoveTowards(obj.transform.position, keyAwayDir[PianoKeys.GetKeyFor(PianoBuilder.CENTRE)].transform.position, step);
                //obj.transform.position = Vector3.MoveTowards(obj.transform.position, lmr2.centre, step);
            }
        }
    }
}


public struct NoteDuration
{
    public bool hasKeyBeenActivated { get; set; }
    public float duration { get; }
    public float start { get; set; }
    public float end { get; set; }
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
