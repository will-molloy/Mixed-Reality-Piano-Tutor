using System.Collections.Generic;
using UnityEngine;
using Sanford.Multimedia.Midi;

/// <summary>  
/// - Handles MIDI device 
/// </summary>  
public class MidiController : MonoBehaviour
{
    protected InputDevice inputDevice;

    private List<MidiEventStorage> midiEvents;

    void Start()
    {
        this.midiEvents = new List<MidiEventStorage>();

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
        return midiEvents;
    }

    void OnApplicationQuit()
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
        }
        else if (e.Message.Command == ChannelCommand.NoteOff)
        {
            PianoBuilder.instance.DeactivateKey(keyNum);
        }
    }

    void storeMidiEvent(object sender, ChannelMessageEventArgs e)
    {
        midiEvents.Add(new MidiEventStorage(e, Time.time));
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


