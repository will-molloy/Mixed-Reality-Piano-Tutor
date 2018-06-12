using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PianoBuilder : MonoBehaviour {

	[SerializeField]
	private GameObject whiteKey;
	[SerializeField]
	private GameObject blackKey;
	[SerializeField]
	private GameObject lockedText;

	public static readonly int CENTRE = (PianoKeys.GetLastKey().keyNum + PianoKeys.GetFirstKey().keyNum) / 2;

    internal Dictionary<PianoKey, GameObject> pianoKeys;

	internal static readonly float yOffset = 0.001f;
	// Whether the piano has been locked in placed by the user
	internal bool locked = false;
	// Whether the piano has been placed into the initial positon
	internal bool placed = false;
	internal GameObject lockedTextObj;

	public static PianoBuilder instance;

	// Use this for initialization

	public void PlacePianoInfrontOfTransform(Transform trf) {
		PlacePianoAt(trf.position + trf.forward * 0.5f);
	}

	public void PlacePianoAt(Vector3 location) {
		this.transform.position = location;
		var firstkey = PianoKeys.GetFirstKey();
		var lastkey = PianoKeys.GetLastKey();
		var whitekeyScale = whiteKey.transform.localScale;
		var blackkeyScale = blackKey.transform.localScale;
		var xOffset = 0f;
		var zOffset = whitekeyScale.z / 4;
		for(int i = firstkey.keyNum; i <= lastkey.keyNum; i++) {
			var currentKey = PianoKeys.GetKeyFor(i);
			GameObject keyObj;
			if(currentKey.color == KeyColor.White) {
				keyObj = Instantiate(whiteKey);
				keyObj.transform.SetParent(this.transform);
				keyObj.transform.localPosition = new Vector3(xOffset, 0f, 0);
				xOffset += whitekeyScale.x + PianoKeys.pianoKeyGap;
			} else {
				keyObj = Instantiate(blackKey);
				keyObj.transform.SetParent(this.transform);
				keyObj.transform.localPosition = new Vector3(xOffset + whitekeyScale.x / 2, yOffset, zOffset);
			}
			pianoKeys[currentKey] = keyObj;
		}
		placed = true;

	}
	void Start () {
		instance = this;
		pianoKeys = new Dictionary<PianoKey, GameObject>();
	}

    // Update is called once per frame
    void Update()
    {
		if(!placed) {
			return;
		}
		if (!locked) {
			var position = this.transform.position;
			var scale = this.transform.localScale;
			var angle = this.transform.localEulerAngles;
			if (Input.GetKey(KeyCode.A)) {
				position.x -= 0.001f;
			}
			if (Input.GetKey(KeyCode.D)) {
				position.x += 0.001f;
			}
			if (Input.GetKey(KeyCode.W)) {
				position.y += 0.001f;
			}
			if (Input.GetKey(KeyCode.S)) {
				position.y -= 0.001f;
			}
			if (Input.GetKey(KeyCode.Q)) {
				position.z += 0.001f;
			}
			if (Input.GetKey(KeyCode.E)) {
				position.z -= 0.001f;
			}
			if (Input.GetKey(KeyCode.Z)) {
				scale += new Vector3(0.001f, 0f, 0.001f);
			}
			if (Input.GetKey(KeyCode.X)) {
				scale -= new Vector3(0.001f, 0f, 0.001f);
			}
			if (Input.GetKey(KeyCode.C)) {
				angle += new Vector3(0f, 1f, 0f);
			}
			if (Input.GetKey(KeyCode.V)) {
				angle -= new Vector3(0f, 1f, 0f);
			}
			this.transform.position = position;
			this.transform.localScale = scale;
			this.transform.localEulerAngles = angle;
		}
		if (Input.GetKeyDown(KeyCode.Space)) {
			// Locking the piano in place
			if (lockedTextObj != null) {
				GameObject.Destroy(lockedTextObj);
			}
			if (locked) {
				// Unlock it
				locked = false;
			}
			else {
				// Lock it in place and set a text
				locked = true;
				lockedTextObj = Instantiate(lockedText);
				lockedTextObj.transform.SetParent(this.transform);
				lockedTextObj.transform.localPosition = pianoKeys[PianoKeys.GetKeyFor(CENTRE)].transform.localPosition + new Vector3(0f, 0.1f, 0f);
			}
		}

    }
}
