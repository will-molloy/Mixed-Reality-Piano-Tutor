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
            move(Vector3.down); // towards
        }
        if (Input.GetKey(KeyCode.PageUp))
        {
            move(Vector3.up); // away
        }
    }

    private void move(Vector3 direction)
    {
        spawnedSegments.ForEach(x => x.transform.Translate(direction * 0.1f));
    }

    public void SaveScoresAndViewFeedback(List<MidiEventStorage> midiEvents, List<NoteDuration> durs, float noteScale, float velocityIn, float offsetStartTime)
    {
        Debug.Log("Displaying scores");
        var evs = ConvertToNoteDurationFromMidiEventStorage(midiEvents, 0f, offsetStartTime);
        var res = MakeSegmentsFor_(evs, durs);
        var velocity = 1f / velocityIn * noteScale;
        var total = 0d;
        var correct = 0d;

        if (evs.Count > 0)
        {
            Debug.Log(evs.First().start);
            Debug.Log(durs.First().start);
        }
        else
        {
            Debug.LogWarning("No midievents recorded");
        }

        foreach (var e in res)
        {
            var key = e.Key;
            var list = e.Value;

            foreach (var m in list)
            {
                total++;
                var go = Instantiate(cube);
                var lmraway = piano.GetLMRAwayVectorsForKey(key, Sequencer.calcX(m.offsetY / velocity + m.scaleY / 2f / velocity));
                spawnedSegments.Add(go);
                var dummy = new GameObject();
                var k = piano.GetKeyObj(m.key);
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
        SaveScores(accuracy, midiEvents, durs, noteScale, velocityIn, offsetStartTime);
    }

    private void SaveScores(double accuracy, List<MidiEventStorage> midiEvents, List<NoteDuration> durs, float noteScale, float velocityIn, float offsetStartTime)
    {
        Debug.Log("Saving session - accuracy = " + String.Format("{0:0.0:0}", accuracy));
        var midiSessionDTO = new MidiSessionDto(RuntimeSettings.MIDI_FILE_NAME, accuracy, midiEvents, durs, noteScale, velocityIn, offsetStartTime);
        new MidiSessionController().putMidiSession(midiSessionDTO);
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
        var key = seg[0].key;
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

    private Dictionary<PianoKey, List<MidiSegment>> MakeSegmentsFor_(List<NoteDuration> midiEvents, List<NoteDuration> midiNotesFromFile)
    {
        var segMap = new Dictionary<PianoKey, List<MidiSegment>>();
        var premadeMap = midiNotesFromFile.GroupBy(e => e.key).ToDictionary(pianoKey => pianoKey.Key, notes => notes.ToList());
        var userMap = midiEvents.GroupBy(e => e.key).ToDictionary(pianoKey => pianoKey.Key, notes => notes.ToList());
        if (midiEvents == null || midiNotesFromFile == null)
        {
            Debug.LogError("Null Args recved at MakeSegmentsFor()");
        }

        foreach (var item in premadeMap)
        {
            var segments = new List<MidiSegment>();
            segMap[item.Key] = segments;
            var currMidiEvents = midiEvents.Where(e => e.key == item.Key).ToList();
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

    private Dictionary<PianoKey, List<MidiSegment>> MakeSegmentsFor(List<NoteDuration> midiEvents, List<NoteDuration> midiNotesFromFile)
    {
        var segMap = new Dictionary<PianoKey, List<MidiSegment>>();
        var premadeMap = midiNotesFromFile.GroupBy(e => e.key).ToDictionary(pianoKey => pianoKey.Key, notes => notes.ToList());
        // var premadeMap = PianoKeys.GetAllKeys().ToDictionary(e => e, e => midiNotesFromFile.Where(x => x.key.keyNum == e.keyNum).ToList());

        if (midiEvents == null || midiNotesFromFile == null)
        {
            Debug.LogError("Null Args recved at MakeSegmentsFor()");
        }

        foreach (var item in premadeMap)
        {
            var segments = new List<MidiSegment>();
            var currMidiEvents = midiEvents.Where(e => e.key == item.Key).ToList();
            var currMidiNoteFromFile = item.Value;
            int n = 0;
            int j = 0;
            // Both list should be sorted by their start time

            while (n < currMidiEvents.Count || j < currMidiNoteFromFile.Count)
            {
                if (n >= currMidiEvents.Count)
                {
                    break;
                }
                var userEvent = currMidiEvents[n];
                var refEvent = currMidiNoteFromFile[j];

                if (WithInTolerance(userEvent.start, refEvent.start))
                {
                    // Exact start
                    if (WithInTolerance(userEvent.end, refEvent.end))
                    {
                        // Exact End
                        segments.Add(new MidiSegment(MidiSegment.SegmentType.CORRECT, refEvent));
                    }
                    else if (userEvent.end < refEvent.end + tolerance)
                    {
                        // Early end
                        segments.Add(new MidiSegment(MidiSegment.SegmentType.CORRECT, userEvent));
                    }
                    else
                    {
                        // Late end
                        segments.Add(new MidiSegment(MidiSegment.SegmentType.CORRECT, userEvent, refEvent));
                        segments.Add(new MidiSegment(MidiSegment.SegmentType.EXTRA, refEvent, userEvent));
                        // Advance to ref event after the end of this userevent
                        while (j < currMidiNoteFromFile.Count)
                        {
                            if (userEvent.end < currMidiNoteFromFile[j].start - tolerance)
                            {
                                break;
                            }
                            j++;
                        }

                    }
                    n++;

                }
                else if (userEvent.start > refEvent.start - tolerance && userEvent.start < refEvent.end + tolerance)
                {
                    // Late Start
                    if (userEvent.end < refEvent.end + tolerance)
                    {
                        // Early end
                        segments.Add(new MidiSegment(MidiSegment.SegmentType.CORRECT, userEvent));
                    }
                    else
                    {
                        // Late end
                        segments.Add(new MidiSegment(MidiSegment.SegmentType.CORRECT, userEvent, refEvent));
                        segments.Add(new MidiSegment(MidiSegment.SegmentType.EXTRA, refEvent, userEvent));
                        // Advance to ref event after the end of this userevent
                        while (j < currMidiNoteFromFile.Count)
                        {
                            if (userEvent.end < currMidiNoteFromFile[j].start - tolerance)
                            {
                                break;
                            }
                            j++;
                        }
                    }
                    n++;
                }
                else if (userEvent.start < refEvent.start - tolerance)
                {
                    // Early Start
                    if (userEvent.end < refEvent.start - tolerance)
                    {
                        // very early end
                        segments.Add(new MidiSegment(MidiSegment.SegmentType.EXTRA, userEvent));
                    }
                    else if (userEvent.end < refEvent.end + tolerance)
                    {
                        // Early or exact end
                        segments.Add(new MidiSegment(MidiSegment.SegmentType.EXTRA, userEvent, refEvent));
                        segments.Add(new MidiSegment(MidiSegment.SegmentType.CORRECT, refEvent, userEvent));
                    }
                    else
                    {
                        // Late end
                        segments.Add(new MidiSegment(MidiSegment.SegmentType.EXTRA, userEvent, refEvent));
                        segments.Add(new MidiSegment(MidiSegment.SegmentType.CORRECT, refEvent));
                        segments.Add(new MidiSegment(MidiSegment.SegmentType.EXTRA, refEvent, userEvent));
                    }
                    // Advance to ref event after the end of this userevent
                    while (j < currMidiNoteFromFile.Count)
                    {
                        if (userEvent.end < currMidiNoteFromFile[j].start - tolerance)
                        {
                            break;
                        }
                        j++;
                    }
                    n++;
                }
                else if (userEvent.start > refEvent.end + tolerance)
                {
                    // Very late start (OOB)
                    // Make Missed segment for the whole ref and adv j
                    segments.Add(new MidiSegment(MidiSegment.SegmentType.MISSED, refEvent));
                    j++;
                    continue;
                }
            }
            var ret = FillGaps(segments, currMidiNoteFromFile);
            segMap.Add(item.Key, ret);
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

        public readonly PianoKey key;

        public MidiSegment(SegmentType type, float scaleY, float offsetY, PianoKey key)
        {
            this.type = type;
            this.scaleY = scaleY;
            this.offsetY = offsetY;
            this.key = key;
        }


        public MidiSegment(SegmentType type, NoteDuration from, NoteDuration to)
        {
            this.type = type;
            this.scaleY = to.end - from.start;
            this.offsetY = to.end;
            this.key = from.key;
        }

        public MidiSegment(SegmentType type, NoteDuration dur)
        {
            this.type = type;
            this.scaleY = dur.duration;
            this.offsetY = dur.start;
            this.key = dur.key;
        }

        override public string ToString()
        {
            return this.type.ToString() + " " + this.scaleY + "  " + this.offsetY;
        }

    }
}
