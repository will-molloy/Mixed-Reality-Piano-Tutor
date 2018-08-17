using System.Collections.Generic;
using UnityEngine;
using Sanford.Multimedia.Midi;
using System.Linq;

/// <summary>  
/// - Handles MIDI device 
/// </summary>  
[RequireComponent(typeof(Sequencer))]
sealed public class MidiController : MonoBehaviour
{
    protected InputDevice inputDevice;

    private List<MidiEventStorage> midiEvents;
    private List<MidiEventStorage> mockEvents;
    private HashSet<PianoKey> notesOn;
    private Sequencer seq;

    public float startTime;

    void Start()
    {
        notesOn= new HashSet<PianoKey>();
        mockEvents = new List<MidiEventStorage>();
        mockEvents.Add(new MidiEventStorage(55, false, 0.5f));
        mockEvents.Add(new MidiEventStorage(55, true, 0.8f));
        mockEvents.Add(new MidiEventStorage(74, false, 1f));
        mockEvents.Add(new MidiEventStorage(74, true, 5f));
        mockEvents.Add(new MidiEventStorage(75, false, 1f));
        mockEvents.Add(new MidiEventStorage(75, true, 5f));
        mockEvents.Add(new MidiEventStorage(76, false, 2f));
        mockEvents.Add(new MidiEventStorage(76, true, 2.5f));
        this.midiEvents = new List<MidiEventStorage>();
        seq = GetComponent<Sequencer>();

        if (InputDevice.DeviceCount < 1)
        {
            Debug.LogError("No device found for MIDI input.");
            return;
        }
        if (InputDevice.DeviceCount > 1)
        {
            Debug.LogError("Too many devices found for MIDI input.");
            return;
        }
        inputDevice = new InputDevice(0);
        inputDevice.ChannelMessageReceived += handleChannelMsg;
        inputDevice.ChannelMessageReceived += storeMidiEvent;

        inputDevice.StartRecording();
        Debug.Log("MIDI device inited");
        ClearMidiEventStorage();
    }

    public void ClearMidiEventStorage()
    {
        Debug.Log("Clearing MIDI events storage");
        this.midiEvents.Clear();
    }

    public List<MidiEventStorage> GetMidiEvents()
    {
        // return mockEvents;
        return midiEvents;
    }

    void OnDestroy()
    {
        Debug.Log("Closing MIDI device.");
        inputDevice.Dispose();
    }

    void handleChannelMsg(object sender, ChannelMessageEventArgs e)
    {
        var keyNum = e.Message.Data1;

        Debug.Log(e.Message.Command.ToString() + '\t' + '\t' + e.Message.MidiChannel.ToString() + '\t' + keyNum.ToString() + '\t' + e.Message.Data2.ToString());
        if (e.Message.Command == ChannelCommand.NoteOn)
        {
            PianoBuilder.instance.ActivateKey(keyNum, Color.green);
            PianoBuilder.instance.SetParticleSystemStatusForKey(PianoKeys.GetKeyFor(keyNum), true);
            notesOn.Add(PianoKeys.GetKeyFor(keyNum));
        }
        else if (e.Message.Command == ChannelCommand.NoteOff)
        {
            PianoBuilder.instance.DeactivateKey(keyNum);
            PianoBuilder.instance.SetParticleSystemStatusForKey(PianoKeys.GetKeyFor(keyNum), false);
            notesOn.Remove(PianoKeys.GetKeyFor(keyNum));
        }
    }

    void storeMidiEvent(object sender, ChannelMessageEventArgs e)
    {
        if(seq.IsGamedStarted())
            midiEvents.Add(new MidiEventStorage(e, Time.time - seq.GetStartTime()));
    }

    public bool IsThisKeyOn(PianoKey key) {
        return notesOn.Contains(key);
    }

    public HashSet<PianoKey> GetOnKeys() {
        return notesOn;
    }

}

public struct MidiEventStorage
{
    public float time { get; }

    public int keyNum { get; }

    public bool isEnd { get; }

    public MidiEventStorage(ChannelMessageEventArgs e, float time)
    {
        this.time = time;
        this.keyNum = e.Message.Data1;
        this.isEnd = e.Message.Command == ChannelCommand.NoteOff;
    }

    public MidiEventStorage(int keyNum, bool isEnd, float time)
    {
        this.time = time;
        this.keyNum = keyNum;
        this.isEnd = isEnd;
    }
}


