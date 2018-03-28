using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sanford.Multimedia;
using Sanford.Multimedia.Midi;

public class MidiTest : MonoBehaviour {

	InputDevice inputDevice;


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
	}
	
	// Update is called once per frame
	void Update () {
	}

	void handleChannelMsg(object sender, ChannelMessageEventArgs e) {
        string a;
		a = e.Message.Command.ToString() + '\t' + '\t' + e.Message.MidiChannel.ToString() + '\t' + e.Message.Data1.ToString() + '\t' + e.Message.Data2.ToString();
		Debug.Log(a);
	}
}
