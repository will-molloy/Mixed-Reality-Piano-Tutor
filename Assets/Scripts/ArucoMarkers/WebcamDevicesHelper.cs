using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebcamDevicesHelper : MonoBehaviour {

	void Start () 
	{
		var devices = WebCamTexture.devices;
		Debug.Log("Found " + devices.Length + " webcam device(s).");
		foreach (var d in devices)
		{
			Debug.Log("Found: " + d.name);
		}
	}

}
