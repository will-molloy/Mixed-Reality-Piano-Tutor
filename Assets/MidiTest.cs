using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sanford.Multimedia;
using Sanford.Multimedia.Midi;
using Leap.Unity;
using Leap;
using Leap.Unity.Attributes;

[RequireComponent(typeof(LeapServiceProvider))]
public class MidiTest : MonoBehaviour
{
    protected InputDevice inputDevice;
    protected LeapServiceProvider provider;
    private Frame lastFrame;

    // Use this for initialization
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

        provider = GetComponent<LeapServiceProvider>();
        provider.OnUpdateFrame += OnUpdateFrame;
        provider.OnFixedFrame += OnFixedFrame;
    }

    void OnApplicationQuit()
    {
        Debug.Log("MIDI closed");
        inputDevice.Dispose();
    }

    protected virtual void OnUpdateFrame(Frame frame)
    {
        lastFrame = frame;
        foreach(var i in frame.Hands) {
            foreach(var j in i.Fingers) {
                Debug.DrawRay(j.TipPosition.ToVector3(),j.Direction.ToVector3(), Color.red);
            }
        }
    }
    protected virtual void OnFixedFrame(Frame frame)
    {

    }

    // Update is called once per frame
    void Update()
    {
    }

    void handleChannelMsg(object sender, ChannelMessageEventArgs e)
    {
        Debug.Log(e.Message.Command.ToString() + '\t' + '\t' + e.Message.MidiChannel.ToString() + '\t' + e.Message.Data1.ToString() + '\t' + e.Message.Data2.ToString());
        if (e.Message.Command == ChannelCommand.NoteOn && lastFrame != null)
        {
            if (CalibrationScript.leftKey == null)
            {
                CalibrationScript.leftKey = PianoKeys.GetKeyFor(e.Message.Data1);
                var finger = lastFrame.Hands[0].Fingers[0];
                CalibrationScript.leftThumbPos = GetThumbPos(lastFrame.Hands[0].Fingers);
                Debug.Log("Left thumb tip = " + CalibrationScript.leftThumbPos);

            }
            else if (CalibrationScript.rightKey == null)
            {
                CalibrationScript.rightKey = PianoKeys.GetKeyFor(e.Message.Data1);
            }
        }
    }

    private Vector3 GetThumbPos(List<Finger> fingers)
    {
        return GetThumb(fingers).TipPosition.ToVector3();
    }

    private Finger GetThumb(List<Finger> fingers)
    {
        foreach (var i in fingers)
        {
            if (i.Type == Finger.FingerType.TYPE_THUMB)
            {
                return i;
            }
        }
		Debug.LogError("Couldn't find thumb returning null.");
        return null;
    }
}


