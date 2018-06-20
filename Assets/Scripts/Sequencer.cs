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
    [SerializeField]
    private string midiFileName;
    private MidiFile midiFile;
    private NotesManager noteManager;
    [SerializeField]
    private GameObject pianoRollObject;
    private List<GameObject> pianoRollObjects = new List<GameObject>();
    public static Sequencer instance;
    private PianoBuilder piano;

    void Start()
    {
        piano = GetComponent<PianoBuilder>();
        instance = this;
        midiFile = MidiFile.Read(midiFileName); // TODO get from MIDISelection instance 
    }

    public void SpawnNotes()
    {
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
        SpawnNotesDropDown(noteManager.Notes.ToList());
    }

    private void ClearPianoRoll()
    {
        Debug.Log("Clearing piano roll");
        pianoRollObjects.ForEach(o => GameObject.Destroy(o));
        pianoRollObjects.Clear();
    }

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
            var obj = Instantiate(pianoRollObject);
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

}