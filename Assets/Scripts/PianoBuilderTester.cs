using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PianoBuilder))]
[RequireComponent(typeof(Sequencer))]
public class PianoBuilderTester : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.Space)) {
			var obj = new GameObject();
			obj.transform.position = Vector3.zero;
			GetComponent<PianoBuilder>().BuildPianoAsChildOfTransform(obj.transform);
		}
		
	}
}
