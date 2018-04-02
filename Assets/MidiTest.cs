using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sanford.Multimedia;
using Sanford.Multimedia.Midi;
using Leap.Unity;
using Leap;
using Leap.Unity.Attributes;

[RequireComponent(typeof(LeapServiceProvider))]
public class MidiTest : MonoBehaviour {

	protected InputDevice inputDevice;
	protected LeapServiceProvider provider;
	private Frame lastFrame;

	// Use this for initialization
	void Start () {
		if(InputDevice.DeviceCount != 1) {
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

	void OnApplicationQuit(){
		Debug.Log("MIDI closed");
		inputDevice.Dispose();
	}

	protected virtual void OnUpdateFrame(Frame frame) {
		lastFrame = frame;
	}
	protected virtual void OnFixedFrame(Frame frame) {

	}
	
	// Update is called once per frame
	void Update () {
	}

	void handleChannelMsg(object sender, ChannelMessageEventArgs e) {
		Debug.Log(e.Message.Command.ToString() + '\t' + '\t' + e.Message.MidiChannel.ToString() + '\t' + e.Message.Data1.ToString() + '\t' + e.Message.Data2.ToString());
		var obj = GameObject.FindGameObjectWithTag("MainCamera");
		if(e.Message.Command == ChannelCommand.NoteOn && lastFrame != null) {
            if (CalibrationScript.left == null)
            {
                CalibrationScript.left = PianoKeys.GetKeyFor(e.Message.Data1);
				var fingers = lastFrame.Hands[0].Fingers;
				CalibrationScript.leftThumbPos= fingers[4].TipPosition.ToVector3();
				Debug.Log(CalibrationScript.leftThumbPos);
            }
            else if (CalibrationScript.right == null)
            {
                CalibrationScript.right = PianoKeys.GetKeyFor(e.Message.Data1);
				var fingers = lastFrame.Hands[0].Fingers;
				CalibrationScript.rightThumbPos = fingers[0].TipPosition.ToVector3();
            }
        }
	}
}
