using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Melanchall.DryWetMidi;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;
using Melanchall.DryWetMidi.Common;
using System.Linq;

[RequireComponent(typeof(PianoBuilder))]
public class Sequencer : MonoBehaviour
{

    private static long tick = 0;

    private MidiFile file = MidiFile.Read("Assets/MIDI/forelise.mid");

    private NotesManager noteManager;

    internal PianoBuilder piano;

    internal const float SPEED = .15f;
    // Use this for initialization

    [SerializeField]
    public GameObject dropDowns;

    public static Sequencer instance;
    void Start()
    {
		this.piano = GetComponent<PianoBuilder>();
        instance = this;
    }

    public void SpawnNotes(){
        if (noteManager == null){
        for (int i = 0; i < file.Chunks.Count; i++)
            {
                MidiChunk chunk = file.Chunks[i];
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

        SpawnNotesDropDown(noteManager.Notes.ToList());
    }

    // Update is called once per frame

    private void SpawnNotesDropDown(List<Note> notes) {
        Debug.Log("Spawning piano roll notes");
        pianoRollObjects.ForEach(o => GameObject.Destroy(o));
        pianoRollObjects.Clear();
        notes.ForEach(e => {
            var number = e.NoteNumber;
            var start = e.Time;
            var dur = e.Length;
            float x, y, z;
            var key = PianoKeys.GetKeyFor(number);
            if(key == null) {
                return;
            }
            y = start / 1000f;
            var scale = e.Length / 1000f - 0.01f;
            var obj = Instantiate(dropDowns);
            var awayVector = piano.GetPointingAwayVectorForKey(key);
            var keyPos = piano.GetKeyPositionForKey(key);
            var forwardVector = piano.GetForwardVectorForKey(key);
            var rot = Quaternion.LookRotation(awayVector);
            pianoRollObjects.Add(obj);
            var dropdownScale = obj.transform.localScale;
            obj.transform.localScale = new Vector3(dropdownScale.x, scale, dropdownScale.z);
            obj.transform.position = keyPos + awayVector * y + forwardVector * 0.05f;
            //obj.transform.rotation = rot;
            //obj.transform.localRotation = new Quaternion(-rot.x, -rot.y, -rot.z, -rot.w);
            var angle = (Mathf.Atan(5f)) * Mathf.Rad2Deg;
            obj.transform.eulerAngles = new Vector3(angle, 0, 0);
            var renderer = obj.GetComponent<Renderer>();
            renderer.material.color = key.color == KeyColor.Black ? Color.black : Color.white;
            var rb = obj.GetComponent<Rigidbody>();
            rb.velocity = -awayVector * 0.1f;
        });

    }

    private List<GameObject> pianoRollObjects = new List<GameObject>();

    public List<int> GetNotesAt(long tick)
    {
        var notes = this.noteManager.Notes.ToList();
        var notesAtTick = new List<int>();
        notes.ForEach(note =>
        {
            var on = note.Time;
            var off = note.Time + note.Length;
            if (off >= tick && on <= tick)
            {
                notesAtTick.Add(note.NoteNumber);
            }
        });
        return notesAtTick;
    }
}