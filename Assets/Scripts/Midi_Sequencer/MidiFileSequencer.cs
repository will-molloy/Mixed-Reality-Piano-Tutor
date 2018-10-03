using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using End_Feedback;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;
using UnityEngine;
using Virtual_Piano;

namespace Midi_Sequencer
{
    /// <summary>
    ///     - Builds piano roll from MIDI file after virtual piano is built
    ///     - This includes blocks, auxiliary lines, indicator markers
    ///     - Also notifies ScoreView of users final score and events to build end of session feedback
    ///         - Or when user presses Escape key
    ///     - Also notifies game elements (in PianoBuilder) of every beats accuracy
    /// </summary>
    [RequireComponent(typeof(PianoBuilder))]
    [RequireComponent(typeof(MidiDeviceController))]
    [RequireComponent(typeof(ScoreView))]
    public sealed class MidiFileSequencer : MonoBehaviour
    {
        public static Dictionary<PianoKey, Color> colorDict = new Dictionary<PianoKey, Color>();
        private readonly Dictionary<PianoKey, GameObject> keyAwayDir = new Dictionary<PianoKey, GameObject>();

        private readonly float notesScale = 1f;
        private readonly float offsetStart = 0.5f * 1f / 0.5f;

        private readonly Dictionary<PianoKey, List<GameObject>> pianoRollDict = new Dictionary<PianoKey, List<GameObject>>();

        private List<Coroutine> crtHolder;
        private float deltaTime;
        [SerializeField] private GameObject fineLine;
        private readonly List<GameObject> fineLines = new List<GameObject>();

        private bool gameStarted;

        private MidiDeviceController _midiDeviceController;

        private MidiFile midiFile;
        public List<NoteDuration> noteDurations;
        private PianoBuilder piano;
        [SerializeField] private GameObject pianoRollObject;
        private readonly List<GameObject> pianoRollObjects = new List<GameObject>();
        private ScoreView scoreView;
        public Dictionary<PianoKey, bool> shouldKeyBeOn = new Dictionary<PianoKey, bool>();

        private float startTime = -1;
        private TempoMapManager tempoMapManager;

        private float timeBetweenBeats;
        private int totalBeats;
        private int totalChecked;
        private int totalMissed;

        public static Color MakeColorFromHex(int hex)
        {
            var r = (byte) ((hex >> 16) & 0xFF);
            var g = (byte) ((hex >> 8) & 0xFF);
            var b = (byte) (hex & 0xFF);
            return new Color32(r, g, b, 255);
        }

        private void Start()
        {
            PianoKeys.GetAllKeys().ForEach(e => { shouldKeyBeOn[e] = false; });
            piano = GetComponent<PianoBuilder>();
            crtHolder = new List<Coroutine>();
            _midiDeviceController = GetComponent<MidiDeviceController>();
            scoreView = GetComponent<ScoreView>();
            noteDurations = new List<NoteDuration>();
            foreach (var item in PianoKeys.GetAllKeys()) pianoRollDict.Add(item, new List<GameObject>());
            var ws = PianoKeys.GetAllKeys().Where(e => e.color == KeyColor.White).ToList();
            for (var i = 0; i < ws.Count(); i++) colorDict[ws[i]] = MakeColorFromHex(0xffffff);
            PianoKeys.GetAllKeys().Where(e => e.color == KeyColor.Black).ToList()
                .ForEach(e => colorDict[e] = MakeColorFromHex(0x0000ff));
        }

        public void LoadMidiFile(string file)
        {
            Debug.Log("Loading MIDI file: " + file + ", Mode: " + (RuntimeSettings.IS_PLAY_MODE ? "Play" : "Practice") +
                      ", Difficulty: " + RuntimeSettings.DIFFICULTY + ", GameSpeed: " + RuntimeSettings.GAME_SPEED +
                      ", User: " + RuntimeSettings.USER);
            midiFile = MidiFile.Read(file);
            SpawnNotes();
        }

        public void LoadMidiFile()
        {
            Debug.Log("User: " + RuntimeSettings.USER);
            var file = RuntimeSettings.MIDI_FILE_NAME;
            if (file != null)
                LoadMidiFile(file);
            else
                Debug.LogError("No MIDI file set in RuntimeSettings.");
        }

        public void SpawnNotes()
        {
            if (midiFile == null) throw new Exception("No midifile loaded, use LoadMidiFile() to load");
            tempoMapManager = midiFile.ManageTempoMap();
            var tempomap = tempoMapManager.TempoMap;

            Debug.Log(tempomap.TimeDivision);
            tempomap.TimeSignature.AtTime(0);
            Debug.Log(tempomap.Tempo.AtTime(0));
            SpawnNotesDropDown(midiFile.GetNotes().ToList());
        }

        public float GetStartTime()
        {
            return startTime;
        }

        private void ClearPianoRoll()
        {
            gameStarted = false;
            Debug.Log("Clearing piano roll");
            pianoRollObjects.ForEach(o => Destroy(o));
            pianoRollObjects.Clear();
            foreach (var i in pianoRollDict)
            {
                i.Value.ForEach(o => Destroy(o));
                i.Value.Clear();
            }

            noteDurations.Clear();
            _midiDeviceController.ClearMidiEventStorage();
            crtHolder.ForEach(e => StopCoroutine(e));
        }

        public void StartGame()
        {
            if (!gameStarted)
            {
                gameStarted = true;
                startTime = Time.time;
                crtHolder.Add(StartCoroutine(PulseCoroutine(timeBetweenBeats, totalBeats)));
                crtHolder.Add(StartCoroutine(TriggerChecks(timeBetweenBeats, totalBeats)));
            }
        }

        public static float calcX(float y)
        {
            return Mathf.Sqrt(y * y / 26f);
        }

        private void SpawnNotesDropDown(List<Note> notes)
        {
            Debug.Log("Spawning piano roll notes");
            ClearPianoRoll();
            notes.ForEach(e =>
            {
                var number = e.NoteNumber;
                float y;
                var key = PianoKeys.GetKeyFor(number);
                if (key == null) return;
                var startMusical = (MusicalTimeSpan) e.TimeAs(TimeSpanType.Musical, tempoMapManager.TempoMap);
                var lengthMusical = e.LengthAs(TimeSpanType.Musical, tempoMapManager.TempoMap);
                y = (float) startMusical.Numerator / startMusical.Denominator * notesScale;
                var delta = (MusicalTimeSpan) lengthMusical;
                var scale = (float) delta.Numerator / delta.Denominator * notesScale;
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
                pianoRollObjects.Add(obj);
                var dropdownScale = obj.transform.localScale;
                obj.transform.localScale = new Vector3(dropdownScale.x, scale, dropdownScale.z);
                var rotation = Quaternion.LookRotation(keyPos - awayVector);
                obj.transform.rotation = rotation;
                obj.transform.Rotate(0, -90f, 90f);
                obj.transform.position = lmraway2.away;

                var rendererObj = obj.GetComponent<Renderer>();
                rendererObj.material.color = colorDict[key];

                var expectTime = ((lmraway2.away - keyPos).magnitude + scale / 2) / RuntimeSettings.GAME_SPEED;
                var expectEnd = scale / RuntimeSettings.GAME_SPEED;

                pianoRollDict[key].Add(dummy);
                noteDurations.Add(new NoteDuration(expectTime, expectEnd, key));
            });

            // Spawn fine lines (horizontal)
            var lastNoteMusicalStart =
                (MusicalTimeSpan) notes.Last().TimeAs(TimeSpanType.Musical, tempoMapManager.TempoMap);
            var lastNoteMusicalLen =
                (MusicalTimeSpan) notes.Last().LengthAs(TimeSpanType.Musical, tempoMapManager.TempoMap);
            var lastNoteMuscialEnd = lastNoteMusicalStart + lastNoteMusicalLen;

            var lastBeatY = (float) lastNoteMuscialEnd.Numerator / lastNoteMusicalLen.Denominator * notesScale;
            var beatDelta = 1f * notesScale;

            var midKey = PianoKeys.GetKeyFor(PianoBuilder.CENTRE);
            var totalBeatsI = (int) (lastBeatY / beatDelta);

            for (var i = 1; i <= totalBeatsI; i++)
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
                fineLines.Add(dummy);
            }

            timeBetweenBeats = beatDelta / RuntimeSettings.GAME_SPEED;
            totalBeats = totalBeatsI;
        }

        private IEnumerator TriggerChecks(float time, int totalBeats)
        {
            const int freq = 16;
            for (var i = 0; i <= totalBeats; i++)
            {
                var total = 0;
                var totalMiss = 0;
                for (var j = 0; j < freq; j++)
                {
                    // Check for accuracy
                    var ons = _midiDeviceController.GetOnKeys();
                    var deltaTime = Time.time - startTime;
                    var eligible = noteDurations.FindAll(e => e.start <= deltaTime && e.end >= deltaTime);
                    eligible.ForEach(e =>
                    {
                        total++;
                        if (!ons.Contains(PianoKeys.GetKeyFor(e.keyNum))) totalMiss++;
                    });
                    yield return new WaitForSeconds(time / freq);
                }

                piano.PutInstantFeedback(total, totalMiss, totalBeats);
            }
        }

        public bool IsGamedStarted()
        {
            return gameStarted;
        }

        private IEnumerator PulseCoroutine(float time, int totalBeats)
        {
            for (var i = 0; i <= totalBeats; i++)
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
                ClearPianoRoll();
                startTime = -1f;
            }

            if (startTime < 0f) return;
            var deltaT = Time.time - startTime;

            var minDistDict = new Dictionary<PianoKey, float>();
            foreach (var i in PianoKeys.GetAllKeys()) minDistDict[i] = 2f;
            noteDurations.ForEach(note =>
            {
                if (!note.hasKeyBeenActivated && deltaT >= note.start - note.duration && deltaT < note.end - note.duration)
                {
                    piano.ActivateKey(note.keyNum, Color.red, note.duration);
                    note.hasKeyBeenActivated = true;
                }

                if (deltaT >= note.start - note.duration && deltaT < note.end - note.duration)
                {
                    minDistDict[PianoKeys.GetKeyFor(note.keyNum)] = 0;
                    return;
                }

                if (deltaT > note.end)
                {
                    return;
                }

                if (deltaT >= note.start - 2f && deltaT <= note.start)
                    minDistDict[PianoKeys.GetKeyFor(note.keyNum)] = Mathf.Min(Mathf.Abs(note.start - deltaT),
                        minDistDict[PianoKeys.GetKeyFor(note.keyNum)]);
            });
            foreach (var item in minDistDict)
            {
                float h, s, v;
                Color.RGBToHSV(colorDict[item.Key], out h, out s, out v);
                if (item.Value == 2f)
                    s = 0f;
                else
                    s = (2 - item.Value) / 2 * s;
                var newc = Color.HSVToRGB(h, s, v);
                newc.a = (2 - item.Value) / 2;
                if (newc.a >= 1f)
                    shouldKeyBeOn[item.Key] = true;
                else
                    shouldKeyBeOn[item.Key] = false;
                piano.UpdateDiskColor(item.Key, newc);
            }

            if (noteDurations.Last().hasKeyBeenActivated || Input.GetKeyDown(KeyCode.Escape))
            {
                scoreView.ConvertEventsSaveScoresAndViewFeedback(_midiDeviceController.GetMidiEvents(),
                    noteDurations.Select(x => new CompressedNoteDuration(x)).ToList(), notesScale,
                    RuntimeSettings.GAME_SPEED, startTime);
                ClearPianoRoll();
                startTime = -1f;
            }

            if (gameStarted)
            {
                var step = RuntimeSettings.GAME_SPEED * Time.deltaTime;
                foreach (var item in pianoRollDict)
                {
                    if (item.Value.Count == 0) continue;
                    var lmr = piano.GetLMRAwayVectorsForKey(item.Key);
                    foreach (var obj in item.Value)
                    {
                        obj.transform.Translate(0, -step, 0);
                        if (obj.transform.childCount > 0)
                        {
                            var co = obj.transform.GetChild(0);
                            var childScale = co.transform.localScale;
                            var mag = (obj.transform.position - lmr.centre).magnitude;
                            if (mag < childScale.y)
                            {
                                var cs = co.transform.localScale.y;

                                obj.transform.localScale = new Vector3(1f, mag / cs, 1f);
                            }

                            if (mag < 0.01f) DestroyImmediate(co.gameObject);
                        }
                    }
                }

                foreach (var obj in fineLines)
                {
                    var center = PianoKeys.GetKeyFor(PianoBuilder.CENTRE);
                    var centerAway = keyAwayDir[center];
                    obj.transform.position =
                        Vector3.MoveTowards(obj.transform.position, centerAway.transform.position, step);
                }
            }
        }
    }

    /// <summary>
    ///     - To compare user MIDI events with MIDI file notes
    /// </summary>
    public struct NoteDuration
    {
        public bool hasKeyBeenActivated { get; set; }
        public float duration { get; }
        public float start { get; }
        public float end { get; }
        public int keyNum { get; }

        public NoteDuration(float start, float dur, PianoKey key)
        {
            hasKeyBeenActivated = false;
            this.start = start;
            end = start + dur;
            keyNum = key.keyNum;
            duration = dur;
        }
    }

    /// <summary>
    ///     - Compressed to save JSON file space
    /// </summary>
    [DataContract]
    public class CompressedNoteDuration
    {
        public CompressedNoteDuration(NoteDuration noteDuration)
        {
            duration = noteDuration.duration;
            start = noteDuration.start;
            end = noteDuration.end;
            keyNum = noteDuration.keyNum;
        }

        public CompressedNoteDuration(float start, float dur, PianoKey key)
        {
            this.start = start;
            end = start + dur;
            keyNum = key.keyNum;
            duration = dur;
        }

        public CompressedNoteDuration() // For JSON library
        {
        }

        [DataMember] public float duration { get; set; }
        [DataMember] public float start { get; set; }
        [DataMember] public float end { get; set; }
        [DataMember] public int keyNum { get; set; }

        public override string ToString()
        {
            return
                duration + " " +
                start + " " +
                end + " " +
                keyNum;
        }
    }
}