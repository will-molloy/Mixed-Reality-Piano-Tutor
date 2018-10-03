using System.Collections.Generic;
using System.Linq;
using Midi_Sequencer;
using Midi_Session;
using UnityEngine;
using Virtual_Piano;

namespace End_Feedback
{
    /// <summary>
    ///     - Constructs and controls end of session feedback view
    ///         - Either from Sequencer/PianoBuilder at end of song or Escape key press
    ///         - Or from History UI using a MidiSessionDTO
    /// </summary>
    [RequireComponent(typeof(MidiFileSequencer))]
    [RequireComponent(typeof(PianoBuilder))]
    public class ScoreView : MonoBehaviour
    {
        [SerializeField] private GameObject cube;

        private PianoBuilder piano;
        private List<GameObject> spawnedSegments;

        private void Start()
        {
            piano = GetComponent<PianoBuilder>();
            spawnedSegments = new List<GameObject>();
        }

        private void Update()
        {
            if (Input.GetKey(KeyCode.PageDown)) MoveSegments(Vector3.down); // move towards
            if (Input.GetKey(KeyCode.PageUp)) MoveSegments(Vector3.up); // move away
        }

        private void MoveSegments(Vector3 direction)
        {
            spawnedSegments.ForEach(x => x.transform.Translate(direction * 0.1f));
        }

        public void SaveScoresAndViewFeedback(MidiSessionDto session, bool save = true)
        {
            var userEvents = session.userNoteDurations;
            var trackEvents = session.trackNoteDurations;

            Debug.Log("User events: " + userEvents.Count());
            Debug.Log("Track events: " + trackEvents.Count());

            var segments = MakeSegmentsFor(userEvents, trackEvents);
            var velocity = 1f / session.velocityIn * session.noteScale;
            var total = 0d;
            var correct = 0d;

            if (userEvents.Count == 0) Debug.LogWarning("No midievents recorded");

            foreach (var e in segments)
            {
                var keyNum = e.Key;
                var list = e.Value;

                foreach (var m in list)
                {
                    var key = PianoKeys.GetKeyFor(keyNum);
                    total++;
                    var go = Instantiate(cube);
                    var lmraway = piano.GetLMRAwayVectorsForKey(key,
                        MidiFileSequencer.calcX(m.offsetY / velocity + m.scaleY / 2f / velocity));
                    spawnedSegments.Add(go);
                    var dummy = new GameObject();
                    var k = piano.GetKeyObj(key);
                    dummy.transform.SetParent(k.transform);
                    go.transform.SetParent(piano.transform);
                    var dropdownScale = go.transform.localScale;
                    go.transform.localScale = new Vector3(dropdownScale.x, m.scaleY / velocity, dropdownScale.z);
                    go.transform.position = lmraway.away;

                    Color color;
                    switch (m.type)
                    {
                        case MidiSegment.SegmentType.EXTRA:
                            color = Color.red;
                            go.transform.localScale += new Vector3(.0001f, .0001f, .0001f);
                            break;
                        case MidiSegment.SegmentType.CORRECT:
                            color = Color.green;
                            go.transform.localScale += new Vector3(.0002f, .0002f, .0002f);
                            correct++;
                            break;
                        case MidiSegment.SegmentType.MISSED:
                            color = Color.yellow;
                            break;
                        default:
                            color = Color.black; // WTF C#??
                            break;
                    }

                    var rder = go.GetComponent<Renderer>();
                    rder.material.color = color;
                    var rotation = Quaternion.LookRotation(lmraway.centre - lmraway.away);
                    go.transform.rotation = rotation;
                    go.transform.Rotate(0, -90f, 90f);
                }
            }

            var accuracy = correct / total;

            Debug.Log("Displaying end feedback text");
            int score;
            if (save)
                score = (int) (accuracy * 100);
            else
                score = (int) (session.Accuracy * 100);
            piano.showText(session.FormattedTrackName + ": " + score + "%", 50, false);

            if (save) // dont resave a loaded session
            {
                Debug.Log("Saving session - score = " + accuracy * 100);
                // Same but update accuracy
                var midiSessionDTO = new MidiSessionDto(RuntimeSettings.MIDI_FILE_NAME, accuracy, userEvents,
                    session.trackNoteDurations, session.noteScale, session.velocityIn, session.offsetStartTime);
                new MidiSessionController().putMidiSession(midiSessionDTO);
            }
        }

        public void ConvertEventsSaveScoresAndViewFeedback(List<MidiEventStorage> midiEvents,
            List<CompressedNoteDuration> durs, float noteScale, float velocityIn, float offsetStartTime)
        {
            Debug.Log("Displaying scores");
            var evs = ConvertToNoteDurationFromMidiEventStorage(midiEvents);
            MakeSegmentsFor(evs, durs);
            var total = 0d;
            var correct = 0d;

            var midiSessionDTO = new MidiSessionDto(RuntimeSettings.MIDI_FILE_NAME, 0, evs, durs, noteScale, velocityIn,
                offsetStartTime);
            SaveScoresAndViewFeedback(midiSessionDTO);
        }

        public void ClearScores()
        {
            spawnedSegments.ForEach(e => Destroy(e));
            spawnedSegments.Clear();
        }

        private List<CompressedNoteDuration> ConvertToNoteDurationFromMidiEventStorage(List<MidiEventStorage> midiEvents)
        {
            var list = new List<CompressedNoteDuration>();
            while (midiEvents.Count() > 0)
            {
                // Until we have an empty list, keep searching notes and end of it
                if (midiEvents.Count == 0) break;
                var head = midiEvents.First();
                if (head.isEnd) Debug.LogError("Unexpected end of note");
                var keyNum = head.keyNum;
                for (var i = 1; i < midiEvents.Count(); i++)
                    if (midiEvents[i].keyNum == keyNum)
                    {
                        if (!midiEvents[i].isEnd)
                        {
                            Debug.LogError("Double Start of a note");
                            return list;
                        }

                        var item = midiEvents[i];
                        list.Add(new CompressedNoteDuration(head.time, item.time - head.time, PianoKeys.GetKeyFor(keyNum)));
                        midiEvents.Remove(head);
                        midiEvents.Remove(item);
                        break;
                    }
            }

            return list;
        }

        private Dictionary<int, List<MidiSegment>> MakeSegmentsFor(List<CompressedNoteDuration> userEvents,
            List<CompressedNoteDuration> trackEvents)
        {
            if (userEvents == null || trackEvents == null) Debug.LogError("Null Args recved at MakeSegmentsFor()");
            var segMap = new Dictionary<int, List<MidiSegment>>();
            var trackMap = trackEvents.GroupBy(e => e.keyNum)
                .ToDictionary(pianoKey => pianoKey.Key, notes => notes.ToList());
            var userMap = userEvents.GroupBy(e => e.keyNum).ToDictionary(pianoKey => pianoKey.Key, notes => notes.ToList());

            PianoKeys.GetAllKeys().Select(x => x.keyNum).Where(x => !trackMap.ContainsKey(x)).ToList()
                .ForEach(x => trackMap.Add(x, new List<CompressedNoteDuration>()));
            foreach (var item in trackMap)
            {
                var segments = new List<MidiSegment>();
                segMap[item.Key] = segments;
                item.Value.ForEach(e => segments.Add(new MidiSegment(MidiSegment.SegmentType.MISSED, e)));

                if (!userMap.ContainsKey(item.Key)) continue;

                foreach (var userSeg in userMap[item.Key])
                    segments.Add(new MidiSegment(MidiSegment.SegmentType.EXTRA, userSeg));

                item.Value.ForEach(e =>
                {
                    userMap[item.Key].ForEach(u =>
                    {
                        if (u.start >= e.start && u.end < e.end && u.start < e.end)
                            segments.Add(new MidiSegment(MidiSegment.SegmentType.CORRECT, e, u));
                        else if (u.start >= e.start && u.end > e.end && u.start < e.end)
                            segments.Add(new MidiSegment(MidiSegment.SegmentType.CORRECT, u, e));
                        else if (u.start <= e.start && u.end >= e.end)
                            segments.Add(new MidiSegment(MidiSegment.SegmentType.CORRECT, e));
                        else if (u.start <= e.start && u.end <= e.end && u.end > e.start)
                            segments.Add(new MidiSegment(MidiSegment.SegmentType.CORRECT, e, u));
                    });
                });
            }

            return segMap;
        }

        public struct MidiSegment
        {
            public enum SegmentType
            {
                MISSED,
                EXTRA,
                CORRECT
            }

            public readonly SegmentType type;
            public readonly float scaleY;
            public readonly float offsetY;

            public readonly int keyNum;

            public MidiSegment(SegmentType type, float scaleY, float offsetY, int keyNum)
            {
                this.type = type;
                this.scaleY = scaleY;
                this.offsetY = offsetY;
                this.keyNum = keyNum;
            }

            public MidiSegment(SegmentType type, CompressedNoteDuration from, CompressedNoteDuration to)
            {
                this.type = type;
                scaleY = to.end - from.start;
                offsetY = to.end;
                keyNum = from.keyNum;
            }

            public MidiSegment(SegmentType type, CompressedNoteDuration dur)
            {
                this.type = type;
                scaleY = dur.duration;
                offsetY = dur.start;
                keyNum = dur.keyNum;
            }

            public override string ToString()
            {
                return type + " " + scaleY + "  " + offsetY;
            }
        }
    }
}