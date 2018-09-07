using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Sequencer))]
[RequireComponent(typeof(PianoBuilder))]
public class ScoreView : MonoBehaviour
{

    private PianoBuilder piano;
    private List<GameObject> spawnedSegments;

    [SerializeField]
    private GameObject cube;

    [SerializeField]
    private const float tolerance = 0.1f;

    void Start()
    {
        this.piano = GetComponent<PianoBuilder>();
        this.spawnedSegments = new List<GameObject>();
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.PageDown))
        {
            moveSegments(Vector3.down); // towards
        }
        if (Input.GetKey(KeyCode.PageUp))
        {
            moveSegments(Vector3.up); // away
        }
    }

    private void moveSegments(Vector3 direction)
    {
        spawnedSegments.ForEach(x => x.transform.Translate(direction * 0.1f));
    }

    public void SaveScoresAndViewFeedback(MidiSessionDto session, bool save = true)
    {
        var events = session.midiEvents;
        var notes = session.noteDurations;

        Debug.Log("User events: " + events.Count());
        Debug.Log("Track events: " + notes.Count());
        events.ForEach(x => Debug.Log(x));

        var segments = MakeSegmentsFor(events, notes);
        var velocity = 1f / session.velocityIn * session.noteScale;
        var total = 0d;
        var correct = 0d;

        if (events.Count == 0)
        {
            Debug.LogWarning("No midievents recorded");
        }

        foreach (var e in segments)
        {
            var keyNum = e.Key;
            var list = e.Value;

            foreach (var m in list)
            {
                var key = PianoKeys.GetKeyFor(keyNum);
                total++;
                var go = Instantiate(cube);
                var lmraway = piano.GetLMRAwayVectorsForKey(key, Sequencer.calcX(m.offsetY / velocity + m.scaleY / 2f / velocity));
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
        var score = (int)(accuracy * 100);
        piano.showText(session.FormattedTrackName + ": " + score + "%", 50, false);

        if (save)  // dont resave a loaded session
        {
            Debug.Log("Saving session - score = " + accuracy * 100);
            // Same but update accuracy
            var midiSessionDTO = new MidiSessionDto(RuntimeSettings.MIDI_FILE_NAME, accuracy, events, session.noteDurations, session.noteScale, session.velocityIn, session.offsetStartTime);
            new MidiSessionController().putMidiSession(midiSessionDTO);
        }
    }

    public void ConvertEventsSaveScoresAndViewFeedback(List<MidiEventStorage> midiEvents, List<NoteDuration> durs, float noteScale, float velocityIn, float offsetStartTime)
    {
        Debug.Log("Displaying scores");
        var evs = ConvertToNoteDurationFromMidiEventStorage(midiEvents, 0f, offsetStartTime);
        var res = MakeSegmentsFor(evs, durs);
        var velocity = 1f / velocityIn * noteScale;
        var total = 0d;
        var correct = 0d;

        var midiSessionDTO = new MidiSessionDto(RuntimeSettings.MIDI_FILE_NAME, 0, evs, durs, noteScale, velocityIn, offsetStartTime);
        SaveScoresAndViewFeedback(midiSessionDTO);
    }

    public void ClearScores()
    {
        this.spawnedSegments.ForEach(e => Destroy(e));
        this.spawnedSegments.Clear();
    }

    private List<MidiSegment> FillGaps(List<MidiSegment> seg, List<NoteDuration> refs)
    {
        List<MidiSegment> temp = new List<MidiSegment>();
        refs.ForEach(e =>
        {
            temp.Add(new MidiSegment(MidiSegment.SegmentType.MISSED, e));
        });
        seg.AddRange(temp);
        return seg;

        if (seg.Count < 1)
        {
            refs.ForEach(e => temp.Add(new MidiSegment(MidiSegment.SegmentType.MISSED, e)));
            return temp;
        }
        var key = seg[0].keyNum;
        for (int i = 1; i < seg.Count; i++)
        {
            var gapStart = seg[i - 1].offsetY;
            var gapEnd = seg[i].offsetY;

            for (int j = 0; j < refs.Count; j++)
            {
                var _ref = refs[j];
                if (gapStart > _ref.start)
                {
                    var m = Mathf.Min(_ref.end, gapEnd);
                    temp.Add(new MidiSegment(MidiSegment.SegmentType.MISSED, gapStart, m, key));
                }
            }
        }
        seg.AddRange(temp);
        return seg;
    }

    private List<NoteDuration> ConvertToNoteDurationFromMidiEventStorage(List<MidiEventStorage> midiEvents, float defaultEndTiming, float timeOffset)
    {
        var list = new List<NoteDuration>();
        for (; ; )
        {
            // Until we have an empty list, keep searching notes and end of it
            if (midiEvents.Count == 0) break;
            var head = midiEvents.First();
            if (head.isEnd)
            {
                Debug.LogError("Unexpected end of note");
            }
            var keyNum = head.keyNum;
            for (int i = 1; i < midiEvents.Count(); i++)
            {
                if (midiEvents[i].keyNum == keyNum)
                {
                    if (!midiEvents[i].isEnd)
                    {
                        Debug.LogError("Double Start of a note");
                        return list;
                    }
                    else
                    {
                        var item = midiEvents[i];
                        list.Add(new NoteDuration(head.time, item.time - head.time, PianoKeys.GetKeyFor(keyNum)));
                        midiEvents.Remove(head);
                        midiEvents.Remove(item);
                        break;
                    }
                }
            }
        }
        return list;
    }

    private bool WithInTolerance(float a, float of)
    {
        return of + tolerance >= a && of - tolerance <= a;
    }

    private Dictionary<int, List<MidiSegment>> MakeSegmentsFor(List<NoteDuration> userEvents, List<NoteDuration> trackEvents)
    {
        if (userEvents == null || trackEvents == null)
        {
            Debug.LogError("Null Args recved at MakeSegmentsFor()");
        }
        var segMap = new Dictionary<int, List<MidiSegment>>();

        var userMap = new Dictionary<int, List<NoteDuration>>();
        foreach (var item in userEvents)
        {
            var newList = new List<NoteDuration>();
            var list = userMap.ContainsKey(item.keyNum) ? userMap[item.keyNum] : newList;
            list.Add(item);
            userMap[item.keyNum] = list;
        }

        var trackMap = new Dictionary<int, List<NoteDuration>>();
        foreach (var item in trackEvents)
        {
            var newList = new List<NoteDuration>();
            var list = trackMap.ContainsKey(item.keyNum) ? trackMap[item.keyNum] : newList;
            list.Add(item);
            trackMap[item.keyNum] = list;
        }
        // var trackMap = trackEvents.GroupBy(e => e.key).ToDictionary(pianoKey => pianoKey.Key, notes => notes.ToList());
        // var userMap = userEvents.GroupBy(e => e.key).ToDictionary(pianoKey => pianoKey.Key, notes => notes.ToList());

        foreach (var item in trackMap)
        {
            var segments = new List<MidiSegment>();
            segMap[item.Key] = segments;
            var currMidiEvents = userEvents.Where(e => e.keyNum == item.Key).ToList();
            var currMidiNoteFromFile = item.Value;
            item.Value.ForEach(e => segments.Add(new MidiSegment(MidiSegment.SegmentType.MISSED, e)));

            if (!userMap.ContainsKey(item.Key))
            {
                continue;
            }

            foreach (var userSeg in userMap[item.Key])
            {
                segments.Add(new MidiSegment(MidiSegment.SegmentType.EXTRA, userSeg));
            }

            item.Value.ForEach(e =>
            {
                userMap[item.Key].ForEach(u =>
                {
                    if (u.start >= e.start && u.end < e.end && u.start < e.end)
                    {
                        segments.Add(new MidiSegment(MidiSegment.SegmentType.CORRECT, e, u));
                    }
                    else if (u.start >= e.start && u.end > e.end && u.start < e.end)
                    {
                        segments.Add(new MidiSegment(MidiSegment.SegmentType.CORRECT, u, e));
                    }
                    else if (u.start <= e.start && u.end >= e.end)
                    {
                        segments.Add(new MidiSegment(MidiSegment.SegmentType.CORRECT, e));
                    }
                    else if (u.start <= e.start && u.end <= e.end && u.end > e.start)
                    {
                        segments.Add(new MidiSegment(MidiSegment.SegmentType.CORRECT, e, u));
                    }
                });
            });

        }
        return segMap;
    }

    public struct MidiSegment
    {
        public enum SegmentType
        {
            MISSED, EXTRA, CORRECT
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

        public MidiSegment(SegmentType type, NoteDuration from, NoteDuration to)
        {
            this.type = type;
            this.scaleY = to.end - from.start;
            this.offsetY = to.end;
            this.keyNum = from.keyNum;
        }

        public MidiSegment(SegmentType type, NoteDuration dur)
        {
            this.type = type;
            this.scaleY = dur.duration;
            this.offsetY = dur.start;
            this.keyNum = dur.keyNum;
        }

        override public string ToString()
        {
            return this.type.ToString() + " " + this.scaleY + "  " + this.offsetY;
        }

    }
}
