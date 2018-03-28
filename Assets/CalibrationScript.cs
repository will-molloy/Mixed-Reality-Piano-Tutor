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

	public GameObject keyPrefab;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(left != null && right != null && !inited) {
            var horizontalDist = leftThumbPos.z - rightThumbPos.z;
            var delta = horizontalDist / (right.keyNum - left.keyNum);
			Enumerable.Range(left.keyNum, right.keyNum).ToList().ForEach(v => {
				var obj = Instantiate(keyPrefab);
				var nextPos = new Vector3(leftThumbPos.x, leftThumbPos.y, leftThumbPos.z + delta * v);
				obj.transform.position = nextPos;
			}
			);
			inited = true;
		}
		
	}
}
