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

    public void DisplayScores(List<MidiEventStorage> midiEvents, List<NoteDuration> durs, float noteScale)
    {
        Debug.Log("Displaying scores");
        var evs = ConvertToNoteDurationFromMidiEventStorage(midiEvents, 0f);
        var res = MakeSegmentsFor(evs, durs);

        foreach (var e in res)
        {
            var key = e.Key;
            var list = e.Value;

            foreach (var m in list)
            {
                var go = Instantiate(cube);
                var lmraway = piano.GetLMRAwayVectorsForKey(key, Sequencer.calcX(m.offsetY + m.scaleY / 2f));
                var rder = go.GetComponent<Renderer>();
                Color color;
                switch (m.type)
                {
                    case MidiSegment.SegmentType.EXTRA:
                        color = Color.red;
                        break;
                    case MidiSegment.SegmentType.CORRECT:
                        color = Color.green;
                        break;
                    case MidiSegment.SegmentType.MISSED:
                        color = Color.yellow;
                        break;
                    default:
                        color = Color.black; // WTF C#??
                        break;
                }
                rder.material.color = color;
                spawnedSegments.Add(go);
                var dropdownScale = go.transform.localScale;
                go.transform.localScale = new Vector3(dropdownScale.x, m.scaleY, dropdownScale.z);
                go.transform.position = lmraway.away;
                var rotation = Quaternion.LookRotation(lmraway.centre - lmraway.away);
                go.transform.rotation = rotation;
                go.transform.Rotate(0, -90f, 90f);
            }
        }
    }

    public void ClearScores()
    {
        this.spawnedSegments.ForEach(e => Destroy(e));
        this.spawnedSegments.Clear();
    }

    private List<MidiSegment> FillGaps(List<MidiSegment> seg, List<NoteDuration> refs)
    {
        List<MidiSegment> temp = new List<MidiSegment>();
        if (seg.Count <= 1)
        {
            return seg;
        }
        var key = seg[0].key;
        for (int i = 1; i < seg.Count; i++)
        {
            var gapStart = seg[i - 1].offsetY;
            var gapEnd = seg[i].offsetY;

            for (int j = 0; j < refs.Count; j++)
            {
                var _ref = refs[i];
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


    private List<NoteDuration> ConvertToNoteDurationFromMidiEventStorage(List<MidiEventStorage> midiEvents, float defaultEndTiming)
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

    private Dictionary<PianoKey, List<MidiSegment>> MakeSegmentsFor(List<NoteDuration> midiEvents, List<NoteDuration> midiNotesFromFile)
    {
        var segMap = new Dictionary<PianoKey, List<MidiSegment>>();
        var premadeMap = midiNotesFromFile.GroupBy(e => e.key).ToDictionary(pianoKey => pianoKey.Key, notes => notes.ToList());

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
                        /* 
                        // Peek
                        if (n < midiEvents.Count - 1) {
                            var nextEvent = midiEvents[n+1];
                            if(nextEvent.start > refEvent.start - tolerance && nextEvent.start < refEvent.end + tolerance) {
                                // User has a break in the keys and the next key is within the note
                                segments.Add(new MidiSegment(MidiSegment.SegmentType.MISSED, userEvent, nextEvent));
                            } else if(nextEvent.start > refEvent.end + tolerance) {
                                // User did not press this key again in the duration of ref note
                                segments.Add(new MidiSegment(MidiSegment.SegmentType.MISSED, refEvent.end - userEvent.end, userEvent.end));
                            } else {
                                throw new System.Exception("this shouldnt happen!");
                            }
                        }
                        */
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
