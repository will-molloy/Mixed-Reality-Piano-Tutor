using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sanford.Multimedia.Midi;
using Sanford.Multimedia;

public class CalibrationScript : MonoBehaviour {

	public static PianoKey left;
	public static Vector3 leftThumbPos;
	public static PianoKey right;
	public static Vector3 rightThumbPos;

	public bool inited = false;

	public GameObject BlackKey;
	public GameObject WhiteKey;

	// Use this for initialization:w

	void Start () {
			// Move VR camera
			var camera = GameObject.FindWithTag("MainCamera");
		camera.transform.rotation = new Quaternion(0,0,0,1);
	}
	
	// Update is called once per frame
	void Update () {
		
		if(left != null && right != null && !inited) {
            var horizontalDist = leftThumbPos.z - rightThumbPos.z;
            var delta = horizontalDist / (right.keyNum - left.keyNum);
			var camera = GameObject.FindWithTag("MainCamera");
			var leapSpace = GameObject.FindWithTag("LeapSpace");
			// Init keyboard
			Debug.Log("Left = " + left.keyNum + "Right = " + right.keyNum);
			for (var v = 0; v <= right.keyNum - left.keyNum; v++){
				var keyType = PianoKeys.GetKeyFor(v+left.keyNum).blackOrWhite;
				GameObject obj;
				Vector3 nextPos;
				if (keyType == BlackOrWhite.White){
					obj = Instantiate(WhiteKey);
                    nextPos = new Vector3(leftThumbPos.x, leftThumbPos.y, leftThumbPos.z + delta * v);
				} else {
					obj = Instantiate(BlackKey);
                    nextPos = new Vector3(leftThumbPos.x, leftThumbPos.y + 0.1f, leftThumbPos.z + delta * v);
				}
				obj.transform.position = nextPos;
			}
            // Map game space to real world
            Debug.Log("Left thumb = " + leftThumbPos + ", right thumb = " + rightThumbPos +
            	", camera = " + camera.transform.position + ", leap = " + leapSpace.transform.position);

			inited = true;
		}

		if (inited){
			// MIDI stuff
		}
	}

}
