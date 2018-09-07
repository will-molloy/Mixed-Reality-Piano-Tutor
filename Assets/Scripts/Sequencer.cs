using System.Collections.Generic;
using UnityEngine;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;
using System.Linq;
using System.Collections;
using System.Runtime.Serialization;
using System.IO;
using System;

/// <summary>  
/// - Builds piano roll from MIDI file after virtual piano is built
/// </summary>  
[RequireComponent(typeof(PianoBuilder))]
[RequireComponent(typeof(MidiController))]
[RequireComponent(typeof(ScoreView))]
sealed public class Sequencer : MonoBehaviour
{

    static public readonly Color[] GAY = {
        MakeColorFromHex(0xcf0202),
        MakeColorFromHex(0xc57905),
        MakeColorFromHex(0xc5b805),
        MakeColorFromHex(0x17be17),
        MakeColorFromHex(0x0cbe9b),
        MakeColorFromHex(0xc5fbe),
        MakeColorFromHex(0x7d0ec2)};

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

    public static Dictionary<PianoKey, Color> colorDict = new Dictionary<PianoKey, Color>();

    [SerializeField]
    public readonly float notesScale = 1f;

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

    public static Color MakeColorFromHex(int hex)
    {
        byte R = (byte)((hex >> 16) & 0xFF);
        byte G = (byte)((hex >> 8) & 0xFF);
        byte B = (byte)((hex) & 0xFF);
        return new Color32(R, G, B, 255);
    }

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
        var ws = PianoKeys.GetAllKeys().Where(e => e.color == KeyColor.White).ToList();
        for (int i = 0; i < ws.Count(); i++)
        {
            colorDict[ws[i]] = MakeColorFromHex(0xffffff);
        }
        PianoKeys.GetAllKeys().Where(e => e.color == KeyColor.Black).ToList().ForEach(e => colorDict[e] = MakeColorFromHex(0x0000ff));

    }

    public void LoadMidiFile(string file)
    {
        Debug.Log("Loading MIDI file: " + file + ", Mode: " + (RuntimeSettings.IS_PLAY_MODE ? "Play" : "Practice") + ", Difficulty: " + RuntimeSettings.DIFFICULTY + ", GameSpeed: " + RuntimeSettings.GAME_SPEED + ", User: " + RuntimeSettings.USER);
        midiFile = MidiFile.Read(file);
        SpawnNotes();
    }

    public void LoadMidiFile()
    {
        Debug.Log("User: " + RuntimeSettings.USER);
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
        this.ttp = ((float)ts.Numerator / ts.Denominator) / RuntimeSettings.GAME_SPEED;
        Debug.Log(tempomap.Tempo.AtTime(0));
        SpawnNotesDropDown(midiFile.GetNotes().ToList());
    }

    public float GetStartTime()
    {
        return this.startTime;
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
            var lmraway3 = piano.GetLMRAwayVectorsForKey(key, calcX(y + scale));
            if (!keyAwayDir.ContainsKey(key))
            {
                var newO = new GameObject();
                newO.transform.position = piano.GetLMRAwayVectorsForKey(key, -1).away;
                newO.transform.SetParent(piano.GetKeyObj(key).transform);
                keyAwayDir[key] = newO;
            }
            var keyPos = lmraway.centre;
            dummy.transform.position = lmraway3.away;
            dummy.transform.rotation = Quaternion.LookRotation(keyPos - awayVector);
            dummy.transform.Rotate(0, -90f, 90f);
            dummy.transform.SetParent(piano.transform);
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
            //renderer.material.color = key.color == KeyColor.Black ? Color.blue : Color.white;
            renderer.material.color = colorDict[key];
            var rb = obj.GetComponent<Rigidbody>();
            //rb.velocity = (keyPos - lmraway2.away).normalized * notesSpeed;

            var expectTime = ((lmraway2.away - keyPos).magnitude + scale / 2) / RuntimeSettings.GAME_SPEED;
            var expectEnd = scale / RuntimeSettings.GAME_SPEED;

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
        timeBetweenBeats = beatDelta / RuntimeSettings.GAME_SPEED;
        totalBeats = totalBeatsI;
    }

    private IEnumerator TriggerChecks(float time, int totalBeats)
    {
        /*
        this.noteDurations.ForEach(e => {
            Debug.Log("Start");
            Debug.Log(e.start);
            Debug.Log(e.end);
            Debug.Log("END");
        });
        */
        const int freq = 16;
        for (int i = 0; i <= totalBeats; i++)
        {
            var total = 0;
            var totalMiss = 0;
            for (int j = 0; j < freq; j++)
            {
                // Check for accuracy
                var ons = midiController.GetOnKeys();
                var deltaTime = Time.time - startTime;
                var eligible = this.noteDurations.FindAll(e => e.start <= deltaTime && e.end >= deltaTime);
                eligible.ForEach(e =>
                {
                    total++;
                    if (!ons.Contains(PianoKeys.GetKeyFor(e.keyNum)))
                    {
                        totalMiss++;
                    }
                });
                yield return new WaitForSeconds(time / (float)freq);
            }
            piano.PutInstantFeedback(total, totalMiss, totalBeats);
        }
    }

    public bool IsGamedStarted()
    {
        return this.gameStarted;
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
        if (RuntimeSettings.LOAD_SAVED_SESSION_AT_STARTUP)
        {
            Debug.Log("Loading a saved session");
            RuntimeSettings.LOAD_SAVED_SESSION_AT_STARTUP = false;
            scoreView.SaveScoresAndViewFeedback(RuntimeSettings.CACHED_SESSION, false);
            RuntimeSettings.CACHED_SESSION = null;
            this.ClearPianoRoll();
            this.startTime = -1f;
        }

        if (this.startTime < 0f)
        {
            return;
        }
        var deltaT = Time.time - this.startTime;

        var minDistDict = new Dictionary<PianoKey, float>();
        foreach (var i in PianoKeys.GetAllKeys())
        {
            minDistDict[i] = 2f;
        }
        noteDurations.ForEach(note =>
        {
            if (!note.hasKeyBeenActivated && deltaT >= (note.start - note.duration) && deltaT < (note.end - note.duration))
            {
                piano.ActivateKey(note.keyNum, Color.red, note.duration);
                note.hasKeyBeenActivated = true;
            }
            if (deltaT >= (note.start - note.duration) && deltaT < (note.end - note.duration))
            {
                minDistDict[PianoKeys.GetKeyFor(note.keyNum)] = 0;
                return;
            }
            else if (deltaT > note.end)
            {
                return;
            }

            if (deltaT >= (note.start - 2f) && deltaT <= note.start)
            {
                minDistDict[PianoKeys.GetKeyFor(note.keyNum)] = Mathf.Min(Mathf.Abs(note.start - deltaT), minDistDict[PianoKeys.GetKeyFor(note.keyNum)]);
            }
        });
        foreach (var item in minDistDict)
        {
            float h, s, v;
            Color.RGBToHSV(colorDict[item.Key], out h, out s, out v);
            if (item.Value == 2f)
            {
                s = 0f;
            }
            else
            {
                s = ((2 - item.Value) / 2) * s;
            }
            var newc = Color.HSVToRGB(h, s, v);
            newc.a = (2 - item.Value) / 2;
            piano.UpdateDiskColor(item.Key, newc);
        }
        if (noteDurations.Last().hasKeyBeenActivated || Input.GetKeyDown(KeyCode.Escape))
        {
            scoreView.ConvertEventsSaveScoresAndViewFeedback(midiController.GetMidiEvents(), this.noteDurations, this.notesScale, RuntimeSettings.GAME_SPEED, this.startTime);
            this.ClearPianoRoll();
            this.startTime = -1f;
        }

        if (gameStarted)
        {
            float step = RuntimeSettings.GAME_SPEED * Time.deltaTime;
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
                    if (obj.transform.childCount > 0)
                    {
                        var co = obj.transform.GetChild(0);
                        var childScale = co.transform.localScale;
                        var mag = (obj.transform.position - lmr.centre).magnitude;
                        if (mag < childScale.y)
                        {
                            //obj.transform.GetChild(0).transform.localScale = new Vector3(co.transform.localScale.x, (obj.transform.position - lmr.centre).magnitude, co.transform.localScale.z);
                            var cs = co.transform.localScale.y;
                            var cur = obj.transform.localScale.y;

                            obj.transform.localScale = new Vector3(1f, mag / cs, 1f);
                        }
                        if (mag < 0.01f)
                        {
                            DestroyImmediate(co.gameObject);
                        }
                    }
                }

            }
            foreach (var obj in fineLines)
            {
                var center = PianoKeys.GetKeyFor(PianoBuilder.CENTRE);
                var centerAway = keyAwayDir[center];
                obj.transform.position = Vector3.MoveTowards(obj.transform.position, centerAway.transform.position, step);
            }
        }
    }
}

[DataContract]
public class NoteDuration
{
    [DataMember] public bool hasKeyBeenActivated { get; set; }
    [DataMember] public float duration { get; }
    [DataMember] public float start { get; set; }
    [DataMember] public float end { get; set; }
    [DataMember] public int keyNum { get; }

    public NoteDuration(float start, float dur, PianoKey key)
    {
        this.hasKeyBeenActivated = false;
        this.start = start;
        this.end = start + dur;
        this.keyNum = key.keyNum;
        this.duration = dur;
    }

    public NoteDuration() { }

    override public string ToString()
    {
        return hasKeyBeenActivated + " " +
            duration + " " +
            start + " " +
            end + " " +
            keyNum;
    }

}
