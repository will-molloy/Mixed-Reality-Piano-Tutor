using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sanford.Multimedia;
using Sanford.Multimedia.Midi;
using System.Linq;

/// <summary>  
/// - Handles MIDI device 
/// </summary>  
public class MidiController : MonoBehaviour
{
    protected InputDevice inputDevice;

    void Start()
    {
        if (InputDevice.DeviceCount != 1)
        {
            Debug.LogError("Err: No device or too many devices found for MIDI input.");
            throw new System.Exception();
        }
        inputDevice = new InputDevice(0);
        inputDevice.ChannelMessageReceived += handleChannelMsg;

        inputDevice.StartRecording();
        Debug.Log("MIDI device inited");
    }

    void OnApplicationQuit()
    {
        Debug.Log("MIDI closed");
        inputDevice.Dispose();
    }

    void handleChannelMsg(object sender, ChannelMessageEventArgs e)
    {
        // Handle MIDI event
        Debug.Log(e.Message.Command.ToString() + '\t' + '\t' + e.Message.MidiChannel.ToString() + '\t' + e.Message.Data1.ToString() + '\t' + e.Message.Data2.ToString());
        if (e.Message.Command == ChannelCommand.NoteOn)
        {
            Piano.ActivateKey(e.Message.Data1);
        }
        else if (e.Message.Command == ChannelCommand.NoteOff)
        {
            Piano.DeactivateKey(e.Message.Data1);
        }
    }

}


